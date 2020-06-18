using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IViewer.OtherWindow;
using IViewer.Properties;
using IViewer.ViewModel;
using Microsoft.Win32;

//////////////////////////////
// USE ONLY ENGLISH COMMENT //
//////////////////////////////

namespace IViewer {
  /// <summary>
  /// MainWindow.xaml Interaction Logic
  /// </summary>
  public partial class MainWindow : Window {
    public Thread t;
    public static TomlViewModel tomlViewModel;
    public static string TomlFileName = "test.toml";
    public MainWindow() {
      InitializeComponent();
      Focus();
      IdenticalScale = 72 / GetDPI();

      //初始化选项
      OriginalMode.IsChecked = true;
      SortByFileName.IsChecked = true;

      tomlViewModel = base.DataContext as TomlViewModel;

      //启动Watcher线程
      //tomlWatcher = base.DataContext as TomlWatcher;
      //t = new Thread(tomlWatcher.Run);
      //t.Start();
    }

    #region TopBarAnimation

    private bool _showTopBar;

    private bool ShowTopBar {
      get => _showTopBar;
      set {
        if (value == _showTopBar) {
          return;
        }

        _showTopBar = value;
        TopBar.BeginAnimation(MarginProperty,
          value ? AnimationDict.TopBarShowAnimation : AnimationDict.TopBarHideAnimation);
      }
    }

    #endregion

    #region Tool Functions

    private double GetDPI() {
      Matrix matrix;

      var source = PresentationSource.FromVisual(this);
      if (source?.CompositionTarget != null) {
        matrix = source.CompositionTarget.TransformToDevice;
      }
      else {
        using (var src = new HwndSource(new HwndSourceParameters())) {
          if (src.CompositionTarget != null) {
            matrix = src.CompositionTarget.TransformToDevice;
          }
          else {
            return 96;
          }
        }
      }

      return matrix.M11 * 96;
    }

    private double IdenticalScale;

    private void SwitchWindowState() {
      // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
      // Do nothing for other case
      switch (WindowState) {
        case WindowState.Normal:
          ResizeMode = ResizeMode.NoResize;
          WindowState = WindowState.Maximized;
          MaxButton.Content = "\xE923";
          break;
        case WindowState.Maximized:
          ResizeMode = ResizeMode.CanResize;
          WindowState = WindowState.Normal;
          MaxButton.Content = "\xE739";
          break;
      }

      // AdjustTransform();
    }

    #endregion

    #region Window Frame Control

    private void CloseWindow(object sender, RoutedEventArgs e) {
      //t.Abort();
      Application.Current.Shutdown();
    }

    private void MaxWindow(object sender, EventArgs e) {
      SwitchWindowState();
    }

    private void MinWindow(object sender, EventArgs e) {
      WindowState = WindowState.Minimized;
    }

    private void DragMoveWindow(object _, MouseButtonEventArgs e) {
      if (e.ButtonState != MouseButtonState.Pressed && WindowState != WindowState.Normal) {
        return;
      }

      DragMove();
    }

    #endregion

    #region Keyboard Handle

    private bool Shift;
    private bool Ctrl;
    private bool Alt;

    private void KeyDownHandler(object sender, KeyEventArgs e) {
      if (!IsActive) {
        return;
      }

      switch (e.Key) {
        case Key.LeftShift:
        case Key.RightShift:
          Shift = true;
          break;
        case Key.LeftCtrl:
        case Key.RightCtrl:
          Ctrl = true;
          break;
        case Key.LeftAlt:
        case Key.RightAlt:
          Alt = true;
          break;
      }
    }

    private void KeyUpHandler(object sender, KeyEventArgs e) {
      if (!IsActive) {
        return;
      }

      switch (e.Key) {
        case Key.LeftShift:
        case Key.RightShift:
          Shift = false;
          break;
        case Key.LeftCtrl:
        case Key.RightCtrl:
          Ctrl = false;
          break;
        case Key.LeftAlt:
        case Key.RightAlt:
          Alt = false;
          break;
        case Key.Escape:
          Close();
          break;
        case Key.Space:
          SwitchWindowState();
          break;
        case Key.OemPlus:
          CenterUniform();
          break;
        case Key.D0:
          CenterFit();
          break;
      }
    }

    #endregion

    #region ImgDrag

    // indicates if an image is opened.
    private bool imgLoad;

    // indicates if left mouse button is down
    private bool mouseDown;

    // drag effectiveness to the image
    private double dragMultiplier = 2;

    // drag start point
    private Point mouseBegin;

    // image translate offset
    private Point translateBegin;

    // unscaled image bound
    private Point imgBound;

    // displayed image bound
    private Point displayBound;

    private void CanvasMouseDown(object sender, MouseEventArgs e) {
      if (!imgLoad || !IsActive) {
        return;
      }

      mouseDown = true;
      mouseBegin = e.GetPosition(this);
      translateBegin = new Point(ImgTransform.Matrix.OffsetX, ImgTransform.Matrix.OffsetY);
    }

    private void CanvasMouseUp(object sender, EventArgs e) {
      if (!imgLoad || !IsActive) {
        return;
      }

      Mouse.Capture(null);
      mouseDown = false;
    }

    private void CanvasScrollHandler(object sender, MouseWheelEventArgs e) {
      if (!imgLoad || !IsActive) {
        return;
      }

      CanvasScroll(e.Delta > 0);
    }

    private void SizeChangeHandler(object sender, SizeChangedEventArgs e) {
      IdenticalScale = (image?.DpiX ?? 72) / GetDPI();
      if (!imgLoad || !IsActive) {
        return;
      }

      AdjustTransform();
    }

    private void CanvasMove(Point pos) {
      Mouse.Capture(ActionLayer);

      if (!mouseDown) {
        return;
      }

      var offset = translateBegin + ((pos - mouseBegin) * dragMultiplier);

      var newMat = ImgTransform.Matrix;

      if (offset.X <= 0 && offset.X + displayBound.X >= ImageLayer.ActualWidth) {
        newMat.OffsetX = offset.X;
      }

      if (offset.Y <= 0 && offset.Y + displayBound.Y >= ImageLayer.ActualHeight) {
        newMat.OffsetY = offset.Y;
      }

      UpdateTransform(newMat);
    }

    private void InitTransform() {
      var viewportWidth = ImageLayer.ActualWidth;
      var viewportHeight = ImageLayer.ActualHeight;

      var scale = Math.Min(viewportWidth / imgBound.X, viewportHeight / imgBound.Y);

      var matrix = Matrix.Identity;

      if (scale > IdenticalScale) {
        matrix.M11 = IdenticalScale;
        matrix.M22 = IdenticalScale;
        displayBound.X = imgBound.X * IdenticalScale;
        displayBound.Y = imgBound.Y * IdenticalScale;
      }
      else {
        matrix.M11 = scale;
        matrix.M22 = scale;
        displayBound.X = imgBound.X * scale;
        displayBound.Y = imgBound.Y * scale;
      }

      matrix.OffsetX = (viewportWidth - displayBound.X) / 2;
      matrix.OffsetY = (viewportHeight - displayBound.Y) / 2;

      ImgTransform.Matrix = matrix;
      oldMatrix = matrix;
    }

    private void AdjustTransform() {
      var viewportWidth = ImageLayer.ActualWidth;
      var viewportHeight = ImageLayer.ActualHeight;

      var newMat = ImgTransform.Matrix;

      if (viewportWidth > displayBound.X || viewportHeight > displayBound.Y) {
        newMat.OffsetX = (viewportWidth - displayBound.X) / 2;
        newMat.OffsetY = (viewportHeight - displayBound.Y) / 2;
      }
      else {
        if (newMat.OffsetX + displayBound.X < viewportWidth) {
          newMat.OffsetX = viewportWidth - displayBound.X;
        }

        if (newMat.OffsetY + displayBound.Y < viewportHeight) {
          newMat.OffsetY = viewportHeight - displayBound.Y;
        }
      }

      UpdateTransform(newMat);
    }

    private Matrix oldMatrix;

    private void Animation(double offsetX, double offsetY, double scale, bool instant = false) {
      // Somewhat confusing but use less variable.
      // oldMat: The old matrix in this animation
      // oldMatrix: The old matrix in next animation (that means, new in this animation)
      var oldMat = oldMatrix;
      oldMatrix = new Matrix(scale, 0, 0, scale, offsetX, offsetY);

      var animation = new MatrixAnimation(oldMat, oldMatrix,
        new Duration(TimeSpan.FromMilliseconds(instant ? 0 : 100))) {EasingFunction = new CubicEase()};

      ImgTransform.BeginAnimation(MatrixTransform.MatrixProperty, animation);
    }

    private void UpdateTransform(Matrix newMatrix) {
      ImgTransform.BeginAnimation(MatrixTransform.MatrixProperty, null);
      oldMatrix = newMatrix;
      ImgTransform.Matrix = newMatrix;
    }

    private void CenterUniform() {
      if (!imgLoad || !IsActive) {
        return;
      }

      displayBound.X = imgBound.X * IdenticalScale;
      displayBound.Y = imgBound.Y * IdenticalScale;

      Animation((ImageLayer.ActualWidth - displayBound.X) / 2, (ImageLayer.ActualHeight - displayBound.Y) / 2,
        IdenticalScale);
    }

    private void CenterFit() {
      if (!imgLoad || !IsActive) {
        return;
      }

      var viewportWidth = ImageLayer.ActualWidth;
      var viewportHeight = ImageLayer.ActualHeight;

      var scale = Math.Min(viewportWidth / imgBound.X, viewportHeight / imgBound.Y);

      displayBound.X = imgBound.X * scale;
      displayBound.Y = imgBound.Y * scale;

      Animation((ImageLayer.ActualWidth - displayBound.X) / 2, (ImageLayer.ActualHeight - displayBound.Y) / 2, scale);
    }

    private void CanvasScroll(bool up) {
      if (!imgLoad || !IsActive) {
        return;
      }

      double delta;
      if (Shift) {
        delta = 0.01 * IdenticalScale;
      }
      else if (Ctrl) {
        delta = IdenticalScale;
      }
      else {
        delta = 0.1 * IdenticalScale;
      }

      if (!up) {
        delta = -delta;
      }

      var scale = ImgTransform.Matrix.M11 + delta;
      if (scale < 0.01 * IdenticalScale) {
        return;
      }

      var viewportWidth = ImageLayer.ActualWidth;
      var viewportHeight = ImageLayer.ActualHeight;

      var mouse = Mouse.GetPosition(this);
      var offset = (new Point(ImgTransform.Matrix.OffsetX, ImgTransform.Matrix.OffsetY) - mouse) * scale /
                   ImgTransform.Matrix.M11 + mouse;
      displayBound.X = imgBound.X * scale;
      displayBound.Y = imgBound.Y * scale;

      if (offset.X > 0 || offset.X + displayBound.X < viewportWidth) {
        if (displayBound.X <= viewportWidth) {
          offset.X = (viewportWidth - displayBound.X) / 2;
        }
        else {
          offset.X = offset.X > 0 ? 0 : viewportWidth - displayBound.X;
        }
      }

      if (offset.Y > 0 || offset.Y + displayBound.Y < viewportHeight) {
        if (displayBound.Y <= viewportHeight) {
          offset.Y = (viewportHeight - displayBound.Y) / 2;
        }
        else {
          offset.Y = offset.Y > 0 ? 0 : viewportHeight - displayBound.Y;
        }
      }

      Animation(offset.X, offset.Y, scale);
    }

    #endregion

    private void MouseMoveHandler(object sender, MouseEventArgs e) {
      var pos = e.GetPosition(this);
      ShowTopBar = pos.Y < 60;

      if (mouseDown) {
        CanvasMove(pos);
      }
    }

    private void OpenFile(object sender, EventArgs e) {
      var dialog = new OpenFileDialog {
        RestoreDirectory = true,
        Filter = $"{Properties.Resources.Type_PNG} (*.png)|*.png|" +
                 $"{Properties.Resources.Type_HEIF} (*.heic)|*.heic|" +
                 $"{Properties.Resources.Type_Any} (*.*)|*.*",
        FilterIndex = 1
      };
      if (dialog.ShowDialog() != true) {
        return;
      }

      LoadImage(dialog.FileName);
    }

    private BitmapSource image;

    private void LoadImage(string dir) {
      // var d = File.ReadAllBytes(dir);
      // var bitmap = HeifDecoder.WBitmapFromBytes(d, GetDPI());
      // Pic.Source = bitmap;
      var bitmap = new BitmapImage();
      bitmap.BeginInit();
      bitmap.UriSource = new Uri(dir);
      bitmap.EndInit();

      image = bitmap;

      Img.Source = image;
      imgLoad = true;
      imgBound = new Point(image.Width, image.Height);
      IdenticalScale = image.DpiX / GetDPI();
      InitTransform();
    }

    private void OpenImg_OnClick(object sender, RoutedEventArgs e) {
      //Open Img
      OpenFile(sender, e);
    }

    private void CloseImg_OnClick(object sender, RoutedEventArgs e) {
      //Close Img
    }

    //排序选项函数 descent与其他选项的关系为可同时多选 其他选项直接为互斥单选
    //demo演示，暂不考虑代码简洁性
    private void SortByFileName_OnClick(object sender, RoutedEventArgs e) {
      //文件名排序
      //Sort By File Name
      (sender as MenuItem).IsChecked = true; //勾选当前选项
      //将其他两个选项取消
      SortByModifyDate.IsChecked = false;
      SortByFileSize.IsChecked = false;
    }

    private void SortByModifyDate_OnClick(object sender, RoutedEventArgs e) {
      //修改时间排序
      //Sort By Modify Date
      (sender as MenuItem).IsChecked = true; //勾选当前选项
      //将其他两个选项取消
      SortByFileName.IsChecked = false;
      SortByFileSize.IsChecked = false;
    }

    private void SortByFileSize_OnClick(object sender, RoutedEventArgs e) {
      //文件大小排序
      //Sort By Modify Date
      (sender as MenuItem).IsChecked = true; //勾选当前选项
      //将其他两个选项取消
      SortByFileName.IsChecked = false;
      SortByModifyDate.IsChecked = false;
    }

    private void DescendingSort_OnClick(object sender, RoutedEventArgs e) {
      //升\降序
      (sender as MenuItem).IsChecked = !(sender as MenuItem).IsChecked; //反转选项
      //根据 ischecked 的值更新排序
    }

    //浏览模式，前两个为互斥选项，后两个可多选？

    private void OriginalMode_OnClick(object sender, RoutedEventArgs e) {
      //原始大小（Radio）
      //Original Mode Img
      (sender as MenuItem).IsChecked = true;
      FitWindowMode.IsChecked = false;
    }

    private void FitWindowMode_OnClick(object sender, RoutedEventArgs e) {
      //适合窗口
      //Fit Window Img
      (sender as MenuItem).IsChecked = true;
      OriginalMode.IsChecked = false;
    }

    private void StretchingMode_OnClick(object sender, RoutedEventArgs e) {
      //Stretching Img
      (sender as MenuItem).IsChecked = !(sender as MenuItem).IsChecked;
    }

    private void CenterMode_OnClick(object sender, RoutedEventArgs e) {
      (sender as MenuItem).IsChecked = !(sender as MenuItem).IsChecked;
    }

    //杂项 选项，关于和退出
    private void ConfigItem_OnClick(object sender, RoutedEventArgs e) {
      //选项
      //tomlViewModel.Open();
      var x = new ConfigWindow(ref tomlViewModel);
      x.Show();
    }

  private void AboutItem_OnClick(object sender, RoutedEventArgs e) {//打开about窗口
      Window aboutWindow = new About();
      aboutWindow.Show();
    }

    private void ExitItem_OnClick(object sender, RoutedEventArgs e) {
      Application.Current.Shutdown();
    }

  }
}
