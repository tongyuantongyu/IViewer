using System.ComponentModel;
using System.Globalization;
using System.Resources;
using IViewer.Model;
using IViewer.Properties;

namespace IViewer {
  public class Settings : INotifyPropertyChanged {
    public static TomlConfig TomlConfig;
    public static readonly Settings Instance = new Settings();

    private static readonly ResourceManager ResourceManager =
      new ResourceManager("IViewer.Properties.Resources", typeof(Resources).Assembly);

    private Settings() {
      TomlConfig = new TomlConfig();
      TomlConfig.Read(App.ConfigLocation);
    }

    public static CultureInfo Culture {
      get => CultureInfo.GetCultureInfo(TomlConfig.StringLanguage);
      set {
        TomlConfig.StringLanguage = value.Name;
      }
    }

    //字段
    public string TomlResult => TomlConfig.ToString();

    //behavior
    public bool IsAllowMultipleInstanceRunning {
      get { return TomlConfig.IsAllowMultipleInstanceRunning; }
      set { TomlConfig.IsAllowMultipleInstanceRunning = value; }
    }

    public bool IsConfirmBeforeDeleteFile {
      get { return TomlConfig.IsConfirmBeforeDeleteFile; }
      set { TomlConfig.IsConfirmBeforeDeleteFile = value; }
    }

    public long LongDefaultWindowMode {
      get { return TomlConfig.LongDefaultWindowMode; }
      set { TomlConfig.LongDefaultWindowMode = value; }
    }

    public long LongDefaultImageDisplayMode {
      get { return TomlConfig.LongDefaultImageDisplayMode; }
      set { TomlConfig.LongDefaultImageDisplayMode = value; }
    }

    public bool IsCenterBigImageByDefault {
      get { return TomlConfig.IsCenterBigImageByDefault; }
      set { TomlConfig.IsCenterBigImageByDefault = value; }
    }

    public bool IsEnlargeSmallImageByDefault {
      get { return TomlConfig.IsEnlargeSmallImageByDefault; }
      set { TomlConfig.IsEnlargeSmallImageByDefault = value; }
    }

    public long LongSortFileBy {
      get { return TomlConfig.LongSortFileBy; }
      set { TomlConfig.LongSortFileBy = value; }
    }

    public bool IsDescendingSort {
      get { return TomlConfig.IsDescendingSort; }
      set { TomlConfig.IsDescendingSort = value; }
    }

    public long LongBehaviorOnReachingFirstLastFile {
      get { return TomlConfig.LongBehaviorOnReachingFirstLastFile; }
      set { TomlConfig.LongBehaviorOnReachingFirstLastFile = value; }
    }

    public double DoubleDragMultiplier {
      get { return TomlConfig.DoubleDragMultiplier; }
      set { TomlConfig.DoubleDragMultiplier = value; }
    }

    public long DoubleAnimationSpan {
      get { return TomlConfig.LongAnimationSpan; }
      set { TomlConfig.LongAnimationSpan = value; }
    }

    public double DoubleExtendRenderRatio {
      get { return TomlConfig.DoubleExtendRenderRatio; }
      set { TomlConfig.DoubleExtendRenderRatio = value; }
    }

    public long LongReRenderWaitTime {
      get { return TomlConfig.LongReRenderWaitTime; }
      set { TomlConfig.LongReRenderWaitTime = value; }
    }

    //view
    public long LongFileInfo {
      get { return TomlConfig.LongFileInfo; }
      set { TomlConfig.LongFileInfo = value; }
    }

    public long LongEXIFInfo {
      get { return TomlConfig.LongEXIFInfo; }
      set { TomlConfig.LongEXIFInfo = value; }
    }

    public string StringWindowBackgroundColor {
      get { return TomlConfig.StringWindowBackgroundColor; }
      set { TomlConfig.StringWindowBackgroundColor = value; }
    }

    public string StringImageBackgroundColor {
      get { return TomlConfig.StringImageBackgroundColor; }
      set { TomlConfig.StringImageBackgroundColor = value; }
    }

    public long LongImageEnlargingAlgorithm {
      get { return TomlConfig.LongImageEnlargingAlgorithm; }
      set { TomlConfig.LongImageEnlargingAlgorithm = value; }
    }

    public long LongImageShrinkingAlgorithm {
      get { return TomlConfig.LongImageShrinkingAlgorithm; }
      set { TomlConfig.LongImageShrinkingAlgorithm = value; }
    }

    public long LongImageDoublingAlgorithm {
      get { return TomlConfig.LongImageDoublingAlgorithm; }
      set { TomlConfig.LongImageDoublingAlgorithm = value; }
    }

    //other
    public string StringImageEditorPath {
      get { return TomlConfig.StringImageEditorPath; }
      set { TomlConfig.StringImageEditorPath = value; }
    }

    public long SortBy {
      get { return TomlConfig.LongSortFileBy; }
      set { TomlConfig.LongSortFileBy = value; }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    public static string Resource(string name) {
      return ResourceManager.GetString(name, Culture) ?? name.Replace("_", " ");
    }

    public void RaisePropertyChanged(string propertyName) {
      //属性更改方法
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static void SaveToFile(string fileName) { TomlConfig.Write(fileName); }

    public static void ReadFromFile(string fileName) { TomlConfig.Read(fileName); }
  }
}