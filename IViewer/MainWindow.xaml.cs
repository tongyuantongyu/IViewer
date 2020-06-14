using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using System.Windows.Threading;
using IViewer.Properties;
using Microsoft.Win32;

//////////////////////////////
// USE ONLY ENGLISH COMMENT //
//////////////////////////////

namespace IViewer {
  /// <summary>
  /// MainWindow.xaml Interaction Logic
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      Focus();
      identicalScale = 96 / GetDPI();
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
        TopBar.BeginAnimation(MarginProperty, value ? AnimationDict.TopBarShowAnimation : AnimationDict.TopBarHideAnimation);
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

    private double identicalScale;

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
    }

    #endregion

    #region Window Frame Control

    private void CloseWindow(object sender, RoutedEventArgs e) {
      Close();
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

    #region MouseMove

    private void MouseMoveHandler(object sender, MouseEventArgs e) {
      var pos = e.GetPosition(this);
      ShowTopBar = pos.Y < 60;

      if (mouseDown) {
        CanvasMove(pos);
      }
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

    #region ImgTransform

    #region Variables

    // indicates if an image is opened.
    private bool imgLoad;

    // auto trace and get active image
    private bool image1Active = true;
    private Image ActiveImage => image1Active ? Image1 : Image2;
    private MatrixTransform ActiveTransform => image1Active ? Transform1 : Transform2;

    // translate
    // indicates if left mouse button is down
    private bool mouseDown;
    // drag effectiveness to the image // TODO Configurable
    private double dragMultiplier = 2;
    // drag start point
    private Point mouseBegin;
    // image translate offset at drag start
    private Point translateBegin;
    // original image dimension
    private Point realDimension;
    // virtual displayed image dimension (not really have an image in this size, but we think there's one has such)
    private Point displayDimension;
    // offset of original image
    private Vector realOffset;
    // offset from original image to displayed image.
    private Vector intrinsicOffset;
    // offset of displayed image
    private Vector displayOffset => realOffset - intrinsicOffset;

    // scale
    // pixel level scale rate to original image. This value always use 96DPI
    private double realScale = 1;
    // scale rate from original image to displayed image. This value always use 96DPI
    private double intrinsicScale = 1;
    // scale rate of displayed image. This value use platform DPI
    private double displayScale => realScale / intrinsicScale;
    // scale worker cancel token
    private CancellationTokenSource scaleSource;
    // if resizer is computing
    private bool updating = false;

    private bool Updating {
      set {
        Debug.WriteLine($"Updating status: {updating}=>{value}");
        updating = value;
        LoadingIndicator.Visibility = value ? Visibility.Visible : Visibility.Hidden;
      }
      get => updating;
    }

    #endregion

    #region Transform Update Functions
    // These function are unaware of relation between displayed image and original image.
    // They focus on adjust the active display image (accessed via ActiveImage / ActiveTransform)

    private Matrix oldMatrix;

    private void AnimateTransform() {
      // Somewhat confusing but use less variable.
      // realOldMatrix: The old matrix in this animation
      // oldMatrix: The old matrix in next animation (that means, new in this animation)
      var realOldMatrix = oldMatrix;
      oldMatrix = new Matrix(displayScale, 0, 0, displayScale, displayOffset.X, displayOffset.Y);

      var animation = new MatrixAnimation(realOldMatrix, oldMatrix,
        new Duration(TimeSpan.FromMilliseconds(100))) { // TODO make animation duartion configurable
        EasingFunction = new CubicEase()
      };

      ActiveTransform.BeginAnimation(MatrixTransform.MatrixProperty, animation);

      scaleSource = new CancellationTokenSource();
      UpdateImage();
    }

    private void InternalUpdateTransform() {
      ActiveTransform.BeginAnimation(MatrixTransform.MatrixProperty, null);
      oldMatrix = new Matrix(displayScale, 0, 0, displayScale, displayOffset.X, displayOffset.Y);
      ActiveTransform.Matrix = oldMatrix;
    }

    private void UpdateTransform() {
      InternalUpdateTransform();

      scaleSource = new CancellationTokenSource();
      UpdateImage();
    }

    #endregion

    #region Event Handlers

    // Mouse down on canvas. Drag start.
    private void CanvasMouseDown(object sender, MouseEventArgs e) {
      if (!imgLoad || !IsActive) {
        return;
      }
      mouseDown = true;
      mouseBegin = e.GetPosition(this);
      translateBegin = new Point(ActiveTransform.Matrix.OffsetX, ActiveTransform.Matrix.OffsetY);
    }

    // Mouse move on canvas. Use unified mouse move handler so absent here.

    // Mouse up on canvas. Drag end.
    private void CanvasMouseUp(object sender, EventArgs e) {
      if (!imgLoad || !IsActive) {
        return;
      }

      Mouse.Capture(null);
      mouseDown = false;
      UpdateTransform();
    }

    // Mouse scroll on canvas. Do scale.
    private void CanvasScrollHandler(object sender, MouseWheelEventArgs e) {
      if (!imgLoad || !IsActive) {
        return;
      }

      CanvasScroll(e.Delta > 0);
    }

    // Window size change. Adjust image position on resize.
    private void SizeChangeHandler(object sender, SizeChangedEventArgs e) {
      identicalScale = 96 / GetDPI();
      if (!imgLoad || !IsActive) {
        return;
      }

      AdjustTransform();
    }

    #endregion

    private void CanvasMove(Point pos) {
      Mouse.Capture(ActionLayer);

      if (!mouseDown) {
        return;
      }

      var offset = translateBegin + ((pos - mouseBegin) * dragMultiplier);

      var newOffset = realOffset;

      if (offset.X <= 0 && offset.X + displayDimension.X >= ImageLayer.ActualWidth) {
        newOffset.X = offset.X;
      }

      if (offset.Y <= 0 && offset.Y + displayDimension.Y >= ImageLayer.ActualHeight) {
        newOffset.Y = offset.Y;
      }

      if (newOffset == realOffset) {
        return;
      }

      realOffset = newOffset;
      InternalUpdateTransform();
    }

    #region Transform control

    private Int32Rect sourceArea;

    private void InstantUpdateImage() {
      // necessary image area (but don't go over original image bound)

      // left up point: make sure one drag won't go out of rendered area.
      var iOffset = new Vector(
        Math.Max(-realOffset.X / realScale - 2 * ImageLayer.ActualWidth / realScale, 0),
        Math.Max(-realOffset.Y / realScale - 2 * ImageLayer.ActualHeight / realScale, 0));

      var targetArea = new Int32Rect(
        (int)iOffset.X, (int)iOffset.Y,
        Math.Min((int)(3 * ImageLayer.ActualWidth / realScale), image.Width),
        Math.Min((int)(3 * ImageLayer.ActualHeight / realScale), image.Height));
      // TODO 5: related to drag multiplier (n+1)

      // necessary area not changed. no need to update.
      if (sourceArea == targetArea) {
        return;
      }

      sourceArea = targetArea;

      Debug.WriteLine("Update image: begin");
      Dispatcher.Invoke(() => Updating = true, DispatcherPriority.Normal);
      Debug.WriteLine("Update image: scale: start");
      var newImage = image.GetPartial(sourceArea, realScale);
      Debug.WriteLine("Update image: scale: finish");

      Debug.WriteLine("Update image: set: begin");
      
      Dispatcher.Invoke(() => {
        var inactive = ActiveImage;
        image1Active = !image1Active;
        ActiveImage.Source = newImage;
        intrinsicOffset = -iOffset;
        intrinsicScale = realScale;
        InternalUpdateTransform();

        ActiveImage.Visibility = Visibility.Visible;
        inactive.Visibility = Visibility.Hidden;
        Debug.WriteLine($"Active: {ActiveImage.Name}, {ActiveImage.Visibility}; Hidden: {inactive.Name}, {inactive.Visibility}");
      }, DispatcherPriority.Normal);
      Debug.WriteLine("Update image: set: end");
      Dispatcher.Invoke(() => Updating = false, DispatcherPriority.Normal);
      Debug.WriteLine("Update image: end");
    }

    private void UpdateImage() {
      // we only resize a partition of image so resize on move is always needed.
      // if (Math.Abs(displayScale - currentScale) < 0.0001) {
      //   InternalUpdateTransform();
      //   return;
      // }

      Task.Run(async delegate {
        // debounce user operation
        try {
          await Task.Delay(TimeSpan.FromSeconds(0.5), scaleSource.Token);
        }
        catch (TaskCanceledException) {
          Debug.WriteLine("Update image: resize cancelled.");
          return;
        }

        Debug.WriteLine("Update image: time up. Start.");
        // if not cancelled, start scale compute. Transform is not allowed during computing
        InstantUpdateImage();

        Debug.WriteLine("Update image: Finished.");
      });
    }

    private void InitTransform() {
      var viewportWidth = ImageLayer.ActualWidth;
      var viewportHeight = ImageLayer.ActualHeight;

      var scale = Math.Min(viewportWidth / realDimension.X, viewportHeight / realDimension.Y);

      // TODO Configurable init scale rule
      // Keep original size for small pictures but scale down for large ones
      realScale = Math.Min(scale, identicalScale);
      intrinsicScale = realScale;

      sourceArea = new Int32Rect(0, 0, image.Width, image.Height);

      displayDimension.X = realDimension.X * realScale;
      displayDimension.Y = realDimension.Y * realScale;

      realOffset = new Vector(viewportWidth - displayDimension.X, viewportHeight - displayDimension.Y) / 2;
      intrinsicOffset = new Vector();

      Debug.WriteLine($"Init image: real:{realScale} intrinsic:{intrinsicScale} offset:{realOffset}.");

      ActiveImage.Source = image.GetFull(realScale);

      InternalUpdateTransform();
    }

    private void AdjustTransform() {
      var viewportWidth = ImageLayer.ActualWidth;
      var viewportHeight = ImageLayer.ActualHeight;

      var offset = realOffset;

      if (viewportWidth > displayDimension.X || viewportHeight > displayDimension.Y) {
        // center the image when image is smaller than view area
        Debug.WriteLine("Adjust image: small image, center.");
        offset.X = (viewportWidth - displayDimension.X) / 2;
        offset.Y = (viewportHeight - displayDimension.Y) / 2;
      }
      else {
        // image shifted too much. We can cover whole viewport but background is visible
        if (offset.X + displayDimension.X < viewportWidth) {
          offset.X = viewportWidth - displayDimension.X;
        }
        else if (offset.X > 0) {
          offset.X = 0;
        }
        if (offset.Y + displayDimension.Y < viewportHeight) {
          offset.Y = viewportHeight - displayDimension.Y;
        }
        else if (offset.Y > 0) {
          offset.Y = 0;
        }
      }
      Debug.WriteLine($"Adjust image: old: {realOffset} new: {offset}.");
      realOffset = offset;

      UpdateTransform();
    }

    private void CenterUniform() {
      if (!imgLoad || !IsActive) {
        return;
      }

      displayDimension.X = realDimension.X * identicalScale;
      displayDimension.Y = realDimension.Y * identicalScale;

      realOffset = new Vector(ImageLayer.ActualWidth - displayDimension.X, ImageLayer.ActualHeight - displayDimension.Y) / 2;

      realScale = 1;

      AnimateTransform();
    }

    #endregion

    private void CenterFit() {
      if (!imgLoad || !IsActive) {
        return;
      }

      var viewportWidth = ImageLayer.ActualWidth;
      var viewportHeight = ImageLayer.ActualHeight;

      var scale = Math.Min(viewportWidth / realDimension.X, viewportHeight / realDimension.Y);

      displayDimension.X = realDimension.X * scale;
      displayDimension.Y = realDimension.Y * scale;

      realOffset = new Vector(ImageLayer.ActualWidth - displayDimension.X, ImageLayer.ActualHeight - displayDimension.Y) / 2;

      realScale = scale / identicalScale;

      AnimateTransform();
    }

    private void CanvasScroll(bool up) {
      if (!imgLoad || !IsActive) {
        return;
      }

      double delta;
      if (Shift) {
        delta = 0.01;
      }
      else if (Ctrl) {
        delta = 1;
      }
      else {
        delta = 0.1;
      }

      if (!up) {
        delta = -delta;
      }

      ImageScale(delta);
    }

    private void ImageScale(double delta) {
      // Cancel pending resize calculate, if any.
      scaleSource?.Cancel();

      if (!imgLoad || !IsActive) {
        return;
      }

      var scale = realScale + delta;
      if (scale < 0.01) {
        return;
      }

      var viewportWidth = ImageLayer.ActualWidth;
      var viewportHeight = ImageLayer.ActualHeight;

      Debug.WriteLine($"Old offset: {realOffset}");
      var mouse = Mouse.GetPosition(this);
      var mouseVector = new Vector(mouse.X, mouse.Y);
      Debug.WriteLine($"Mouse position: {mouseVector}");
      var offset = (realOffset - mouseVector) * scale / realScale + mouseVector;
      displayDimension.X = realDimension.X * scale;
      displayDimension.Y = realDimension.Y * scale;
      Debug.WriteLine($"New offset: {offset}");

      // post-adjust image position
      if (offset.X > 0 || offset.X + displayDimension.X < viewportWidth) {
        Debug.WriteLine($"X need adjust. {offset.X}, {displayDimension.X} {viewportWidth}");
        if (displayDimension.X <= viewportWidth) {
          // small image. Force center.
          offset.X = (viewportWidth - displayDimension.X) / 2;
        }
        else {
          // big image. Scale at corner can't zoom at mouse point
          offset.X = offset.X > 0 ? 0 : viewportWidth - displayDimension.X;
        }
      }

      if (offset.Y > 0 || offset.Y + displayDimension.Y < viewportHeight) {
        Debug.WriteLine($"Y need adjust. {offset.Y}, {displayDimension.Y} {viewportHeight}");
        if (displayDimension.Y <= viewportHeight) {
          offset.Y = (viewportHeight - displayDimension.Y) / 2;
        }
        else {
          offset.Y = offset.Y > 0 ? 0 : viewportHeight - displayDimension.Y;
        }
      }

      realScale = scale;
      realOffset = offset;

      AnimateTransform();
    }

    #endregion

    #region File Handle

    private void OpenFile(object sender, EventArgs e) {
      var dialog = new OpenFileDialog {
        RestoreDirectory = true,
        Filter = $"{Properties.Resources.Type_WEBP} (*.webp)|*.webp|" +
                 $"{Properties.Resources.Type_PNG} (*.png)|*.png|" +
                 $"{Properties.Resources.Type_HEIF} (*.heic)|*.heic|" +
                 $"{Properties.Resources.Type_Any} (*.*)|*.*", FilterIndex = 1
      };
      if (dialog.ShowDialog() != true) {
        return;
      }

      LoadImage(dialog.FileName);
    }

    private ImageLibrary.Image image;

    private void LoadImage(string dir) {
      image = ImageLibrary.Image.FromFile(dir);

      ActiveImage.Source = image.GetFull();
      imgLoad = true;
      realDimension = new Point(image.Width, image.Height);
      identicalScale = 96 / GetDPI();
      InitTransform();
    }

    #endregion
  }
}
