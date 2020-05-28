using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace IViewer {
  class TomlConfig {
    //Test Values
    public long ConfigLong;
    public bool ConfigBool;
    public string ConfigString;
    public double ConfigDouble;

    public TomlConfig() {
      ConfigLong = 15;
      ConfigBool = true;
      ConfigString = "dzyTest2";
      ConfigDouble = 3.14159265;
    }

    public void Write(string fileName)//写文件
    {
      FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
      //动态添加所有属性
      string output = "" + @"[TestTable]" + "\n";
      Type t = typeof(TomlConfig);
      FieldInfo[] infos = t.GetFields();
      foreach (FieldInfo info in infos) {
        if (info.FieldType == output.GetType())//string类型需要添加单引号
          output += info.Name + "=" + "'" + info.GetValue(this) + "'" + "\n";
        else if (info.FieldType == ConfigBool.GetType())
          output += info.Name + "=" + ((bool)info.GetValue(this) ? "true" : "false") + "\n";
        else
          output += info.Name + "=" + info.GetValue(this) + "\n";
      }

      byte[] bytes = Encoding.UTF8.GetBytes(output);
      fileStream.Write(bytes, 0, bytes.Length);
    }

    public bool Read(string fileName)//返回是否读取成功
    {
      FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
      try {
        byte[] bytes = new byte[fileStream.Length];
        //读取文件信息
        fileStream.Read(bytes, 0, bytes.Length);
        //将得到的字节型数组重写编码为字符型数组
        char[] c = Encoding.UTF8.GetChars(bytes);
        String input = new String(c);
        //将字符串转换为toml
        var doc = Toml.Parse(input);

        if (doc.HasErrors) return false;//解析错误则返回失败
        var table = doc.ToModel();

        //自动导入所有属性，未成功  无法使用变量类型动态强制类型转换
        //Type t = typeof(TomlConfig);
        //FieldInfo[] infos = t.GetFields();
        //foreach (FieldInfo info in infos) {
        //  info.SetValue(this, ((info.FieldType)(TomlTable)table["TestTable"])[info.Name]);
        //}

        //自动导入所有属性，静态判断类型 成功
        Type t = typeof(TomlConfig);
        FieldInfo[] infos = t.GetFields();
        foreach (FieldInfo info in infos) {
          if (info.FieldType==ConfigLong.GetType())
            info.SetValue(this, (long)((TomlTable)table["TestTable"])[info.Name]);
          else if (info.FieldType==ConfigBool.GetType())
            info.SetValue(this, (bool)((TomlTable)table["TestTable"])[info.Name]);
          else if (info.FieldType==ConfigString.GetType())
            info.SetValue(this, (string)((TomlTable)table["TestTable"])[info.Name]);
          else if (info.FieldType == ConfigDouble.GetType())
            info.SetValue(this, (double)((TomlTable)table["TestTable"])[info.Name]);
        }

        //手动导入 成功
        //ConfigLong = (long)((TomlTable)table["TestTable"])[nameof(ConfigLong)];
        //ConfigBool = (bool)((TomlTable)table["TestTable"])[nameof(ConfigBool)];
        //ConfigString = (string)((TomlTable)table["TestTable"])[nameof(ConfigString)];
        //ConfigDouble = (double)((TomlTable)table["TestTable"])[nameof(ConfigDouble)];

        return true;
      }
      catch (Exception e) {
        Console.WriteLine(e);
        throw;
      }
    }
  }
}
