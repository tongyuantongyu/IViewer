﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace IViewer {
  public class TomlViewModel:INotifyPropertyChanged {
    public TomlConfig tomlConfig;

    public string Result {
      get { return tomlConfig.ToString(); }
      set { ; }
    }
    public TomlViewModel() { tomlConfig=new TomlConfig();}

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged(string propertyName) {//属性更改方法
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Read() {//读文件
      tomlConfig.Read("test.toml");RaisePropertyChanged("Result");
    }

  }

}
