using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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


    private void Window_Loaded(object sender, RoutedEventArgs e) {
    }

    private static void ApplyFilter(Bitmap srcBp, Bitmap dstBp) {
      // TODO: Change to your filter logic
      Misc.CopyBitmap(srcBp, dstBp);
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      SettingTest.GetInstance().TestData = "TESTTESTUPDATE";
      SettingTest.GetInstance().Status = true;
    }

    private void Button_Click_1(object sender, RoutedEventArgs e) {
      var w = new SettingWindow {DataContext = SettingTest.GetInstance()};
      w.Show();
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
