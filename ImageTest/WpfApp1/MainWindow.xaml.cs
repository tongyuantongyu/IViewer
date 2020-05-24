using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Serialization;
using ImageDecoder.Heif;
using WpfAppImageTest;


namespace WpfApp1 {
  /// <summary>
  ///   MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow {
    //顶栏动画
    private static readonly ThicknessAnimationUsingKeyFrames TopBarShowAnimation = new ThicknessAnimationUsingKeyFrames {
      KeyFrames = new ThicknessKeyFrameCollection {
        new EasingThicknessKeyFrame(new Thickness(0, -30, 0, 0), KeyTime.FromTimeSpan(TimeSpan.Zero),
          new CubicEase()),
        new EasingThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150)),
          new CubicEase())
      }
    };

    private static readonly ThicknessAnimationUsingKeyFrames TopBarHideAnimation = new ThicknessAnimationUsingKeyFrames {
      KeyFrames = new ThicknessKeyFrameCollection {
        new EasingThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)),
          new CubicEase()),
        new EasingThicknessKeyFrame(new Thickness(0, -30, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(650)),
          new CubicEase())
      }
    };
    private void TopBarAnimation(bool show) {
      TopBar.BeginAnimation(MarginProperty, show ? TopBarShowAnimation : TopBarHideAnimation);
    }

    //动画相关布尔量
    private bool showTopbar;

    public MainWindow() {
      InitializeComponent();
      //初始化
      showTopbar = false;
    }

    private void MainWindow1_MouseMove(object sender, MouseEventArgs e) {
      var pos = e.GetPosition(this);
      if (showTopbar == (pos.Y < 60)) {
        return;
      }
      showTopbar = !showTopbar;
      TopBarAnimation(showTopbar);

    }

    private void SwitchWindowState() {
      switch (WindowState) {
        case WindowState.Normal:
          ResizeMode = ResizeMode.NoResize;
          WindowState = WindowState.Maximized;
          MaxButton.Content = "\xE923";
          break;
        case WindowState.Maximized:
          ResizeMode = ResizeMode.CanResize;
          WindowState = WindowState.Normal;
          MaxButton.Content = "\xE739";
          break;
      }
    }

    private double GetDPI() {
      Matrix matrix;
      var source = PresentationSource.FromVisual(this);
      if (source != null) {
        matrix = source.CompositionTarget.TransformToDevice;
      }
      else {
        using (var src = new HwndSource(new HwndSourceParameters())) {
          matrix = src.CompositionTarget.TransformToDevice;
        }
      }

      return matrix.M11 * 96;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
    }

    private void Window_KeyUp(object sender, KeyEventArgs e) {
      switch (e.Key) {
        case Key.Escape:
          Close();
          break;
        case Key.Space:
          SwitchWindowState();
          break;
      }
    }

    private void Close_Click(object sender, RoutedEventArgs e) {
      Close();
    }

    private void Max_Click(object sender, RoutedEventArgs e) {
      SwitchWindowState();
    }

    private void Min_Click(object sender, RoutedEventArgs e) {
      WindowState = WindowState.Minimized;
    }

    private void GripMove(object sender, MouseButtonEventArgs e) {
      if (e.ButtonState == MouseButtonState.Pressed) {
        DragMove();
      }
    }

    private void Grip_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      SwitchWindowState();
    }

    private void Grip_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      if (e.ButtonState == MouseButtonState.Pressed && WindowState == WindowState.Normal) {
        DragMove();
      }
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e) {//打开文件
      Microsoft.Win32.OpenFileDialog op = new Microsoft.Win32.OpenFileDialog();
      op.InitialDirectory = @"%userprofile%";//初始目录
      op.RestoreDirectory = true;
      op.Filter = "heic图片(*.heic)|*.heic|所有文件(*.*)|*.*";//文件类型选项
      op.FilterIndex = 1;//默认为第一项
      if (op.ShowDialog() == true) {
        //获取文件名
        string FileName = op.FileName;
        //OpenImgFile(FileName);
        OpenImgFile("trans.heic");
      }
    }

    private void OpenImgFile(string FileName) {
      var d = File.ReadAllBytes(FileName);
      var dpi = GetDPI();
      var bitmap = HeifDecoder.WBitmapFromBytes(d, dpi);
      Pic.Source = bitmap;
      Pic.Stretch = Stretch.UniformToFill;
    }

    //图片拖拽相关
    private bool bigOrSmall = true;//标记当前双击事件放大或缩小

    private bool isMouseLeftButtonDown = false;
    Point previousMousePoint = new Point(0, 0);
    private void img_MouseDown(object sender, MouseButtonEventArgs e) {
      Point centerPoint = e.GetPosition(Pic);
      isMouseLeftButtonDown = true;
      previousMousePoint = e.GetPosition(Pic);
      if (e.ClickCount >= 2) {
        PicScaleTransform.CenterX = centerPoint.X;
        PicScaleTransform.CenterY = centerPoint.Y;
        if (bigOrSmall) {
          PicScaleTransform.ScaleX += 1;
          PicScaleTransform.ScaleY += 1;
          bigOrSmall = false;
        }
        else {
          PicScaleTransform.ScaleX -= 1;
          PicScaleTransform.ScaleY -= 1;
          bigOrSmall = true;
        }
      }
    }

    private void img_MouseUp(object sender, MouseButtonEventArgs e) {
      isMouseLeftButtonDown = false;
    }

    private void img_MouseLeave(object sender, MouseEventArgs e) {
      isMouseLeftButtonDown = false;
    }

    private void img_MouseMove(object sender, MouseEventArgs e) {
      if (isMouseLeftButtonDown == true) {
        Point position = e.GetPosition(Pic);
        PicTranslateTransform.X += position.X - this.previousMousePoint.X;
        PicTranslateTransform.Y += position.Y - this.previousMousePoint.Y;
      }
    }

    private void img_MouseWheel(object sender, MouseWheelEventArgs e) {//滚轮缩放事件
      Point centerPoint = e.GetPosition(Pic);

      double val = (double)e.Delta / 2000;   //描述鼠标滑轮滚动
      if (PicScaleTransform.ScaleX < 0.3 && PicScaleTransform.ScaleY < 0.3 && e.Delta < 0) {
        return;
      }
      PicScaleTransform.CenterX = centerPoint.X;
      PicScaleTransform.CenterY = centerPoint.Y;
      PicScaleTransform.ScaleX += val;
      PicScaleTransform.ScaleY += val;
    }
    private Setting setting = new Setting();
    //导入设置
    private void ImportSetting(object sender, RoutedEventArgs e) {
      string FileName = "Setting.xml";
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(Setting));
      using (FileStream fs = new FileStream(FileName, FileMode.Open)) {
        setting = (Setting)xmlSerializer.Deserialize(fs);
      }
    }
    //导出设置
    private void ExportSetting(object sender, RoutedEventArgs e) {
      string FileName = "Setting.xml";
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(Setting));
      using (FileStream fs = new FileStream(FileName, FileMode.Create)) {
        xmlSerializer.Serialize(fs, setting);
      }
    }
  }


  // public class Bindable<T> : INotifyPropertyChanged {
  //   public event PropertyChangedEventHandler PropertyChanged;
  //   private T data;
  //   public T Data {
  //     get => data;
  //     set {
  //       if (data.Equals(value)) {
  //         return;
  //       }
  //       data = value;
  //       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Data"));
  //     }
  //   }
  //   public Bindable(T value) {
  //     data = value;
  //   }
  //
  //   public Bindable() { }
  // }
}