using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageLibrary.Resizer {
  public class WPFResizer : IResizer {

    public static IResizer Resizer => new WPFResizer();

    public void Resize(Bitmap src, Bitmap dst, object options = null) {
      var source = Misc.AllocWriteableBitmap(src.Width, src.Height, src.Depth, src.Channel);
      Misc.CopyToWritableBitmap(source, src);
      var scaleTransform = new ScaleTransform((double)dst.Width / src.Width, (double)dst.Height / src.Height);
      var transformed = new TransformedBitmap(source, scaleTransform);
      transformed.CopyPixels(Int32Rect.Empty, dst.Scan0, dst.Stride * dst.Height, dst.Stride);
    }
  }
}
