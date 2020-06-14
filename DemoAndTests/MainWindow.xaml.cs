using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoAndTests {
  /// <summary>
  /// MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      var scale = GetScale();
      Scale.ScaleX = scale;
      Scale.ScaleY = scale;
    }

    private double GetScale() {
      Matrix matrix;

      var source = PresentationSource.FromVisual(this);
      if (source?.CompositionTarget != null) {
        matrix = source.CompositionTarget.TransformToDevice;
      }
      else {
        using (var s = new HwndSource(new HwndSourceParameters())) {
          if (s.CompositionTarget != null) {
            matrix = s.CompositionTarget.TransformToDevice;
          }
          else {
            return 1;
          }
        }
      }

      return 1.0 / matrix.M11;
    }

    private WriteableBitmap src;
    private WriteableBitmap dst;

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      src = new WriteableBitmap(new BitmapImage(new Uri("background.png", UriKind.Relative)));

      // Maybe you need to change dst infos
      dst = Misc.AllocWriteableBitmap(src.PixelWidth, src.PixelHeight,
        8, src.BackBufferStride / src.PixelWidth);

      dst.Lock();

      ApplyFilter(Misc.BitmapOfWritableBitmap(src), Misc.BitmapOfWritableBitmap(dst));

      dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));

      dst.Unlock();

      Img.Source = dst;
    }

    private static void ApplyFilter(Bitmap srcBp, Bitmap dstBp) {
      // TODO: Change to your filter logic
      Misc.CopyBitmap(srcBp, dstBp);
    }
  }
}
