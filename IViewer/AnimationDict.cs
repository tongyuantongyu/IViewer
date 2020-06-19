using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
// ReSharper disable MemberCanBePrivate.Global

namespace IViewer {
  static class AnimationDict {
    public static readonly ThicknessAnimationUsingKeyFrames TopBarShowAnimation =
      new ThicknessAnimationUsingKeyFrames {
        KeyFrames = new ThicknessKeyFrameCollection {
          // new EasingThicknessKeyFrame(new Thickness(0, -30, 0, 0), KeyTime.FromTimeSpan(TimeSpan.Zero),
          //   new CubicEase()),
          new EasingThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150)),
            new CubicEase())
        }
      };

    public static readonly ThicknessAnimationUsingKeyFrames TopBarHideAnimation =
      new ThicknessAnimationUsingKeyFrames {
        KeyFrames = new ThicknessKeyFrameCollection {
          new EasingThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)),
            new CubicEase()),
          new EasingThicknessKeyFrame(new Thickness(0, -30, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(650)),
            new CubicEase())
        }
      };

    public static readonly DoubleAnimationUsingKeyFrames FadeInAnimation =
      new DoubleAnimationUsingKeyFrames {
        KeyFrames = new DoubleKeyFrameCollection {
          new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)))
        }
      };

    public static readonly DoubleAnimationUsingKeyFrames FadeOutAnimation =
      new DoubleAnimationUsingKeyFrames {
        KeyFrames = new DoubleKeyFrameCollection {
          new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500))),
          new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(700)))
        }
      };
  }

  // Following code comes from http://pwlodek.blogspot.com/2010/12/matrixanimation-for-wpf.html
  public class MatrixAnimation : MatrixAnimationBase {
    public static readonly DependencyProperty FromProperty =
      DependencyProperty.Register("From", typeof(Matrix?), typeof(MatrixAnimation),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ToProperty =
      DependencyProperty.Register("To", typeof(Matrix?), typeof(MatrixAnimation),
        new PropertyMetadata(null));

    public static readonly DependencyProperty EasingFunctionProperty =
      DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(MatrixAnimation),
        new UIPropertyMetadata(null));

    public MatrixAnimation() { }

    public MatrixAnimation(Matrix toValue, Duration duration) {
      To = toValue;
      Duration = duration;
    }

    public MatrixAnimation(Matrix toValue, Duration duration, FillBehavior fillBehavior) {
      To = toValue;
      Duration = duration;
      FillBehavior = fillBehavior;
    }

    public MatrixAnimation(Matrix fromValue, Matrix toValue, Duration duration) {
      From = fromValue;
      To = toValue;
      Duration = duration;
    }

    public MatrixAnimation(Matrix fromValue, Matrix toValue, Duration duration, FillBehavior fillBehavior) {
      From = fromValue;
      To = toValue;
      Duration = duration;
      FillBehavior = fillBehavior;
    }

    public Matrix? From {
      set { SetValue(FromProperty, value); }
      get { return (Matrix)GetValue(FromProperty); }
    }

    public Matrix? To {
      set { SetValue(ToProperty, value); }
      get { return (Matrix)GetValue(ToProperty); }
    }

    public IEasingFunction EasingFunction {
      get { return (IEasingFunction)GetValue(EasingFunctionProperty); }
      set { SetValue(EasingFunctionProperty, value); }
    }

    protected override Freezable CreateInstanceCore() {
      return new MatrixAnimation();
    }

    protected override Matrix GetCurrentValueCore(Matrix defaultOriginValue, Matrix defaultDestinationValue,
      AnimationClock animationClock) {
      if (animationClock.CurrentProgress == null) {
        return Matrix.Identity;
      }

      var normalizedTime = animationClock.CurrentProgress.Value;
      if (EasingFunction != null) {
        normalizedTime = EasingFunction.Ease(normalizedTime);
      }

      var from = From ?? defaultOriginValue;
      var to = To ?? defaultDestinationValue;

      var newMatrix = new Matrix(
        ((to.M11 - from.M11) * normalizedTime) + from.M11,
        ((to.M12 - from.M12) * normalizedTime) + from.M12,
        ((to.M21 - from.M21) * normalizedTime) + from.M21,
        ((to.M22 - from.M22) * normalizedTime) + from.M22,
        ((to.OffsetX - from.OffsetX) * normalizedTime) + from.OffsetX,
        ((to.OffsetY - from.OffsetY) * normalizedTime) + from.OffsetY);

      return newMatrix;
    }
  }
}