using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
      DataContext = SettingTest.GetInstance();
    }

    private WriteableBitmap w;
    private WriteableBitmap o;
    private void Window_Loaded(object sender, RoutedEventArgs e) {
      w = new WriteableBitmap(new BitmapImage(new Uri("Yosemite.jpg", UriKind.Relative)));
      Console.WriteLine("img load");
      Img.Source = w;
      Scale.ScaleX = w.DpiX / 120;
      Scale.ScaleY = w.DpiY / 120;
    }

    [DllImport("libnnedi3.dll", EntryPoint = "DoubleImage", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool DoubleImage(ref Bitmap src, ref Bitmap dst);

    private void Button_Click(object sender, RoutedEventArgs e) {
      // var b = Misc.BitmapOfWritableBitmap(w);
      // for (int i = 0; i < 50; i++) {
      //   Console.Write(Marshal.ReadByte(b.Scan0, i));
      //   Console.Write(" ");
      // }
      // Console.WriteLine();
      // o = Misc.AllocWriteableBitmap(2 * w.PixelWidth, 2 * w.PixelHeight, 8, w.BackBufferStride / w.PixelWidth);
      // o.Lock();
      // var ou = Misc.BitmapOfWritableBitmap(o);
      // Console.WriteLine("mem alloc");
      // for (int i = 0; i < 50; i++) {
      //   Console.Write(Marshal.ReadByte(ou.Scan0, i));
      //   Console.Write(" ");
      // }
      // Console.WriteLine();
      // var r = DoubleImage(ref b, ref ou);
      // Console.WriteLine($"enlarge finish with {r}");
      // for (int i = 0; i < 50; i++) {
      //   Console.Write(Marshal.ReadByte(ou.Scan0, i));
      //   Console.Write(" ");
      // }
      // Console.WriteLine();
      // o.AddDirtyRect(new Int32Rect(0, 0, o.PixelWidth, o.PixelHeight));
      // o.Unlock();
      // Console.WriteLine($"{o.PixelWidth} {o.PixelHeight}");
      // Img.Source = o;
      // Scale.ScaleX = o.DpiX / 120;
      // Scale.ScaleY = o.DpiY / 120;
      // Topmost = true;
      var m = MetadataExtractor.ImageMetadataReader.ReadMetadata("background.png");
      Console.WriteLine(m);
    }
  }

  public class SettingTest : INotifyPropertyChanged {
    private static SettingTest instance;

    public static SettingTest GetInstance() {
      return instance ?? (instance = new SettingTest());
    }
    private string data = "TESTTEST";
    private bool status = false;

    public string TestData {
      get => data;
      set {
        data = value;
        OnPropertyChanged(nameof(TestData));
      }
    }

    public bool Status {
      get => status;
      set {
        Debug.WriteLine("Changed.");
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
