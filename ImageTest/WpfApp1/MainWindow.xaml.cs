using System;
using System.ComponentModel;
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

    private bool showTopbar = false;

    public MainWindow() {
      InitializeComponent();
    }

    private void TopBarAnimation(bool show) {
      TopBar.BeginAnimation(MarginProperty, show ? TopBarShowAnimation : TopBarHideAnimation);
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
      var d = File.ReadAllBytes("trans.heic");
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