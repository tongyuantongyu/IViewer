using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IViewer {
  /// <summary>
  /// App.xaml 的交互逻辑
  /// </summary>
  public partial class App : Application {
    public static readonly string ConfigLocation = $"{Path.GetDirectoryName(ResourceAssembly.Location)}\\config.toml";
    public const string ConfigTable = "IViewer";
  }
}
