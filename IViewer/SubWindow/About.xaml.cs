using System.Windows;

namespace IViewer.SubWindow {
  /// <summary>
  /// About.xaml 的交互逻辑
  /// </summary>
  public partial class About : Window {
    public About() {
      InitializeComponent();
      TextBlockAbout.Text = "IViewer v0.1.0" + "\n" +
                            "Thanks：" + "\n" +
                            "metadata-extractor" + "\n" +
                            "Tomlyn" + "\n"
        ;
    }
  }
}
