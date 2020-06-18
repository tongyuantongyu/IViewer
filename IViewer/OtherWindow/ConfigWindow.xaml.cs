using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IViewer.ViewModel;
using Microsoft.Win32;

namespace IViewer.OtherWindow {
  /// <summary>
  /// ConfigWindow.xaml 的交互逻辑
  /// </summary>
  public partial class ConfigWindow : Window {
    public ConfigWindow(TomlViewModel tomlViewModel) {

      InitializeComponent();
      //Initial ComboBox with Enum name
      for (int i = 0; i < Enum.GetValues(typeof(EnumDefaultWindowMode)).Length; i++)
        ComboBoxDefaultWindowMode.Items.Add(Enum.GetName(typeof(EnumDefaultWindowMode), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumDefaultImageDisplayMode)).Length; i++)
        ComboBoxDefaultImageDisplayMode.Items.Add(Enum.GetName(typeof(EnumDefaultImageDisplayMode), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumSortFileBy)).Length; i++)
        ComboBoxSortFileBy.Items.Add(Enum.GetName(typeof(EnumSortFileBy), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumBehaviorOnReachingFirstLastFile)).Length; i++)
        ComboBoxBehaviorOnReachingFirstLastFile.Items.Add(Enum.GetName(typeof(EnumBehaviorOnReachingFirstLastFile), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumFileInfo)).Length; i++)
        ComboBoxFileInfo.Items.Add(Enum.GetName(typeof(EnumFileInfo), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumEXIFInfo)).Length; i++)
        ComboBoxEXIFInfo.Items.Add(Enum.GetName(typeof(EnumEXIFInfo), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumImageEnlargingAlgorithm)).Length; i++)
        ComboBoxImageEnlargingAlgorithm.Items.Add(Enum.GetName(typeof(EnumImageEnlargingAlgorithm), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumImageShrinkingAlgorithm)).Length; i++)
        ComboBoxShrinkingAlgorithm.Items.Add(Enum.GetName(typeof(EnumImageShrinkingAlgorithm), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumImageDoublingAlgorithm)).Length; i++)
        ComboBoxDoublingAlgorithm.Items.Add(Enum.GetName(typeof(EnumImageDoublingAlgorithm), i));
      for (int i = 0; i < Enum.GetValues(typeof(EnumLanguage)).Length; i++)
        ComboBoxLanguage.Items.Add(Enum.GetName(typeof(EnumLanguage), i));

      base.DataContext = tomlViewModel;
    }

    private void Window_Closed(object sender, EventArgs e) {
      //写入配置
      TomlViewModel tomlViewModel = base.DataContext as TomlViewModel;
      tomlViewModel.WB(MainWindow.TomlFileName);
      tomlViewModel.RaisePropertyChanged("TomlResult");
    }

    private void ScrollBarDragMultiplier_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      ScrollBar scrollBar = sender as ScrollBar;
      double num = scrollBar.Value;
      LabelDragMultiplierNum.Content = num.ToString();
    }

    private void ScrollBarAnimationSpan_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      ScrollBar scrollBar = sender as ScrollBar;
      double num = scrollBar.Value;
      LabelAnimationSpanNum.Content = num.ToString();
    }

    private void ScrollBarExtendRenderRatio_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      ScrollBar scrollBar = sender as ScrollBar;
      double num = scrollBar.Value;
      LabelExtendRenderRatioNum.Content = num.ToString();
    }

    private void ScrollBarReRenderWaitTime_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      ScrollBar scrollBar = sender as ScrollBar;
      double num = scrollBar.Value;
      LabelReRenderWaitTimeNum.Content = num.ToString();
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
  }
}

