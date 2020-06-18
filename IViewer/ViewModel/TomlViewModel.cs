using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IViewer.ViewModel {
  public class TomlViewModel : INotifyPropertyChanged {
    public static TomlConfig tomlConfig;
    public string fileName = "test.toml";
    public event PropertyChangedEventHandler PropertyChanged;

    public void RaisePropertyChanged(string propertyName) {//属性更改方法
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public TomlViewModel() {
      tomlConfig = new TomlConfig();
      tomlConfig.Read(fileName);
    }
    public void WB() { tomlConfig.Write(fileName);}

    //字段
    public string TomlResult {
      get { return tomlConfig.ToString(); }
      set {; }
    }

    //behavior
    public bool IsAllowMultipleInstanceRunning {
      get { return tomlConfig.IsAllowMultipleInstanceRunning; }
      set { tomlConfig.IsAllowMultipleInstanceRunning = value; }
    }

    public string IsConfirmBeforeDeleteFile {
      get { return tomlConfig.IsConfirmBeforeDeleteFile.ToString(); }
      set { tomlConfig.IsConfirmBeforeDeleteFile = bool.Parse(value); }
    }

    public long LongDefaultWindowMode {
      get { return tomlConfig.LongDefaultWindowMode; }
      set { tomlConfig.LongDefaultWindowMode = (long)(value); }
    }

    public long LongDefaultImageDisplayMode {
      get { return tomlConfig.LongDefaultImageDisplayMode; }
      set { tomlConfig.LongDefaultImageDisplayMode = value; }
    }

    public string IsCenterBigImageByDefault {
      get { return tomlConfig.IsCenterBigImageByDefault.ToString(); }
      set { tomlConfig.IsCenterBigImageByDefault = bool.Parse(value); }
    }

    public string IsEnlargeSmallImageByDefault {
      get { return tomlConfig.IsEnlargeSmallImageByDefault.ToString(); }
      set { tomlConfig.IsEnlargeSmallImageByDefault = bool.Parse(value); }
    }

    public long LongSortFileBy {
      get { return tomlConfig.LongSortFileBy; }
      set { tomlConfig.LongSortFileBy = value; }
    }

    public string IsDescendingSort {
      get { return tomlConfig.IsDescendingSort.ToString(); }
      set { tomlConfig.IsDescendingSort = bool.Parse(value); }
    }

    public long LongBehaviorOnReachingFirstLastFile {
      get { return tomlConfig.LongBehaviorOnReachingFirstLastFile; }
      set { tomlConfig.LongBehaviorOnReachingFirstLastFile = value; }
    }

    public string DoubleDragMultiplier {
      get { return tomlConfig.DoubleDragMultiplier.ToString(); }
      set { tomlConfig.DoubleDragMultiplier = double.Parse(value); }
    }

    public string DoubleAnimationSpan {
      get { return tomlConfig.DoubleAnimationSpan.ToString(); }
      set { tomlConfig.DoubleAnimationSpan = double.Parse(value); }
    }
    public string DoubleExtendRenderRatio {
      get { return tomlConfig.DoubleExtendRenderRatio.ToString(); }
      set { tomlConfig.DoubleExtendRenderRatio = double.Parse(value); }
    }
    public string DoubleReRenderWaitTime {
      get { return tomlConfig.DoubleReRenderWaitTime.ToString(); }
      set { tomlConfig.DoubleReRenderWaitTime = double.Parse(value); }
    }

    //view
    public long LongFileInfo {
      get { return tomlConfig.LongFileInfo; }
      set { tomlConfig.LongFileInfo = value; }
    }

    public long LongEXIFInfo {
      get { return tomlConfig.LongEXIFInfo; }
      set { tomlConfig.LongEXIFInfo = value; }
    }

    public string StringWindowBackgroundColor {
      get { return tomlConfig.StringWindowBackgroundColor; }
      set { tomlConfig.StringWindowBackgroundColor = value; }
    }

    public string StringImageBackgroundColor {
      get { return tomlConfig.StringImageBackgroundColor; }
      set { tomlConfig.StringImageBackgroundColor = value; }
    }

    public long LongImageEnlargingAlgorithm {
      get { return tomlConfig.LongImageEnlargingAlgorithm; }
      set { tomlConfig.LongImageEnlargingAlgorithm = value; }
    }

    public long LongImageShrinkingAlgorithm {
      get { return tomlConfig.LongImageShrinkingAlgorithm; }
      set { tomlConfig.LongImageShrinkingAlgorithm = value; }
    }

    public long LongImageDoublingAlgorithm {
      get { return tomlConfig.LongImageDoublingAlgorithm; }
      set { tomlConfig.LongImageDoublingAlgorithm = value; }
    }
    //other
    public string StringImageEditorPath {
      get { return tomlConfig.StringImageEditorPath; }
      set { tomlConfig.StringImageEditorPath = value; }
    }

    public string StringLanguage {
      get { return tomlConfig.StringLanguage; }
      set { tomlConfig.StringLanguage = value; }
    }

  }
}
