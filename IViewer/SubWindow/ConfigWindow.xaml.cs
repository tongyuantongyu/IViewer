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
      //Initial ComboBox with Enum name
      for (int i = 0; i < Enum.GetValues(typeof(EnumDefaultWindowMode)).Length; i++) {
        ComboBoxDefaultWindowMode.Items.Add(Enum.GetName(typeof(EnumDefaultWindowMode), i));
      }

      for (int i = 0; i < Enum.GetValues(typeof(EnumDefaultImageDisplayMode)).Length; i++) {
        ComboBoxDefaultImageDisplayMode.Items.Add(Enum.GetName(typeof(EnumDefaultImageDisplayMode), i));
      }

      for (int i = 0; i < Enum.GetValues(typeof(EnumSortFileBy)).Length; i++) {
        ComboBoxSortFileBy.Items.Add(Enum.GetName(typeof(EnumSortFileBy), i));
      }

      for (int i = 0; i < Enum.GetValues(typeof(EnumBehaviorOnReachingFirstLastFile)).Length; i++) {
        ComboBoxBehaviorOnReachingFirstLastFile.Items.Add(Enum.GetName(typeof(EnumBehaviorOnReachingFirstLastFile), i));
      }

      for (int i = 0; i < Enum.GetValues(typeof(EnumFileInfo)).Length; i++) {
        ComboBoxFileInfo.Items.Add(Enum.GetName(typeof(EnumFileInfo), i));
      }

      for (int i = 0; i < Enum.GetValues(typeof(EnumEXIFInfo)).Length; i++) {
        ComboBoxEXIFInfo.Items.Add(Enum.GetName(typeof(EnumEXIFInfo), i));
      }

      for (int i = 0; i < Enum.GetValues(typeof(EnumImageEnlargingAlgorithm)).Length; i++) {
        ComboBoxImageEnlargingAlgorithm.Items.Add(Enum.GetName(typeof(EnumImageEnlargingAlgorithm), i));
      }

      for (int i = 0; i < Enum.GetValues(typeof(EnumImageShrinkingAlgorithm)).Length; i++) {
        ComboBoxShrinkingAlgorithm.Items.Add(Enum.GetName(typeof(EnumImageShrinkingAlgorithm), i));
      }

      for (int i = 0; i < Enum.GetValues(typeof(EnumImageDoublingAlgorithm)).Length; i++) {
        ComboBoxDoublingAlgorithm.Items.Add(Enum.GetName(typeof(EnumImageDoublingAlgorithm), i));
      }

      // for (int i = 0; i < Enum.GetValues(typeof(EnumLanguage)).Length; i++) {
      //   ComboBoxLanguage.Items.Add(Enum.GetName(typeof(EnumLanguage), i));
      // }

      DataContext = settings;
    }



    private void Window_Closed(object sender, EventArgs e) {
      //写入配置
      Settings settings = DataContext as Settings;
      Settings.SaveToFile(App.ConfigLocation);
      settings.RaisePropertyChanged("TomlResult");
    }

    private void SelectFileButton_OnClick(object sender, RoutedEventArgs e) {
      var dialog = new OpenFileDialog {
        RestoreDirectory = true,
        Filter = $"{Properties.Resources.Type_PNG} (*.png)|*.png|" +
                 $"{Properties.Resources.Type_HEIF} (*.heic)|*.heic|" +
                 $"{Properties.Resources.Type_Any} (*.*)|*.*",
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