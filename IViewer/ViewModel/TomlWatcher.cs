using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IViewer
{
    public class TomlWatcher
    {
        public string FileName;
        public TomlConfig tomlConfig;

        public TomlWatcher()//初始化文件和配置类
        {
            //FileName = "test.toml";
            //tomlConfig = new TomlConfig();
            //if (File.Exists(FileName))
            //{
            //    tomlConfig.Read(FileName);
            //}
            //else
            //{
            //    tomlConfig.Write(FileName);
            //}
        }

        public void W() {
            tomlConfig.Write(FileName);
        }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Run()
        {
          using (FileSystemWatcher watcher = new FileSystemWatcher()) {

            string dir = AppDomain.CurrentDomain.BaseDirectory;
            watcher.Path = dir;
            watcher.Filter = FileName;

            watcher.NotifyFilter = NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;

            watcher.EnableRaisingEvents = true;
            while (true) {
              System.Threading.Thread.Sleep(500);
            }
          }
        }

        private void OnChanged(object source, FileSystemEventArgs e)//修改时读取信息
        { 
            tomlConfig.Read(FileName);

        }

        private void OnRenamed(object source, RenamedEventArgs e)//重命名时重新创建一个
        {
            ShowFileStatus(source, e);
            tomlConfig.Write(FileName);
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            
        }

        private void OnDeleted(object source, FileSystemEventArgs e)//删除时重新创建
        {
            ShowFileStatus(source, e);
            tomlConfig.Write(FileName);
        }

        private void ShowFileStatus(object source, FileSystemEventArgs e)
        {
            
        }
    }
}

