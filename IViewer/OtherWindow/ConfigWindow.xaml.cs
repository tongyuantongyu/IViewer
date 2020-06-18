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
using IViewer.ViewModel;

namespace IViewer.OtherWindow {
  /// <summary>
  /// ConfigWindow.xaml 的交互逻辑
  /// </summary>
  public partial class ConfigWindow : Window {
    public ConfigWindow(ref TomlViewModel tomlViewModel) {

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

      base.DataContext = tomlViewModel;
    }

    private void Window_Closed(object sender, EventArgs e) {
      //写入配置
      TomlViewModel tomlViewModel = base.DataContext as TomlViewModel;
      tomlViewModel.WB();
      tomlViewModel.RaisePropertyChanged("TomlResult");
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
      MainWindow.tomlViewModel.IsAllowMultipleInstanceRunning = ButtonAllowMultipleInstanceRunning.Content.ToString();
    }

    private void ButtonConfirmBeforeDeleteFile_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlViewModel.IsConfirmBeforeDeleteFile = ButtonConfirmBeforeDeleteFile.Content.ToString();
    }

    private void ButtonBoxCenterBigImageByDefault_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlViewModel.IsCenterBigImageByDefault = ButtonCenterBigImageByDefault.Content.ToString();
    }

    private void ButtonEnlargeSmallImageByDefault_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlViewModel.IsEnlargeSmallImageByDefault = ButtonEnlargeSmallImageByDefault.Content.ToString();
    }

    private void ButtonDescendingSort_OnClick(object sender, RoutedEventArgs e) {
      RevertBoolContent(sender);
      MainWindow.tomlViewModel.IsDescendingSort = ButtonDescendingSort.Content.ToString();
    }
  }
}

