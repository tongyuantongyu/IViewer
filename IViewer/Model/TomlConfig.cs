using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace IViewer.Model {
  //ConfigEnum
  public enum EnumDefaultWindowMode {
    [Description("DefaultWindowMode_Normal")]
    Normal = 1,
    [Description("DefaultWindowMode_Maximized")]
    Maximized,
    [Description("DefaultWindowMode_LastTime")]
    LastTime
  }

  public enum EnumDefaultImageDisplayMode {
    [Description("DefaultImageDisplayMode_FitWindow")]
    FitWindow = 1,
    [Description("DefaultImageDisplayMode_OriginalSize")]
    OriginalSize
  }

  public enum EnumSortFileBy {
    [Description("SortFileBy_FileName")]
    FileName = 1,
    [Description("SortFileBy_ModifiedDate")]
    ModifiedDate,
    [Description("SortFileBy_Size")]
    Size
  }

  public enum EnumBehaviorOnReachingFirstLastFile {
    [Description("BehaviorOnReachingFirstLastFile_Ask")]
    Ask = 1,
    [Description("BehaviorOnReachingFirstLastFile_LoopInFolder")]
    LoopInFolder,
    [Description("BehaviorOnReachingFirstLastFile_Stop")]
    Stop,
    [Description("BehaviorOnReachingFirstLastFile_GotoPreviousNextFolder")]
    GotoPreviousNextFolder
  }

  public enum EnumFileInfo {
    [Description("FileInfo_Hide")]
    Hide = 1,
    [Description("FileInfo_Show")]
    Show,
    [Description("FileInfo_ShowOnHover")]
    ShowOnHover
  }

  public enum EnumEXIFInfo {
    [Description("EXIFInfo_Hide")]
    Hide = 1,
    [Description("EXIFInfo_Show")]
    Show,
    [Description("EXIFInfo_ShowOnHover")]
    ShowOnHover
  }

  public enum EnumImageEnlargingAlgorithm {
    [Description("ImageEnlargingAlgorithm_System")]
    System = 1,
    [Description("ImageEnlargingAlgorithm_NearestNeighbor")]
    NearestNeighbor,
    [Description("ImageEnlargingAlgorithm_Bilinear")]
    Bilinear,
    [Description("ImageEnlargingAlgorithm_Bicubic")]
    Bicubic,
    [Description("ImageEnlargingAlgorithm_HighQualityBilinear")]
    HighQualityBilinear,
    [Description("ImageEnlargingAlgorithm_HighQualityBicubic")]
    HighQualityBicubic
  }

  public enum EnumImageShrinkingAlgorithm {
    [Description("ImageShrinkingAlgorithm_System")]
    System = 1,
    [Description("ImageShrinkingAlgorithm_NearestNeighbor")]
    NearestNeighbor,
    [Description("ImageShrinkingAlgorithm_Bilinear")]
    Bilinear,
    [Description("ImageShrinkingAlgorithm_Bicubic")]
    Bicubic,
    [Description("ImageShrinkingAlgorithm_HighQualityBilinear")]
    HighQualityBilinear,
    [Description("ImageShrinkingAlgorithm_HighQualityBicubic")]
    HighQualityBicubic
  }

  public enum EnumImageDoublingAlgorithm {
    [Description("ImageDoublingAlgorithm_None")]
    None = 1,
    [Description("ImageDoublingAlgorithm_Nnedi3")]
    Nnedi3
  }

  public class TomlConfig {
    public long LongAnimationSpan;
    public double DoubleDragMultiplier;
    public double DoubleExtendRenderRatio;

    public long LongReRenderWaitTime;
    //Config Values

    //Behavior 
    public bool IsAllowMultipleInstanceRunning;
    public bool IsCenterBigImageByDefault;
    public bool IsConfirmBeforeDeleteFile;
    public bool IsDescendingSort;
    public bool IsEnlargeSmallImageByDefault;
    public long LongBehaviorOnReachingFirstLastFile;
    public long LongDefaultImageDisplayMode;
    public long LongDefaultWindowMode;

    public long LongEXIFInfo;

    //view
    public long LongFileInfo;
    public long LongImageDoublingAlgorithm;
    public long LongImageEnlargingAlgorithm;
    public long LongImageShrinkingAlgorithm;
    public string StringLanguage;
    public long LongSortFileBy;

    public string StringImageBackgroundColor;

    //other
    public string StringImageEditorPath;
    public string StringWindowBackgroundColor;

    public TomlConfig() //初始化
    {
      //behavior
      IsAllowMultipleInstanceRunning = true;
      IsConfirmBeforeDeleteFile = false;
      LongDefaultWindowMode = (long)EnumDefaultWindowMode.Maximized;
      LongDefaultImageDisplayMode = (long)EnumDefaultImageDisplayMode.FitWindow;
      IsCenterBigImageByDefault = true;
      IsEnlargeSmallImageByDefault = false;
      LongSortFileBy = (long)EnumSortFileBy.Size;
      IsDescendingSort = false;
      LongBehaviorOnReachingFirstLastFile = (long)EnumBehaviorOnReachingFirstLastFile.Ask;
      DoubleDragMultiplier = 2;
      LongAnimationSpan = 100;
      DoubleExtendRenderRatio = 1;
      LongReRenderWaitTime = 500;
      //view
      LongFileInfo = (long)EnumFileInfo.Hide;
      LongEXIFInfo = (long)EnumEXIFInfo.ShowOnHover;
      StringWindowBackgroundColor = "(255,255,255)";
      StringImageBackgroundColor = "(0,0,0)";
      LongImageEnlargingAlgorithm = (long)EnumImageEnlargingAlgorithm.HighQualityBicubic;
      LongImageShrinkingAlgorithm = (long)EnumImageShrinkingAlgorithm.HighQualityBilinear;
      LongImageDoublingAlgorithm = (long)EnumImageDoublingAlgorithm.None;
      //other
      StringImageEditorPath = "";
      StringLanguage = "en";
    }

    public override string ToString() {
      return GenerateTomlString();
    }

    public string GenerateTomlString() {
      const string tableName = App.ConfigTable;
      //动态添加所有属性
      string output = "[" + tableName + "]\n";
      Type t = typeof(TomlConfig);
      FieldInfo[] infos = t.GetFields();
      foreach (FieldInfo info in infos) {
        if (info.FieldType == output.GetType()) {
          output += info.Name + "=" + "'" + info.GetValue(this) + "'" + "\n";
        }
        else if (info.FieldType == false.GetType()) {
          output += info.Name + "=" + ((bool)info.GetValue(this) ? "true" : "false") + "\n";
        }
        else {
          output += info.Name + "=" + info.GetValue(this) + "\n";
        }
      }

      return output;
    }

    public void Write(string fileName) //写文件
    {
      using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
        string output = GenerateTomlString();
        byte[] bytes = Encoding.UTF8.GetBytes(output);
        fileStream.Write(bytes, 0, bytes.Length);
      }
    }

    public string ReadDoc(string fileName) {
      if (File.Exists(fileName)) {
        using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
          byte[] bytes = new byte[fileStream.Length];
          fileStream.Read(bytes, 0, bytes.Length);
          char[] c = Encoding.UTF8.GetChars(bytes);
          string input = new string(c);
          return input;
        }
      }

      Write(fileName);
      return null;
    }

    public bool ReadString(string str) {
      const string tableName = App.ConfigTable;
      var doc = Toml.Parse(str);
      if (doc.HasErrors) {
        return false; //解析错误则返回失败
      }

      //使用Toml库解析
      var table = doc.ToModel();
      var tomlTable = (TomlTable)table[tableName];

      //反射自动导入所有属性
      Type t = typeof(TomlConfig);
      FieldInfo[] infos = t.GetFields();
      foreach (FieldInfo info in infos) {
        if (tomlTable.ContainsKey(info.Name)) {
          info.SetValue(this, Convert.ChangeType(((TomlTable)table[tableName])[info.Name], info.FieldType));
        }
        else {
          return false; //key错误则返回
        }
      }

      return true;
    }

    public bool Read(string fileName) {
      string input = ReadDoc(fileName); //读取文件
      if (input != null) {
        if (ReadString(input)) {
          return true;
        }

        Write(fileName);
        return false;

      }

      Write(fileName);
      return false;
    }
  }
}