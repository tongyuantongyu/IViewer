using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ImageDecoder.Heif;

namespace WpfApp1 {
  /// <summary>
  ///   MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow {
    private bool isUpToolBarUp; //布尔量，判断顶栏移动方向

    public MainWindow() {
      InitializeComponent();
      var isUpToolBarUp = true; //顶栏移动初始方向向上
      UpToolBarAnimation(isUpToolBarUp);
    }

    private void UpToolBarAnimation(bool isUpToolBarUp) //顶栏动画
    {
      var tt = new TranslateTransform(); //创建一个一个对象，对两个值在时间线上进行动画处理（移动距离，移动到的位置）
      var da = new DoubleAnimation(); //设定动画时间线
      var duration = new Duration(TimeSpan.FromSeconds(0.2)); //btnFlash要进行动画操作的控件名
      UpToolBar.RenderTransform = tt;
      if (isUpToolBarUp) //移出
      {
        tt.Y = 0; //开始
        da.To = -80; //结束
      }
      else //移入
      {
        tt.Y = -80;
        da.To = 0;
      }

      da.Duration = duration;
      //开始进行动画处理
      tt.BeginAnimation(TranslateTransform.YProperty, da);
    }

    private void MainWindow1_MouseMove(object sender, MouseEventArgs e) {
      var pt = e.GetPosition(this); //GetPosition返回相对指定元素的鼠标位置
      if (pt.Y > 50 && isUpToolBarUp) //鼠标移出框
      {
        UpToolBarAnimation(isUpToolBarUp);
        isUpToolBarUp = !isUpToolBarUp;
      }

      if (pt.Y < 50 && !isUpToolBarUp) //鼠标移入框
      {
        UpToolBarAnimation(isUpToolBarUp);
        isUpToolBarUp = !isUpToolBarUp;
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
      var d = File.ReadAllBytes("sayo.heic");
      var dpi = GetDPI();
      var bitmap = HeifDecoder.WBitmapFromBytes(d, dpi);
      Pic.Source = bitmap;
    }

    private void Window_KeyUp(object sender, KeyEventArgs e) {
      switch (e.Key) {
        case Key.Escape:
          Close();
          break;
        case Key.Space:
          switch (WindowState) {
            case WindowState.Normal:
              WindowState = WindowState.Maximized;
              break;
            case WindowState.Maximized:
              WindowState = WindowState.Normal;
              break;
          }

          break;
      }
    }

    private void Pic_MouseDown(object sender, MouseButtonEventArgs e) {
      var pos = Mouse.GetPosition(this);
      if (4 <= pos.Y && pos.Y <= 40 && 4 <= pos.X && pos.X <= Width - 4 && WindowState == WindowState.Normal) {
        DragMove();
      }
    }
  }
}