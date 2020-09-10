using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using ImageLibrary.Resizer;
using IViewer.Model;
using IViewer.SubWindow;
using MetadataExtractor;
using Microsoft.Win32;
using Matrix = System.Windows.Media.Matrix;
using ImageLibrary.Filter;

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
      DataContext = settings;
      Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.StringWindowBackgroundColor));
    }

    private readonly Settings settings = Settings.Instance;

    #region MouseMove

    private void MouseMoveHandler(object sender, MouseEventArgs e) {
      var pos = e.GetPosition(this);
      ShowTopBar = pos.Y < 60;

      if (fileInfo == EnumFileInfo.ShowOnHover) {
        ShowInfo = pos.X < 210;
      }

      if (mouseDown) {
        CanvasMove(point2Vector(pos));
      }
    }

    private void MouseLeaveHandler(object sender, MouseEventArgs e) {
      ShowTopBar = false;
      if (fileInfo == EnumFileInfo.ShowOnHover) {
        ShowInfo = false;
      }
    }

    #endregion

    #region Animations

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

    private bool _showInfo;
    private EnumFileInfo fileInfo;

    private bool ShowInfo {
      get => _showInfo;
      set {
        if (value == _showInfo) {
          return;
        }

        _showInfo = value;
        ImageInfo.BeginAnimation(OpacityProperty,
          value ? AnimationDict.FadeInAnimation : AnimationDict.FadeOutAnimation);
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

    private IResizer getResizer() {
      var modeEnlarge = (EnumImageEnlargingAlgorithm)settings.LongImageEnlargingAlgorithm;
      var modeShrink = (EnumImageShrinkingAlgorithm)settings.LongImageShrinkingAlgorithm;
      var modeDoubling = (EnumImageDoublingAlgorithm)settings.LongImageDoublingAlgorithm;

      IResizer resizerEnlarge;
      IResizer resizerShrink;
      IDoubler doubler;
      switch ((EnumImageEnlargingAlgorithm)settings.LongImageEnlargingAlgorithm) {
        case EnumImageEnlargingAlgorithm.NearestNeighbor:
          resizerEnlarge = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.NearestNeighbor);
          break;
        case EnumImageEnlargingAlgorithm.Bilinear:
          resizerEnlarge = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.Bilinear);
          break;
        case EnumImageEnlargingAlgorithm.Bicubic:
          resizerEnlarge = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.Bicubic);
          break;
        case EnumImageEnlargingAlgorithm.HighQualityBilinear:
          resizerEnlarge = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.HighQualityBilinear);
          break;
        case EnumImageEnlargingAlgorithm.HighQualityBicubic:
          resizerEnlarge = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.HighQualityBicubic);
          break;
        default:
          resizerEnlarge = new WPFResizer();
          break;
      }

      switch ((EnumImageShrinkingAlgorithm)settings.LongImageShrinkingAlgorithm) {
        case EnumImageShrinkingAlgorithm.NearestNeighbor:
          resizerShrink = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.NearestNeighbor);
          break;
        case EnumImageShrinkingAlgorithm.Bilinear:
          resizerShrink = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.Bilinear);
          break;
        case EnumImageShrinkingAlgorithm.Bicubic:
          resizerShrink = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.Bicubic);
          break;
        case EnumImageShrinkingAlgorithm.HighQualityBilinear:
          resizerShrink = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.HighQualityBilinear);
          break;
        case EnumImageShrinkingAlgorithm.HighQualityBicubic:
          resizerShrink = new OptionFixedResizer(GDIResizer.Resizer, InterpolationMode.HighQualityBicubic);
          break;
        default:
          resizerShrink = WPFResizer.Resizer;
          break;
      }

      switch ((EnumImageDoublingAlgorithm)settings.LongImageDoublingAlgorithm) {
        case EnumImageDoublingAlgorithm.Nnedi3:
          doubler = Nnedi3Doubler.Doubler;
          break;
        default:
          doubler = null;
          break;
      }

      return new AutoSwitchResizer(resizerEnlarge, resizerShrink, doubler);
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
        case Key.Left:
          TryLoadImage(true);
          break;
        case Key.Right:
          TryLoadImage();
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

    // drag effectiveness to the image
    private double dragMultiplier => settings.DoubleDragMultiplier;

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
      if (settings.LongAnimationSpan == 0) {
        UpdateTransform();
        return;
      }

      var realOldMatrix = oldMatrix;
      oldMatrix = new Matrix(displayScale, 0, 0, displayScale, displayOffset.X, displayOffset.Y);

      var animation = new MatrixAnimation(realOldMatrix, oldMatrix,
        new Duration(TimeSpan.FromMilliseconds(settings.LongAnimationSpan))) {
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
    private static int FilterLabel = -1;

    private void InstantUpdateImage() {
      // image area required to be computed (but don't go over original image bound)

      var outband = settings.DoubleExtendRenderRatio;

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
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      Debug.WriteLine("Update image: scale: finish");

      Debug.WriteLine("Update image: set: begin");

      Dispatcher.Invoke(() => {
        var inactive = ActiveImage;
        image1Active = !image1Active;
        ActiveImage.Source = newImage;
        intrinsicOffset = -iOffset;
        intrinsicScale = realScale;
        InternalUpdateTransform();
        switch (FilterLabel) {
          case 0: { rev_click();break; }
          case 1: { greyColor_click(); break; }
          case 2: { blackOrWhite_click(); break; }
          case 3: { deleteColor_click(); break; }
          case 4: { redOnly_click(); break; }
          case 5: { blueOnly_click(); break; }
          case 6: { greenOnly_click(); break; }
          case 7: { getOld_click(); break; }
          case 8: { colorFul_click(); break; }
          case 9: { relief_click(); break; }
          case 10: { cartoon_click(); break; }
          case 11: { sketch_click(); break; }
          case 12: { brightness_plus50_click(); break; }
          case 13: { brightness_plus30_click(); break; }
          case 14: { brightness_plus0_click(); break; }
          case 15: { brightness_delete30_click(); break; }
          case 16: { brightness_delete50_click(); break; }
          case 17: { contrast_plus50_click(); break; }
          case 18: { contrast_plus30_click(); break; }
          case 19: { contrast_plus0_click(); break; }
          case 20: { contrast_delete30_click(); break; }
          case 21: { contrast_delete50_click(); break; }
        }
        
        ActiveImage.Visibility = Visibility.Visible;
        inactive.Visibility = Visibility.Hidden;
        Debug.WriteLine(
          $"Active: {ActiveImage.Name}, {ActiveImage.Visibility}; Hidden: {inactive.Name}, {inactive.Visibility}");
      });
      Debug.WriteLine("Update image: set: end");
      Dispatcher.Invoke(() => Updating = false);
      Debug.WriteLine("Update image: end");
      //rev_click(sender, e);
      //rev_click();


    }

    private void UpdateImage() {
      GenMetadata();
      Task.Run(async delegate {
        // debounce user operation
        try {
          await Task.Delay(TimeSpan.FromSeconds(((double)settings.LongReRenderWaitTime) / 1000), imageUpdateCancellationTokenSource.Token);
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
      //rev_click();
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
    private void CanvasMouseUp(object sender, MouseEventArgs e) {
      if (!imgLoad || !IsActive) {
        return;
      }

      Mouse.Capture(null);
      mouseDown = false;
      UpdateTransform();
      //rev_click(sender, e);

      var pos = point2Vector(e.GetPosition(this));
      if ((pos - mouseBegin).Length < 1 && pos.Y > 60 && pos.X < 210) {
        OpenDetail();
      }
    }

    // Mouse scroll on canvas. Do scale.
    private void CanvasScrollHandler(object sender, MouseWheelEventArgs e) {
      if (!imgLoad || !IsActive || Updating) {
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
      //rev_click(sender, e);
    }

    #endregion

    #region Transform control

    private void InitTransform() {
      var scale = Math.Min(viewportWidth / realDimension.X, viewportHeight / realDimension.Y);

      // TODO Configurable init scale rule
      // Keep original size for small pictures but scale down for large ones
      realScale = Math.Min(scale, 1);
      intrinsicScale = realScale;

      sourceArea = new Int32Rect(0, 0, image.Width, image.Height);

      realOffset = new Vector(viewportWidth - virtualDimension.X, viewportHeight - virtualDimension.Y) / 2;
      intrinsicOffset = new Vector();

      Debug.WriteLine($"Init image: real:{realScale} intrinsic:{intrinsicScale} offset:{realOffset}.");

      ActiveImage.Source = image.GetFull(WPFResizer.Resizer, realScale);

      InternalUpdateTransform();
      GenMetadata();
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
        new Vector(viewportWidth - virtualDimension.X, viewportHeight - virtualDimension.Y) / 2;

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
      if (!mouseDown || Updating) {
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

      //rev_click();
    }

    #endregion

    #endregion

    #region File Handle

    private string currentImagePath = null;

    private readonly List<Tuple<string, string>> fileTypes = new List<Tuple<string, string>> {
      new Tuple<string, string>(Properties.Resources.Type_AVIF, "*.avif"),
      new Tuple<string, string>(Properties.Resources.Type_BMP, "*.bmp"),
      new Tuple<string, string>(Properties.Resources.Type_FLIF, "*.flif"),
      new Tuple<string, string>(Properties.Resources.Type_HEIF, "*.heic"),
      new Tuple<string, string>(Properties.Resources.Type_JPG, "*.jpg;*.jpeg;*.jpe;*.jfif"),
      new Tuple<string, string>(Properties.Resources.Type_TIF, "*.png"),
      new Tuple<string, string>(Properties.Resources.Type_WEBP, "*.webp"),
    };

    private void OpenFile(object sender, EventArgs e) {
      var allType = string.Join(";", fileTypes.Select(entry => entry.Item2));
      var dialog = new OpenFileDialog {
        RestoreDirectory = true,
        Filter = $"{Properties.Resources.Type_AnyImage} ({allType})|{allType}|"
                 + string.Join("|", fileTypes.Select(entry => $"{entry.Item1} ({entry.Item2})|{entry.Item2}"))
                 + $"|{Properties.Resources.Type_Any} (*.*)|*.*",
        FilterIndex = 1
      };
      if (dialog.ShowDialog() != true) {
        return;
      }

      currentImagePath = dialog.FileName;

      LoadImage(currentImagePath);
    }

    private ImageLibrary.Image image;
    private IResizer resizer;

    private void LoadImage(string dir) {
      image = ImageLibrary.Image.FromFile(dir);
      if (image == null) {
        MessageBox.Show(Properties.Resources.OpenImageFailMessage,
          Properties.Resources.OpenImageFailTitle,
          MessageBoxButton.OK,
          MessageBoxImage.Error);
        currentImagePath = null;
        return;
      }

      InitFileList();
      PostLoadImage();
    }

    private void PostLoadImage() {
      fileInfo = (EnumFileInfo)settings.LongFileInfo;
      switch (fileInfo) {
        case EnumFileInfo.Hide:
          ImageInfo.Opacity = 0;
          break;
        case EnumFileInfo.Show:
          ImageInfo.Opacity = 1;
          break;
      }

      if (fileInfo != EnumFileInfo.Hide) {
        PreGenMetadata();
        GenMetadata();
      }

      Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.StringWindowBackgroundColor));

      resizer = getResizer();

      ActiveImage.Source = image.GetFull(resizer);
      imgLoad = true;
      realDimension = new Vector(image.Width, image.Height);
      identicalScale = 96 / GetDPI();
      InitTransform();
    }

    private string metadataBasic = "";
    private string metadataExif = "";

    private void PreGenMetadata() {
      metadataBasic = image.Metadata.BasicData().
        Aggregate("", (current, item) => current + $"{Settings.Resource(item.Item1) ?? item.Item1}: {item.Item2}\n");

      metadataBasic += $"{Properties.Resources.Basic_Info_Image_Info}: {image.Width}x{image.Height}\n";

      if ((EnumEXIFInfo)settings.LongEXIFInfo == EnumEXIFInfo.Show) {
        metadataExif = "\n" + image.Metadata.ExifData().
          Aggregate("", (current, item) => current + $"{Settings.Resource(item.Item1) ?? item.Item1}: {item.Item2}\n");
      }
    }

    private void GenMetadata() {
      var metadata = metadataBasic;
      metadata += $"{Properties.Resources.Basic_Info_Image_Scale}: {realScale * 100:F2}%\n";

      if ((EnumEXIFInfo)settings.LongEXIFInfo == EnumEXIFInfo.Show) {
        metadata += metadataExif;
      }

      ImageInfo.Text = metadata;
    }

    private List<string> directoryFileList = new List<string>();
    private int currentOffset = 0;

    private void InitFileList() {
      if (currentImagePath == null) {
        directoryFileList = new List<string>();
        return;
      }

      directoryFileList = System.IO.Directory.GetParent(currentImagePath)
        .GetFiles()
        .OrderBy(s => s.Name)
        .Select(s => s.FullName)
        .ToList();

      currentOffset = directoryFileList.FindIndex(x => x.EndsWith(currentImagePath));
    }

    private void TryLoadImage(bool forward = false) {
      ImageLibrary.Image temp = null;
      while (temp == null) {
        if (forward) {
          currentOffset--;
          if (currentOffset == -1) {
            currentOffset += directoryFileList.Count;
          }
        }
        else {
          currentOffset++;
          if (currentOffset == directoryFileList.Count) {
            currentOffset = 0;
          }
        }

        temp = ImageLibrary.Image.FromFile(directoryFileList[currentOffset]);
      }

      image = temp;
      PostLoadImage();
    }

    private void HandleDragFile(object sender, DragEventArgs e) {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
        return;
      }

      string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (files != null && files.Length > 0) {
        currentImagePath = files[0];
        LoadImage(files[0]);
      }
    }

    private void CommandArgProcess(object sender, RoutedEventArgs e) {
      var args = Environment.GetCommandLineArgs();
      if (args.Length > 1) {
        currentImagePath = args[1];
        LoadImage(args[1]);
      }
    }

    private void OpenExternal(object sender, RoutedEventArgs e) {
      if (string.IsNullOrWhiteSpace(settings.StringImageEditorPath)) {
        return;
      }

      try {
        Process.Start(settings.StringImageEditorPath, $"\"{Path.GetFullPath(currentImagePath)}\"");
      }
      catch (Exception) {}
    }

    #endregion

    #region Sub Windows

    private void OpenSettings(object sender, RoutedEventArgs e) {
      settings.PauseNotify = true;
      var settingWindow = new SubWindow.ConfigWindow(settings);
      settingWindow.ShowDialog();
      settings.PauseNotify = false;
      Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.StringWindowBackgroundColor));
      if (imgLoad) {
        PostLoadImage();
      }
    }

    private void OpenAbout(object sender, RoutedEventArgs e) {
      var aboutWindow = new SubWindow.About();
      aboutWindow.ShowDialog();
    }

    private void OpenDetail() {
      var detail = image.Metadata.Directories
        .Select(directory => new KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>(
          directory.Name,
          directory.Tags
            .Select(tag => new KeyValuePair<string, string>(tag.Name, tag.Description))
            .Where(pair => !(pair.Key.Contains("Unknown") || pair.Value.Contains("Unknown")))
          ));

      var detailWindow = new MetadataWindow(detail);
      detailWindow.ShowDialog();
    }

    #endregion


    #region Fliter
    private WriteableBitmap src;
    private WriteableBitmap dst;
    private void rev_click() {
      
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var rev = new ImageLibrary.Filter.Rev();
      rev.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.Rev(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void rev_click(object sender, EventArgs e) {
      FilterLabel = 0;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var rev = new ImageLibrary.Filter.Rev();
      rev.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst),-1);
      //WPFilter.Rev(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void greyColor_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.GrayColor();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.grayColor(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void greyColor_click(object sender, EventArgs e) {
      FilterLabel = 1;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.GrayColor();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.grayColor(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void blackOrWhite_click(object sender, EventArgs e) {
      FilterLabel = 2;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.BlackOrWhite();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.blackOrWhite(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void blackOrWhite_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.BlackOrWhite();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.blackOrWhite(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void deleteColor_click(object sender, EventArgs e) {
      FilterLabel = 3;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.DeleteColor();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.deleteColor(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void deleteColor_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.DeleteColor();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.deleteColor(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void redOnly_click(object sender, EventArgs e) {
      FilterLabel = 4;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.RedOnly();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.redOnly(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void redOnly_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.RedOnly();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.redOnly(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void blueOnly_click(object sender, EventArgs e) {
      FilterLabel = 5;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.BlueOnly();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.blueOnly(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void blueOnly_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.BlueOnly();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.blueOnly(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void greenOnly_click(object sender, EventArgs e) {
      FilterLabel = 6;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.GreenOnly();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.greenOnly(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void greenOnly_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.GreenOnly();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.greenOnly(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void gauss_click(object sender, EventArgs e) {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      //WPFilter.gauss(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      var filter = new ImageLibrary.Filter.Gauss();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void getOld_click(object sender, EventArgs e) {
      FilterLabel = 7;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.GetOld();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.getOld(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void getOld_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.GetOld();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.getOld(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void colorFul_click(object sender, EventArgs e) {
      FilterLabel = 8;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.ColorFul();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.colorFul(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void colorFul_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.ColorFul();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.colorFul(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void relief_click(object sender, EventArgs e) {
      FilterLabel = 9;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Relief();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.relief(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void relief_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Relief();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.relief(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void cartoon_click(object sender, EventArgs e) {
      FilterLabel = 10;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Cartoon();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.cartoon(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void cartoon_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Cartoon();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.cartoon(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void sketch_click(object sender, EventArgs e) {
      FilterLabel = 11;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Sketch();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.sketch(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void sketch_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Sketch();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -1);
      //WPFilter.sketch(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_plus50_click(object sender, EventArgs e) {
      FilterLabel = 12;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0.5);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_plus50_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0.5);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_plus30_click(object sender, EventArgs e) {
      FilterLabel = 13;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0.3);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_plus30_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0.3);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_plus0_click(object sender, EventArgs e) {
      FilterLabel = 14;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_plus0_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_delete30_click(object sender, EventArgs e) {
      FilterLabel = 15;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -0.3);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), -0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_delete30_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -0.3);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), -0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_delete50_click(object sender, EventArgs e) {
      FilterLabel = 16;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -0.5);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), -0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void brightness_delete50_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Brightness();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -0.5);
      //WPFilter.brightness(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), -0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }

    private void contrast_plus50_click(object sender, EventArgs s) {
      FilterLabel = 17;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0.5);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void contrast_plus50_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0.5);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void contrast_plus30_click(object sender, EventArgs s) {
      FilterLabel = 18;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0.3);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void contrast_plus30_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0.3);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }

    private void contrast_plus0_click(object sender, EventArgs s) {
      FilterLabel = 19;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void contrast_plus0_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), 0);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), 0);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }

    private void contrast_delete30_click(object sender, EventArgs s) {
      FilterLabel = 20;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -0.3);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), -0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void contrast_delete30_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -0.3);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), -0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void contrast_delete50_click(object sender, EventArgs s) {
      FilterLabel = 21;
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -0.5);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), -0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }
    private void contrast_delete50_click() {
      var newImage = image.GetPartial(resizer, sourceArea, realScale);
      //src = new WriteableBitmap(new BitmapImage(new Uri("b5.jpg", UriKind.Relative)));
      src = new WriteableBitmap(newImage);

      // Maybe you need to change dst infos
      dst = ImageLibrary.Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
      8, src.BackBufferStride / src.PixelWidth);

      dst = new WriteableBitmap(src.PixelWidth, src.PixelHeight, 96, 96, PixelFormats.Bgr24, null);

      dst.Lock();
      var filter = new ImageLibrary.Filter.Contrast();
      filter.Filter(ImageLibrary.Misc.BitmapOfWritableBitmap(src), ImageLibrary.Misc.BitmapOfWritableBitmap(dst), -0.5);
      //WPFilter.Filter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst), -0.3);
      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      ActiveImage.Source = dst;
    }

    #endregion
  }
}