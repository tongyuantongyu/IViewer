using System.Windows;

namespace IViewer.SubWindow {
  /// <summary>
  /// About.xaml 的交互逻辑
  /// </summary>
  public partial class About : Window {
    public About() {
      InitializeComponent();
      TextBlockAbout.Text = "Ivewer" + "\n" +
                            "感谢：" + "\n" +
                            "metadata-extractor" + "\n" +
                            "Tomlyn" + "\n"
        ;
    }
  }
}
