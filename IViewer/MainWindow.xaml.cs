using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;

//////////////////////////////
// USE ONLY ENGLISH COMMENT //
//////////////////////////////

namespace IViewer {
  /// <summary>
  ///   MainWindow.xaml Interaction Logic
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      Focus();
      identicalScale = 96 / GetDPI();
    }

    private Settings settings = new Settings();

    #region MouseMove

    private void MouseMoveHandler(object sender, MouseEventArgs e) {
      var pos = e.GetPosition(this);
      ShowTopBar = pos.Y < 60;

      if (mouseDown) {
        CanvasMove(point2Vector(pos));
      }
    }

    #endregion

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

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    public bool IsForeground() {
      IntPtr windowHandle = new WindowInteropHelper(this).Handle;
      IntPtr foregroundWindow = GetForegroundWindow();
      return windowHandle == foregroundWindow;
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

    private static Vector point2Vector(Point p) {
      return new Vector(p.X, p.Y);
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

    private void SwitchPin(object _, EventArgs e) {
      Topmost = !Topmost;
      PinButton.Content = Topmost ? "\xe840" : "\xe718";
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
          if (IsForeground()) {
            Close();
          }
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

    #region Image Transform

    #region Variables

    // indicates if an image is opened.
    private bool imgLoad;

    // auto trace and get active image
    private bool image1Active = true;
    private Image ActiveImage => image1Active ? Image1 : Image2;
    private MatrixTransform ActiveTransform => image1Active ? Transform1 : Transform2;

    private double viewportWidth => ImageLayer.ActualWidth / identicalScale;
    private double viewportHeight => ImageLayer.ActualHeight / identicalScale;

    // translate
    // indicates if left mouse button is down
    private bool mouseDown;

    // drag effectiveness to the image // TODO Configurable
    private readonly double dragMultiplier = 2;

    // drag start point
    private Vector mouseBegin;

    // image translate offset at drag start
    private Vector translateBegin;

    // original image dimension
    private Vector realDimension;

    // virtual displayed image dimension (not really have an image in this size, but we think there's one has such)
    private Vector virtualDimension => realDimension * realScale;

    // offset of original image
    private Vector realOffset;

    // offset from original image to displayed image.
    private Vector intrinsicOffset;

    // offset of displayed image
    private Vector displayOffset => (realOffset - intrinsicOffset * realScale) * identicalScale;

    // scale
    // pixel level scale rate to original image. This value always use 96DPI
    private double realScale = 1;

    // scale rate from original image to displayed image. This value always use 96DPI
    private double intrinsicScale = 1;

    // scale rate of displayed image. This value use platform DPI
    private double displayScale => realScale / intrinsicScale * identicalScale;

    // scale worker cancel token
    private CancellationTokenSource imageUpdateCancellationTokenSource;

    // if resizer is computing
    private bool updating;

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
        new Duration(TimeSpan.FromMilliseconds(100))) {
        // TODO make animation duartion configurable
        EasingFunction = new CubicEase()
      };

      ActiveTransform.BeginAnimation(MatrixTransform.MatrixProperty, animation);

      imageUpdateCancellationTokenSource = new CancellationTokenSource();
      UpdateImage();
    }

    private void InternalUpdateTransform() {
      ActiveTransform.BeginAnimation(MatrixTransform.MatrixProperty, null);
      oldMatrix = new Matrix(displayScale, 0, 0, displayScale, displayOffset.X, displayOffset.Y);
      ActiveTransform.Matrix = oldMatrix;
    }

    private void UpdateTransform() {
      InternalUpdateTransform();

      imageUpdateCancellationTokenSource = new CancellationTokenSource();
      UpdateImage();
    }

    #endregion

    #region Image Update Functions

    private Int32Rect sourceArea;

    private void InstantUpdateImage() {
      // image area required to be computed (but don't go over original image bound)

      // TODO customizable
      const double outband = 1;

      var iOffset = new Vector(
        Math.Max(-realOffset.X / realScale - outband * viewportWidth / realScale, 0),
        Math.Max(-realOffset.Y / realScale - outband * viewportHeight / realScale, 0));

      var targetArea = new Int32Rect(
        (int)iOffset.X, (int)iOffset.Y,
        Math.Min((int)((1 + 2 * outband) * viewportWidth / realScale), image.Width),
        Math.Min((int)((1 + 2 * outband) * viewportHeight / realScale), image.Height));

      // required area not changed. no need to update.
      if (sourceArea == targetArea && Math.Abs(intrinsicScale - realScale) < 0.0001) {
        return;
      }

      sourceArea = targetArea;

      Debug.WriteLine("Update image: begin");
      Dispatcher.Invoke(() => Updating = true);
      Debug.WriteLine($"Update image: scale: area: {sourceArea}, scale: {realScale}");
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
        Debug.WriteLine(
          $"Active: {ActiveImage.Name}, {ActiveImage.Visibility}; Hidden: {inactive.Name}, {inactive.Visibility}");
      });
      Debug.WriteLine("Update image: set: end");
      Dispatcher.Invoke(() => Updating = false);
      Debug.WriteLine("Update image: end");
    }

    private void UpdateImage() {
      Task.Run(async delegate {
        // debounce user operation
        try {
          // TODO: changable
          await Task.Delay(TimeSpan.FromSeconds(0.5), imageUpdateCancellationTokenSource.Token);
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

    #endregion

    #region Event Handlers

    // Mouse down on canvas. Drag start.
    private void CanvasMouseDown(object sender, MouseEventArgs e) {
      if (!imgLoad || !IsActive) {
        return;
      }

      mouseDown = true;
      mouseBegin = point2Vector(e.GetPosition(this));
      translateBegin = realOffset;
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

    #region Transform control

    private void InitTransform() {
      var scale = Math.Min(viewportWidth / realDimension.X, viewportHeight / realDimension.Y);

      // TODO Configurable init scale rule
      // Keep original size for small pictures but scale down for large ones
      realScale = Math.Min(scale, identicalScale);
      intrinsicScale = realScale;

      sourceArea = new Int32Rect(0, 0, image.Width, image.Height);

      realOffset = new Vector(viewportWidth - virtualDimension.X, viewportHeight - virtualDimension.Y) / 2;
      intrinsicOffset = new Vector();

      Debug.WriteLine($"Init image: real:{realScale} intrinsic:{intrinsicScale} offset:{realOffset}.");

      ActiveImage.Source = image.GetFull(realScale);

      InternalUpdateTransform();
    }

    private void InternalAdjustTransform() {
      var offset = realOffset;

      Debug.WriteLine("Adjust image: check X.");
      if (viewportWidth > virtualDimension.X) {
        Debug.WriteLine("Adjust image: small, center.");
        offset.X = (viewportWidth - virtualDimension.X) / 2;
      }
      else if (offset.X + virtualDimension.X < viewportWidth) {
        Debug.WriteLine("Adjust image: big, X right overflow.");
        offset.X = viewportWidth - virtualDimension.X;
      }
      else if (offset.X > 0) {
        Debug.WriteLine("Adjust image: big, X left overflow.");
        offset.X = 0;
      }

      Debug.WriteLine("Adjust image: check Y.");
      if (viewportHeight > virtualDimension.Y) {
        Debug.WriteLine("Adjust image: small, center.");
        offset.Y = (viewportHeight - virtualDimension.Y) / 2;
      }
      else if (offset.Y + virtualDimension.Y < viewportHeight) {
        Debug.WriteLine("Adjust image: big, Y bottom overflow.");
        offset.Y = viewportHeight - virtualDimension.Y;
      }
      else if (offset.Y > 0) {
        Debug.WriteLine("Adjust image: big, Y top overflow.");
        offset.Y = 0;
      }

      Debug.WriteLine($"Adjust image: old: {realOffset} new: {offset}.");
      realOffset = offset;
    }

    private void AdjustTransform() {
      InternalAdjustTransform();
      UpdateTransform();
    }

    private void CenterUniform() {
      if (!imgLoad || !IsActive) {
        return;
      }

      realScale = 1;
      realOffset =
        new Vector(ImageLayer.ActualWidth - virtualDimension.X, ImageLayer.ActualHeight - virtualDimension.Y) / 2;

      AnimateTransform();
    }

    private void CenterFit() {
      if (!imgLoad || !IsActive) {
        return;
      }

      realScale = Math.Min(viewportWidth / realDimension.X, viewportHeight / realDimension.Y);
      realOffset = new Vector(viewportWidth - virtualDimension.X, viewportHeight - virtualDimension.Y) / 2;

      AnimateTransform();
    }

    
    private void CanvasMove(Vector pos) {
      if (!mouseDown) {
        return;
      }

      Mouse.Capture(ActionLayer);

      var offset = translateBegin + ((pos - mouseBegin) * dragMultiplier);

      var newOffset = realOffset;

      if (offset.X <= 0 && offset.X + virtualDimension.X >= viewportWidth) {
        newOffset.X = offset.X;
      }

      if (offset.Y <= 0 && offset.Y + virtualDimension.Y >= viewportHeight) {
        newOffset.Y = offset.Y;
      }

      if (newOffset == realOffset) {
        return;
      }

      realOffset = newOffset;
      InternalUpdateTransform();
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
      imageUpdateCancellationTokenSource?.Cancel();

      if (!imgLoad || !IsActive) {
        return;
      }

      var newScale = realScale + delta;
      if (newScale < 0.01) {
        return;
      }

      Debug.WriteLine($"Old offset: {realOffset}");
      var mouse = point2Vector(Mouse.GetPosition(this));
      Debug.WriteLine($"Mouse position: {mouse}");
      var offset = (realOffset - mouse) * newScale / realScale + mouse;
      Debug.WriteLine($"New offset: {offset}");

      realScale = newScale;
      realOffset = offset;

      InternalAdjustTransform();

      AnimateTransform();

      Debug.WriteLine($"Current scale: {realScale}");
    }

    #endregion

    #endregion

    #region File Handle

    private void OpenFile(object sender, EventArgs e) {
      var dialog = new OpenFileDialog {
        RestoreDirectory = true,
        Filter = $"{Properties.Resources.Type_WEBP} (*.webp)|*.webp|" +
                 $"{Properties.Resources.Type_PNG} (*.png)|*.png|" +
                 $"{Properties.Resources.Type_HEIF} (*.heic)|*.heic|" +
                 $"{Properties.Resources.Type_Any} (*.*)|*.*",
        FilterIndex = 1
      };
      if (dialog.ShowDialog() != true) {
        return;
      }

      LoadImage(dialog.FileName);
    }

    private ImageLibrary.Image image;

    private void LoadImage(string dir) {
      image = ImageLibrary.Image.FromFile(dir);

      ActiveImage.Source = image.GetFull(ImageLibrary.Resizer.WPFResizer.Resizer);
      imgLoad = true;
      realDimension = new Vector(image.Width, image.Height);
      identicalScale = 96 / GetDPI();
      InitTransform();
    }

    #endregion
  }
}