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

    public void Write(string fileName) //写文件
    {
      using (FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate)) {
        //动态添加所有属性
        string output = "" + @"[TestTable]" + "\n";

        Type t = typeof(TomlConfig);
        FieldInfo[] infos = t.GetFields();
        foreach (FieldInfo info in infos) {
          if (info.FieldType == ConfigString.GetType()) //string类型需要添加单引号
            output += info.Name + "=" + "'" + info.GetValue(this) + "'" + "\n";
          else if (info.FieldType == ConfigBool.GetType()) //bool类型需要小写
            output += info.Name + "=" + ((bool)info.GetValue(this) ? "true" : "false") + "\n";
          else
            output += info.Name + "=" + info.GetValue(this) + "\n";
        }

        byte[] bytes = Encoding.UTF8.GetBytes(output);
        fileStream.Write(bytes, 0, bytes.Length);
      }
    }

    public bool Read(string fileName) //返回是否读取成功
    {
      using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {

        try {
          byte[] bytes = new byte[fileStream.Length];
          //读取文件信息
          fileStream.Read(bytes, 0, bytes.Length);
          //将得到的字节型数组重写编码为字符型数组
          char[] c = Encoding.UTF8.GetChars(bytes);
          String input = new String(c);
          //将字符串转换为toml
          var doc = Toml.Parse(input);

          if (doc.HasErrors) return false; //解析错误则返回失败
          var table = doc.ToModel();
          Console.WriteLine(doc.ToString());

          //反射自动导入所有属性
          Type t = typeof(TomlConfig);
          FieldInfo[] infos = t.GetFields();
          foreach (FieldInfo info in infos) {
            info.SetValue(this, Convert.ChangeType(((TomlTable)table["TestTable"])[info.Name], info.FieldType));
          }

          return true;
        }
        catch (Exception e) {
          Console.WriteLine(e);
          throw;
        }
      }
    }
  }
}
