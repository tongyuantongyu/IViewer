using System;
using System.Collections.Generic;
using System.IO;
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
using ImageDecoder.Avif;
using ImageDecoder.Bmp;
using ImageDecoder.Flif;
using ImageDecoder.Heif;
using ImageDecoder.Webp;

namespace WpfApp1 {
  /// <summary>
  /// MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
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
      var d = File.ReadAllBytes("65261833.heic");
      var dpi = GetDPI();
      var bitmap = HeifDecoder.WBitmapFromBytes(d, dpi);
      Pic.Source = bitmap;
    }

    private void Window_KeyUp(object sender, KeyEventArgs e) {
      switch (e.Key) {
        case Key.Escape:
          Close();
          break;
        case Key.Space:
          switch (WindowState) {
            case WindowState.Normal:
              WindowState = WindowState.Maximized;
              break;
            case WindowState.Maximized:
              WindowState = WindowState.Normal;
              break;
          }

          break;
      }
    }
  }
}
