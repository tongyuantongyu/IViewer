using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace IViewer.SubWindow {
  public partial class MetadataWindow : Window {
    public MetadataWindow(IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>> data) {
      InitializeComponent();
      foreach (var page in data) {
        Base.Items.Add(new TabItem {Header = page.Key,
          Content = new DataGrid {
            AutoGenerateColumns = true,
            Margin = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ItemsSource = page.Value
          }
        });
      }

      if (Base.Items.Count >= 1) {
        ((TabItem) Base.Items[0]).IsSelected = true;
      }
    }
  }


}
