using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IViewer.OtherWindow {
  /// <summary>
  /// ConfigWindow.xaml 的交互逻辑
  /// </summary>
  public partial class ConfigWindow : Window {
    public ConfigWindow(ref TomlWatcher tw) {
      InitializeComponent();
      base.DataContext = tw;
    }

    private void Window_Closed(object sender, EventArgs e) {
      //写入配置
      //TomlWatcher tomlWatcher = base.DataContext as TomlWatcher;
      //tomlWatcher.W();
      //重启应用
      //this.Dispatcher.Invoke((ThreadStart)delegate ()
      //  {
      //    Application.Current.Shutdown();
      //  }
      //);
      //System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }

    public void RevertBoolContent(object sender) {//将文本置换
      Button button = sender as Button;
      bool now = bool.Parse(button.Content.ToString());
      button.Content = (!now).ToString();
    }
    private void ButtonAllowMultipleInstanceRunning_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlWatcher.IsAllowMultipleInstanceRunning = ButtonAllowMultipleInstanceRunning.Content.ToString();
    }

    private void ButtonConfirmBeforeDeleteFile_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlWatcher.IsConfirmBeforeDeleteFile = ButtonConfirmBeforeDeleteFile.Content.ToString();
    }

    private void ButtonBoxCenterBigImageByDefault_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlWatcher.IsCenterBigImageByDefault = ButtonCenterBigImageByDefault.Content.ToString();
    }

    private void ButtonEnlargeSmallImageByDefault_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlWatcher.IsEnlargeSmallImageByDefault = ButtonEnlargeSmallImageByDefault.Content.ToString();
    }

    private void ButtonDescendingSort_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlWatcher.IsDescendingSort = ButtonDescendingSort.Content.ToString();
    }
  }
}

