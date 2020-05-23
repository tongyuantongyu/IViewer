using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ImageDecoder.Heif;

namespace WpfApp1 {
  /// <summary>
  ///   MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow {
    //顶栏动画
    private static readonly ThicknessAnimationUsingKeyFrames TopBarShowAnimation = new ThicknessAnimationUsingKeyFrames {
      KeyFrames = new ThicknessKeyFrameCollection {
        new EasingThicknessKeyFrame(new Thickness(0, -30, 0, 0), KeyTime.FromTimeSpan(TimeSpan.Zero),
          new CubicEase()),
        new EasingThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150)),
          new CubicEase())
      }
    };

    private static readonly ThicknessAnimationUsingKeyFrames TopBarHideAnimation = new ThicknessAnimationUsingKeyFrames {
      KeyFrames = new ThicknessKeyFrameCollection {
        new EasingThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)),
          new CubicEase()),
        new EasingThicknessKeyFrame(new Thickness(0, -30, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(650)),
          new CubicEase())
      }
    };
    private void TopBarAnimation(bool show) {
      TopBar.BeginAnimation(MarginProperty, show ? TopBarShowAnimation : TopBarHideAnimation);
    }
    //菜单项动画
    private static readonly ThicknessAnimationUsingKeyFrames MenuItemShowAnimation = new ThicknessAnimationUsingKeyFrames {
      KeyFrames = new ThicknessKeyFrameCollection {
        new EasingThicknessKeyFrame(new Thickness(0, -45, 0, 0), KeyTime.FromTimeSpan(TimeSpan.Zero),
          new CubicEase()),
        new EasingThicknessKeyFrame(new Thickness(0, 30, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150)),
          new CubicEase())
      }
    };

    private static readonly ThicknessAnimationUsingKeyFrames MenuItemHideAnimation = new ThicknessAnimationUsingKeyFrames {
      KeyFrames = new ThicknessKeyFrameCollection {
        new EasingThicknessKeyFrame(new Thickness(0, 30, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400)),
          new CubicEase()),
        new EasingThicknessKeyFrame(new Thickness(0, -45, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(450)),
          new CubicEase())
      }
    };
    private void MenuItemAnimation(bool show) {
      OpenFileButton.BeginAnimation(MarginProperty, show ? MenuItemShowAnimation : MenuItemHideAnimation);
    }
    //动画相关布尔量
    private bool showTopbar;
    private bool showMenu;

    public MainWindow() {
      InitializeComponent();
      //初始化
      showTopbar = false;
      showMenu = false;
    }

    private void MainWindow1_MouseMove(object sender, MouseEventArgs e) {
      var pos = e.GetPosition(this);
      if (showTopbar == (pos.Y < 60)) {
        return;
      }
      showTopbar = !showTopbar;
      TopBarAnimation(showTopbar);
      //菜单隐藏时将菜单项一起隐藏
      if (showTopbar == false) {
        showMenu = false;
        MenuItemAnimation(showMenu);
      }
    }

    private void SwitchWindowState() {
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
    }

    private double GetDPI() {
      Matrix matrix;
      var source = PresentationSource.FromVisual(this);
      if (source != null) {
        matrix = source.CompositionTarget.TransformToDevice;
      }
      else {
        using (var src = new HwndSource(new HwndSourceParameters())) {
          matrix = src.CompositionTarget.TransformToDevice;
        }
      }

      return matrix.M11 * 96;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      //var d = File.ReadAllBytes("trans.heic");
      //var dpi = GetDPI();
      //var bitmap = HeifDecoder.WBitmapFromBytes(d, dpi);
      //Pic.Source = bitmap;
    }

    private void Window_KeyUp(object sender, KeyEventArgs e) {
      switch (e.Key) {
        case Key.Escape:
          Close();
          break;
        case Key.Space:
          SwitchWindowState();
          break;
      }
    }


    private void Close_Click(object sender, RoutedEventArgs e) {
      Close();
    }

    private void Max_Click(object sender, RoutedEventArgs e) {
      SwitchWindowState();
    }

    private void Min_Click(object sender, RoutedEventArgs e) {
      WindowState = WindowState.Minimized;
    }

    private void GripMove(object sender, MouseButtonEventArgs e) {
      if (e.ButtonState == MouseButtonState.Pressed) {
        DragMove();
      }
    }

    private void Grip_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      SwitchWindowState();
    }

    private void Grip_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      if (e.ButtonState == MouseButtonState.Pressed && WindowState == WindowState.Normal) {
        DragMove();
      }
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e) {//打开文件
      Microsoft.Win32.OpenFileDialog op = new Microsoft.Win32.OpenFileDialog();
      op.InitialDirectory = @"E:\C sharp\IViewer\ImageTest\WpfApp1\bin\x64\Debug";//初始目录
      op.RestoreDirectory = true;
      op.Filter = "heic图片(*.heic)|*.heic|所有文件(*.*)|*.*";//文件类型选项
      op.FilterIndex = 1;//默认为第一项
      if (op.ShowDialog()==true)
      {
        //获取文件名
        string FileName = op.FileName;
        OpenImgFile(FileName);
      }
    }

    private void OpenImgFile(string FileName) {
      var d = File.ReadAllBytes(FileName);
      var dpi = GetDPI();
      var bitmap = HeifDecoder.WBitmapFromBytes(d, dpi);
      Pic.Source = bitmap;
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e) {//点击菜单键弹出功能按钮
      showMenu = !showMenu;
      MenuItemAnimation(showMenu);
    }
    //图片拖拽相关
    private bool isMouseLeftButtonDown = false;
    Point previousMousePoint = new Point(0, 0);
    private void img_MouseDown(object sender, MouseButtonEventArgs e) {
      isMouseLeftButtonDown = true;
      previousMousePoint = e.GetPosition(Pic);
    }

    private void img_MouseUp(object sender, MouseButtonEventArgs e) {
      isMouseLeftButtonDown = false;
    }

    private void img_MouseLeave(object sender, MouseEventArgs e) {
      isMouseLeftButtonDown = false;
    }

    private void img_MouseMove(object sender, MouseEventArgs e) {
      if (isMouseLeftButtonDown == true) {
        Point position = e.GetPosition(Pic);
        PicTranslateTransform.X += position.X - this.previousMousePoint.X;
        PicTranslateTransform.Y += position.Y - this.previousMousePoint.Y;
      }
    }

    private void img_MouseWheel(object sender, MouseWheelEventArgs e) {//滚轮缩放事件
      Point centerPoint = e.GetPosition(Pic);

      double val = (double)e.Delta / 2000;   //描述鼠标滑轮滚动
      if (PicScaleTransform.ScaleX < 0.3 && PicScaleTransform.ScaleY < 0.3 && e.Delta < 0) {
        return;
      }
      PicScaleTransform.CenterX = centerPoint.X;
      PicScaleTransform.CenterY = centerPoint.Y;

      PicScaleTransform.ScaleX += val;
      PicScaleTransform.ScaleY += val;
    }
  }
 
  // public class Bindable<T> : INotifyPropertyChanged {
  //   public event PropertyChangedEventHandler PropertyChanged;
  //   private T data;
  //   public T Data {
  //     get => data;
  //     set {
  //       if (data.Equals(value)) {
  //         return;
  //       }
  //       data = value;
  //       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Data"));
  //     }
  //   }
  //   public Bindable(T value) {
  //     data = value;
  //   }
  //
  //   public Bindable() { }
  // }
}