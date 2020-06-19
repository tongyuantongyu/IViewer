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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IViewer.OtherWindow {
  /// <summary>
  /// MoreImageInfoWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MoreImageInfoWindow : Window {
    public MoreImageInfoWindow() {
      InitializeComponent();
      
      TextBlockSource.Text = MainWindow.myMetaDataExtractor.SourseOutput();
      TextBlockImage.Text = MainWindow.myMetaDataExtractor.ImageOutput();
      TextBlockCamera.Text = MainWindow.myMetaDataExtractor.CameraOutput();
      TextBlockFile.Text = MainWindow.myMetaDataExtractor.FileOutput();
    }
  }
}
