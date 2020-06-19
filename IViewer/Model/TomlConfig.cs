using System;
using System.IO;
using System.Reflection;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace IViewer.Model {
  //ConfigEnum
  public enum EnumDefaultWindowMode {
    Normal = 0,
    Maximized = 1,
    LastTime = 2
  }

  public enum EnumDefaultImageDisplayMode {
    OriginalSize = 0,
    FitWindow = 1
  }

  public enum EnumSortFileBy {
    FileName = 0,
    ModifiedDate = 1,
    Size = 2
  }

  public enum EnumBehaviorOnReachingFirstLastFile {
    Ask = 0,
    LoopInFolder = 1,
    Stop = 2,
    GotoPreviousNextFolder = 3
  }

  public enum EnumFileInfo {
    Hide = 0,
    Show = 1,
    ShowOnHover = 2
  }

  public enum EnumEXIFInfo {
    Hide = 0,
    Show = 1,
    ShowOnHover = 2
  }

  public enum EnumImageEnlargingAlgorithm {
    A = 0,
    B = 1,
    C = 2
  }

  public enum EnumImageShrinkingAlgorithm {
    A = 0,
    B = 1,
    C = 2
  }

  public enum EnumImageDoublingAlgorithm {
    A = 0,
    B = 1,
    C = 2
  }

  public enum EnumLanguage {
    Chinese = 0,
    English = 1
  }

  public class TomlConfig {
    public double DoubleAnimationSpan;
    public double DoubleDragMultiplier;
    public double DoubleExtendRenderRatio;

    public double DoubleReRenderWaitTime;
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
    public long LongLanguage;
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
      DoubleDragMultiplier = 0.57;
      DoubleAnimationSpan = 0.98;
      DoubleExtendRenderRatio = 0.66;
      DoubleReRenderWaitTime = 1.67;
      //view
      LongFileInfo = (long)EnumFileInfo.Hide;
      LongEXIFInfo = (long)EnumEXIFInfo.ShowOnHover;
      StringWindowBackgroundColor = "(255,255,255)";
      StringImageBackgroundColor = "(0,0,0)";
      LongImageEnlargingAlgorithm = (long)EnumImageEnlargingAlgorithm.B;
      LongImageShrinkingAlgorithm = (long)EnumImageShrinkingAlgorithm.C;
      LongImageDoublingAlgorithm = (long)EnumImageDoublingAlgorithm.A;
      //other
      StringImageEditorPath = "";
      LongLanguage = (long)EnumLanguage.Chinese;
    }

    public override string ToString() {
      return GenerateTomlString();
    }

    public string GenerateTomlString() //生成toml字符串
    {
      bool BoolType = false;
      string TableName = "TestTable";
      //动态添加所有属性
      string output = "[" + TableName + "]\n";
      Type t = typeof(TomlConfig);
      FieldInfo[] infos = t.GetFields();
      foreach (FieldInfo info in infos) {
        if (info.FieldType == output.GetType()) { //string类型需要添加单引号
          output += info.Name + "=" + "'" + info.GetValue(this) + "'" + "\n";
        }
        else if (info.FieldType == false.GetType()) { //bool类型需要小写
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
      using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
        string output = GenerateTomlString();
        byte[] bytes = Encoding.UTF8.GetBytes(output);
        fileStream.Write(bytes, 0, bytes.Length);
      }
    }

    public string ReadDoc(string fileName) { //读取文件返回字符串，如果文件不存在，则创建文件写入默认值返回null
      if (File.Exists(fileName)) { //文件存在
        using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
          byte[] bytes = new byte[fileStream.Length];
          //读取文件信息
          fileStream.Read(bytes, 0, bytes.Length);
          //将得到的字节型数组重写编码为字符型数组
          char[] c = Encoding.UTF8.GetChars(bytes);
          string input = new string(c);
          return input;
        }
      }

      Write(fileName);
      return null;
    }

    public bool ReadString(string str) //处理字符串
    {
      string TableName = "TestTable";
      var doc = Toml.Parse(str);
      if (doc.HasErrors) {
        return false; //解析错误则返回失败
      }

      //使用Toml库解析
      var table = doc.ToModel();
      var tomlTable = (TomlTable)table[TableName];

      //反射自动导入所有属性
      Type t = typeof(TomlConfig);
      FieldInfo[] infos = t.GetFields();
      foreach (FieldInfo info in infos) {
        if (tomlTable.ContainsKey(info.Name)) {
          info.SetValue(this, Convert.ChangeType(((TomlTable)table[TableName])[info.Name], info.FieldType));
        }
        else {
          return false; //key错误则返回
        }
      }

      return true;
    }

    public bool Read(string fileName) //处理读取的文件，返回是否读取成功
    {
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