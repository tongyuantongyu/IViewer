using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Ink;

namespace ImageLibrary.Resizer {
  public class GDIResizer : IResizer {
    public static IResizer Resizer => new GDIResizer();
    public void Resize(Bitmap src, Bitmap dst, object options = null) {
      var mode = InterpolationMode.HighQualityBicubic;
      if (options != null) {
        if (options is InterpolationMode m) {
          mode = m;
        }
        else {
          throw new ArgumentException("Bad option.");
        }
      }

      // // run through scaler
      // var bitmap = new System.Drawing.Bitmap(dst.Width, dst.Height, GetSystemPixelFormat(dst));
      // using (var graphics = Graphics.FromImage(bitmap)) {
      //
      //   graphics.CompositingQuality = CompositingQuality.HighQuality;
      //   graphics.InterpolationMode = mode;
      //   graphics.SmoothingMode = SmoothingMode.HighQuality;
      //
      //   var srcBitmap = ToSystemBitmap(src);
      //
      //   graphics.DrawImage(srcBitmap, -1, -1, srcBitmap.Width + 1, srcBitmap.Height + 1);
      // }

      var destRect = new Rectangle(0, 0, dst.Width, dst.Height);
      var destBitmap = new System.Drawing.Bitmap(dst.Width, dst.Height, GetSystemPixelFormat(dst));
      var srcBitmap = ToSystemBitmap(src);

      destBitmap.SetResolution(96, 96);

      using (var graphics = Graphics.FromImage(destBitmap)) {
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = mode;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using (var wrapMode = new ImageAttributes()) {
          wrapMode.SetWrapMode(WrapMode.TileFlipXY);
          graphics.DrawImage(srcBitmap, destRect, 0, 0, srcBitmap.Width, srcBitmap.Height, GraphicsUnit.Pixel, wrapMode);
        }
      }

      FromSystemBitmap(destBitmap, dst);
      srcBitmap.Dispose();
      destBitmap.Dispose();
    }

    private static PixelFormat GetSystemPixelFormat(Bitmap src) {
      if (src.Depth == 8) {
        return src.Channel == 3 ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;
      }

      return src.Channel == 3 ? PixelFormat.Format48bppRgb : PixelFormat.Format64bppArgb;
    }

    private static System.Drawing.Bitmap ToSystemBitmap(Bitmap src) {
      var format = GetSystemPixelFormat(src);

      var bitmap = new System.Drawing.Bitmap(src.Width, src.Height, format);

      var bitmapData = bitmap.LockBits(
        new Rectangle(0, 0, src.Width, src.Height),
        ImageLockMode.WriteOnly,
        format
      );

      for (var i = 0; i < src.Height; i++) {
        unsafe {
          Buffer.MemoryCopy((src.Scan0 + i * src.Stride).ToPointer(),
            (bitmapData.Scan0 + i * bitmapData.Stride).ToPointer(),
            bitmapData.Stride, (src.Channel * src.Depth * src.Width) >> 3);
        }
      }

      bitmap.UnlockBits(bitmapData);

      return bitmap;
    }

    private static void FromSystemBitmap(System.Drawing.Bitmap src, Bitmap dst) {
      var bitmapData = src.LockBits(
        new Rectangle(0, 0, src.Width, src.Height),
        ImageLockMode.ReadOnly,
        src.PixelFormat
      );

      for (var i = 0; i < src.Height; i++) {
        unsafe {
          Buffer.MemoryCopy((bitmapData.Scan0 + i * bitmapData.Stride).ToPointer(),
            (dst.Scan0 + i * dst.Stride).ToPointer(),
            dst.Stride, (dst.Channel * dst.Depth * dst.Width) >> 3);
        }
      }

      src.UnlockBits(bitmapData);
    }
  }
}
