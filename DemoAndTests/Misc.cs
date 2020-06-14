using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DemoAndTests {
  public static class Misc {
    public static WriteableBitmap AllocWriteableBitmap(int width, int height, int depth, int channel) {
      PixelFormat format;
      if (depth == 8) {
        format =channel == 3 ? PixelFormats.Bgr24 : PixelFormats.Bgra32;
      }
      else {
        format = channel == 3 ? PixelFormats.Rgb48 : PixelFormats.Rgba64;
      }
      return new WriteableBitmap(width, height, 96, 96, format, null);
    }

    public static void CopyToWritableBitmap(WriteableBitmap wb, Bitmap b) {
      wb.Lock();
      
      var lineWidth = Math.Min(wb.BackBufferStride, b.Stride);
      var height = Math.Min(wb.PixelHeight, b.Height);

      // Parallel.For(0, height, i => {
      //   unsafe {
      //     Buffer.MemoryCopy((b.Scan0 + i * b.Stride).ToPointer(),
      //       (wb.BackBuffer + i * wb.BackBufferStride).ToPointer(),
      //       wb.BackBufferStride, lineWidth);
      //   }
      // });

      for (var i = 0; i < height; i++) {
        unsafe {
          Buffer.MemoryCopy((b.Scan0 + i * b.Stride).ToPointer(),
            (wb.BackBuffer + i * wb.BackBufferStride).ToPointer(),
            wb.BackBufferStride, lineWidth);
        }
      }
      
      wb.AddDirtyRect(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight));

      wb.Unlock();
    }

    public static void CopyFromWritableBitmap(WriteableBitmap wb, Bitmap b) {
      var lineWidth = Math.Min(wb.BackBufferStride, b.Stride);
      var height = Math.Min(wb.PixelHeight, b.Height);

      for (var i = 0; i < height; i++) {
        unsafe {
          Buffer.MemoryCopy((b.Scan0 + i * b.Stride).ToPointer(),
            (wb.BackBuffer + i * wb.BackBufferStride).ToPointer(),
            wb.BackBufferStride, lineWidth);
        }
      }
    }

    public static void CopyBitmap(Bitmap src, Bitmap dst) {
      var lineWidth = Math.Min(src.Stride, dst.Stride);
      var height = Math.Min(src.Height, dst.Height);

      for (var i = 0; i < height; i++) {
        unsafe {
          Buffer.MemoryCopy((src.Scan0 + i * src.Stride).ToPointer(),
            (dst.Scan0 + i * dst.Stride).ToPointer(),
            dst.Stride, lineWidth);
        }
      }
    }

    public static Bitmap BitmapOfWritableBitmap(WriteableBitmap wb) {
      int channel;
      int depth;
      if (wb.Format == PixelFormats.Bgr24) {
        channel = 3;
        depth = 8;
      }
      else if (wb.Format == PixelFormats.Bgra32 || wb.Format == PixelFormats.Bgr32) {
        channel = 4;
        depth = 8;
      }
      else if (wb.Format == PixelFormats.Rgb48) {
        channel = 3;
        depth = 16;
      }
      else if (wb.Format == PixelFormats.Rgba64) {
        channel = 4;
        depth = 16;
      }
      else {
        throw new ArgumentException($"Unsupported Pixel Format: {wb.Format}");
      }
      return new Bitmap(wb.BackBuffer, wb.BackBufferStride, wb.PixelWidth, wb.PixelHeight, depth, channel);
    }
  }
}
