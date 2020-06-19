using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using IViewer.Model;

namespace IViewer {
  public class Settings : INotifyPropertyChanged {
    public static TomlConfig TomlConfig;
    public event PropertyChangedEventHandler PropertyChanged;
    public static readonly Settings Instance = new Settings();

    public void RaisePropertyChanged(string propertyName) {//属性更改方法
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Settings() {
      TomlConfig = new TomlConfig();
      TomlConfig.Read(App.ConfigLocation);
    }
    public static void SaveToFile(string fileName) { TomlConfig.Write(fileName);}

    public static void ReadFromFile(string fileName) { TomlConfig.Read(fileName);}
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
      set { TomlConfig.LongDefaultWindowMode = (long)(value); }
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

    public string DoubleDragMultiplier {
      get { return TomlConfig.DoubleDragMultiplier.ToString(); }
      set { TomlConfig.DoubleDragMultiplier = double.Parse(value); }
    }

    public string DoubleAnimationSpan {
      get { return TomlConfig.DoubleAnimationSpan.ToString(); }
      set { TomlConfig.DoubleAnimationSpan = double.Parse(value); }
    }
    public string DoubleExtendRenderRatio {
      get { return TomlConfig.DoubleExtendRenderRatio.ToString(); }
      set { TomlConfig.DoubleExtendRenderRatio = double.Parse(value); }
    }
    public string DoubleReRenderWaitTime {
      get { return TomlConfig.DoubleReRenderWaitTime.ToString(); }
      set { TomlConfig.DoubleReRenderWaitTime = double.Parse(value); }
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

    public long LongLanguage {
      get { return TomlConfig.LongLanguage; }
      set { TomlConfig.LongLanguage = value; }
    }

    //ContexMenu
    public bool SortByFileName {
      get { return TomlConfig.LongSortFileBy == (long)EnumSortFileBy.FileName; }
      set {
        if (value == true) {
          TomlConfig.LongSortFileBy = (long)EnumSortFileBy.FileName;RaisePropertyChanged("LongSortFileBy");
          RaisePropertyChanged("TomlResult");
        }
      }
    }

    public bool SortByModifyDate {
      get { return TomlConfig.LongSortFileBy == (long)EnumSortFileBy.ModifiedDate; }
      set {
        if (value == true) {
          TomlConfig.LongSortFileBy = (long)EnumSortFileBy.ModifiedDate;RaisePropertyChanged("LongSortFileBy");
          RaisePropertyChanged("TomlResult");
        }
      }
    }

    public bool SortBySize {
      get { return TomlConfig.LongSortFileBy == (long)EnumSortFileBy.Size; }
      set {
        if (value == true) {
          TomlConfig.LongSortFileBy = (long)EnumSortFileBy.Size; RaisePropertyChanged("LongSortFileBy");
          RaisePropertyChanged("TomlResult");
        }
      }
    }

    public bool OriginalMode {
      get { return TomlConfig.LongDefaultImageDisplayMode == (long)EnumDefaultImageDisplayMode.OriginalSize; }
      set {
        if (value == true) {
          TomlConfig.LongDefaultImageDisplayMode = (long)EnumDefaultImageDisplayMode.OriginalSize; RaisePropertyChanged("LongDefaultImageDisplayMode");
          RaisePropertyChanged("TomlResult");
        }
      }
    }

    public bool FitWindowMode {
      get { return TomlConfig.LongDefaultImageDisplayMode == (long)EnumDefaultImageDisplayMode.FitWindow; }
      set {
        if (value == true) {
          TomlConfig.LongDefaultImageDisplayMode = (long)EnumDefaultImageDisplayMode.FitWindow; RaisePropertyChanged("LongDefaultImageDisplayMode");
          RaisePropertyChanged("TomlResult");
        }
      }
    }
  }
}
