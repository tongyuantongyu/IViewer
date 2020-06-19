using System;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace IViewer.Watcher {
  public class FileWatcher {
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public void Run() {
      using (var watcher = new FileSystemWatcher()) {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        watcher.Path = dir;
        watcher.Filter = App.ConfigLocation;

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
          Thread.Sleep(500);
        }
      }
    }

    private void OnChanged(object source, FileSystemEventArgs e) //修改时读取信息
    {
      Settings.TomlConfig.Read(App.ConfigLocation);
    }

    private void OnRenamed(object source, RenamedEventArgs e) //重命名时重新创建一个
    {
      ShowFileStatus(source, e);
      Settings.TomlConfig.Write(App.ConfigLocation);
    }

    private void OnCreated(object source, FileSystemEventArgs e) { }

    private void OnDeleted(object source, FileSystemEventArgs e) //删除时重新创建
    {
      ShowFileStatus(source, e);
      Settings.TomlConfig.Write(App.ConfigLocation);
    }

    private void ShowFileStatus(object source, FileSystemEventArgs e) { }
  }
}