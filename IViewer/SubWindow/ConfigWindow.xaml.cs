using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using IViewer.Model;
using Microsoft.Win32;

namespace IViewer.SubWindow {
  /// <summary>
  ///   ConfigWindow.xaml 的交互逻辑
  /// </summary>
  public partial class ConfigWindow : Window {
    public ConfigWindow(Settings settings) {
      InitializeComponent();
      DataContext = settings;
    }

    private void Window_Closed(object sender, EventArgs e) {
      //写入配置
      var settings = DataContext as Settings;
      Settings.SaveToFile(App.ConfigLocation);
    }

    private void PickImageEditor(object sender, RoutedEventArgs e) {
      var dialog = new OpenFileDialog {
        RestoreDirectory = true,
        Filter = $"{Properties.Resources.Type_EXE} (*.exe)|*.exe",
        FilterIndex = 0
      };
      if (dialog.ShowDialog() != true) {
        return;
      }

      TextBoxImageEditorPath.Text = dialog.FileName;
    }

    private void VerifyLong(object sender, System.Windows.Input.TextCompositionEventArgs e) {
      if (!long.TryParse(e.Text, out var v)) {
        e.Handled = true;
      }
      else if (v < 0) {
        e.Handled = true;
      }
    }

    private void VerifyDouble(object sender, System.Windows.Input.TextCompositionEventArgs e) {
      if (!double.TryParse(e.Text, out var v)) {
        e.Handled = true;
      }
      else if (v < 0 || double.IsInfinity(v) || double.IsNaN(v)) {
        e.Handled = true;
      }
    }
  }
}