using System.ComponentModel;
using System.Globalization;
using System.Resources;
using IViewer.Model;
using IViewer.Properties;

namespace IViewer {
  public class Settings : INotifyPropertyChanged {
    public static TomlConfig TomlConfig;
    public static readonly Settings Instance = new Settings();
    private bool pauseNotify;
    public bool PauseNotify {
      get => pauseNotify;
      set {
        if (!value) {
          RaisePropertyChanged("");
        }

        pauseNotify = value;
      }
    }

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
      set {
        TomlConfig.IsAllowMultipleInstanceRunning = value;
        ConditionalRaisePropertyChanged(nameof(IsAllowMultipleInstanceRunning));
      }
    }

    public bool IsConfirmBeforeDeleteFile {
      get { return TomlConfig.IsConfirmBeforeDeleteFile; }
      set {
        TomlConfig.IsConfirmBeforeDeleteFile = value;
        ConditionalRaisePropertyChanged(nameof(IsConfirmBeforeDeleteFile));
      }
    }

    public long LongDefaultWindowMode {
      get { return TomlConfig.LongDefaultWindowMode; }
      set {
        TomlConfig.LongDefaultWindowMode = value;
        ConditionalRaisePropertyChanged(nameof(LongDefaultWindowMode));
      }
    }

    public long LongDefaultImageDisplayMode {
      get { return TomlConfig.LongDefaultImageDisplayMode; }
      set {
        TomlConfig.LongDefaultImageDisplayMode = value;
        ConditionalRaisePropertyChanged(nameof(LongDefaultImageDisplayMode));
      }
    }

    public bool IsCenterBigImageByDefault {
      get { return TomlConfig.IsCenterBigImageByDefault; }
      set {
        TomlConfig.IsCenterBigImageByDefault = value;
        ConditionalRaisePropertyChanged(nameof(IsCenterBigImageByDefault));
      }
    }

    public bool IsEnlargeSmallImageByDefault {
      get { return TomlConfig.IsEnlargeSmallImageByDefault; }
      set {
        TomlConfig.IsEnlargeSmallImageByDefault = value;
        ConditionalRaisePropertyChanged(nameof(IsEnlargeSmallImageByDefault));
      }
    }

    public long LongSortFileBy {
      get { return TomlConfig.LongSortFileBy; }
      set {
        TomlConfig.LongSortFileBy = value;
        ConditionalRaisePropertyChanged(nameof(LongSortFileBy));
      }
    }

    public bool IsDescendingSort {
      get { return TomlConfig.IsDescendingSort; }
      set {
        TomlConfig.IsDescendingSort = value;
        ConditionalRaisePropertyChanged(nameof(IsDescendingSort));
      }
    }

    public long LongBehaviorOnReachingFirstLastFile {
      get { return TomlConfig.LongBehaviorOnReachingFirstLastFile; }
      set {
        TomlConfig.LongBehaviorOnReachingFirstLastFile = value;
        ConditionalRaisePropertyChanged(nameof(LongBehaviorOnReachingFirstLastFile));
      }
    }

    public double DoubleDragMultiplier {
      get { return TomlConfig.DoubleDragMultiplier; }
      set {
        TomlConfig.DoubleDragMultiplier = value;
        ConditionalRaisePropertyChanged(nameof(DoubleDragMultiplier));
      }
    }

    public long LongAnimationSpan {
      get { return TomlConfig.LongAnimationSpan; }
      set {
        TomlConfig.LongAnimationSpan = value;
        ConditionalRaisePropertyChanged(nameof(LongAnimationSpan));
      }
    }

    public double DoubleExtendRenderRatio {
      get { return TomlConfig.DoubleExtendRenderRatio; }
      set {
        TomlConfig.DoubleExtendRenderRatio = value;
        ConditionalRaisePropertyChanged(nameof(DoubleExtendRenderRatio));
      }
    }

    public long LongReRenderWaitTime {
      get { return TomlConfig.LongReRenderWaitTime; }
      set {
        TomlConfig.LongReRenderWaitTime = value;
        ConditionalRaisePropertyChanged(nameof(LongReRenderWaitTime));
      }
    }

    //view
    public long LongFileInfo {
      get { return TomlConfig.LongFileInfo; }
      set {
        TomlConfig.LongFileInfo = value;
        ConditionalRaisePropertyChanged(nameof(LongFileInfo));
      }
    }

    public long LongEXIFInfo {
      get { return TomlConfig.LongEXIFInfo; }
      set {
        TomlConfig.LongEXIFInfo = value;
        ConditionalRaisePropertyChanged(nameof(LongEXIFInfo));
      }
    }

    public string StringWindowBackgroundColor {
      get { return TomlConfig.StringWindowBackgroundColor; }
      set {
        TomlConfig.StringWindowBackgroundColor = value;
        ConditionalRaisePropertyChanged(nameof(StringWindowBackgroundColor));
      }
    }

    public string StringImageBackgroundColor {
      get { return TomlConfig.StringImageBackgroundColor; }
      set {
        TomlConfig.StringImageBackgroundColor = value;
        ConditionalRaisePropertyChanged(nameof(StringImageBackgroundColor));
      }
    }

    public long LongImageEnlargingAlgorithm {
      get { return TomlConfig.LongImageEnlargingAlgorithm; }
      set {
        TomlConfig.LongImageEnlargingAlgorithm = value;
        ConditionalRaisePropertyChanged(nameof(LongImageEnlargingAlgorithm));
      }
    }

    public long LongImageShrinkingAlgorithm {
      get { return TomlConfig.LongImageShrinkingAlgorithm; }
      set {
        TomlConfig.LongImageShrinkingAlgorithm = value;
        ConditionalRaisePropertyChanged(nameof(LongImageShrinkingAlgorithm));
      }
    }

    public long LongImageDoublingAlgorithm {
      get { return TomlConfig.LongImageDoublingAlgorithm; }
      set {
        TomlConfig.LongImageDoublingAlgorithm = value;
        ConditionalRaisePropertyChanged(nameof(LongImageDoublingAlgorithm));
      }
    }

    //other
    public string StringImageEditorPath {
      get { return TomlConfig.StringImageEditorPath; }
      set {
        TomlConfig.StringImageEditorPath = value;
        ConditionalRaisePropertyChanged(nameof(StringImageEditorPath));
      }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    public static string Resource(string name) {
      return name != null ? ResourceManager.GetString(name, Culture) ?? name.Replace("_", " ") : null;
    }

    public void RaisePropertyChanged(string propertyName) {
      //属性更改方法
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ConditionalRaisePropertyChanged(string propertyName) {
      if (!PauseNotify) {
        RaisePropertyChanged(propertyName);
      }
    }

    public static void SaveToFile(string fileName) { TomlConfig.Write(fileName); }

    public static void ReadFromFile(string fileName) { TomlConfig.Read(fileName); }
  }
}