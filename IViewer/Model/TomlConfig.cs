using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace IViewer
{
  public class TomlConfig
    {
        //Test Values
        public long ConfigLong;
        public bool ConfigBool;
        public string ConfigString;
        public double ConfigDouble;

        public TomlConfig()
        {
            ConfigLong = 15;
            ConfigBool = true;
            ConfigString = "dzyTest2";
            ConfigDouble = 3.14159265;
        }

        public override string ToString() {
            return GenerateTomlString();
        }
        public string GenerateTomlString()//生成toml字符串
        {
            string TableName = "TestTable";
            //动态添加所有属性
            string output = "[" + TableName + "]\n";
            Type t = typeof(TomlConfig);
            FieldInfo[] infos = t.GetFields();
            foreach (FieldInfo info in infos)
            {
                if (info.FieldType == ConfigString.GetType())//string类型需要添加单引号
                    output += info.Name + "=" + "'" + info.GetValue(this) + "'" + "\n";
                else if (info.FieldType == ConfigBool.GetType())//bool类型需要小写
                    output += info.Name + "=" + ((bool)info.GetValue(this) ? "true" : "false") + "\n";
                else
                    output += info.Name + "=" + info.GetValue(this) + "\n";
            }
            return output;
        }
        public void Write(string fileName)//写文件
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create,FileAccess.Write))
            {
                string output = GenerateTomlString();
                byte[] bytes = Encoding.UTF8.GetBytes(output);
                fileStream.Write(bytes, 0, bytes.Length);
            }
        }

        public string ReadDoc(string fileName) //读取文件返回字符串，如果文件不存在，则创建文件写入默认值返回null
        {
            if (File.Exists(fileName))//文件存在
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[fileStream.Length];
                    //读取文件信息
                    fileStream.Read(bytes, 0, bytes.Length);
                    //将得到的字节型数组重写编码为字符型数组
                    char[] c = Encoding.UTF8.GetChars(bytes);
                    String input = new String(c);
                    return input;
                }
            }
            else//文件不存在,创建文件，写入默认值
            {
                Write(fileName);
                return null;
            }

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

          //反射自动导入所有属性
          Type t = typeof(TomlConfig);
          FieldInfo[] infos = t.GetFields();
          foreach (FieldInfo info in infos) {
            if (table.ContainsKey(info.Name)) {
              info.SetValue(this, Convert.ChangeType(((TomlTable)table[TableName])[info.Name], info.FieldType));
            }
            else {
              return false;//key错误则返回
            }
          }

          return true;
        }

        public bool Read(string fileName) //处理读取的文件，返回是否读取成功
        {
          String input = ReadDoc(fileName); //读取文件
          if (input != null) {
            if (!ReadString(input)) {
              Write(fileName);
              return false;
            }
            else {
              return true;
            }
          }
          Write(fileName);
          return false;
        }

    }

}
