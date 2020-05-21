using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageDecoder.Bmp.Extern;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageDecoder.Bmp {
  public static class BmpDecoder {
    public static unsafe Bitmap BitmapFromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return BitmapFromPointer((IntPtr)dataptr, data.LongLength);
      }
    }

    public static Bitmap BitmapFromPointer(IntPtr data, long length) {
      var bmp = EasyBmpNative.DecodeFromBuffer(data, (UIntPtr) length);
      Bitmap b = null;
      BitmapData bd = null;
      var success = false;
      try {
        if (bmp.bmp == IntPtr.Zero) {
          return null;
        }

        b = new Bitmap(bmp.width, bmp.height, PixelFormat.Format32bppArgb);
        bd = b.LockBits(new Rectangle(0, 0, bmp.width, bmp.height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        if (!EasyBmpNative.WriteToMemory(ref bmp, bd.Scan0, (UIntPtr) bd.Stride)) {
          return null;
        }

        success = true;
      }
      finally {
        EasyBmpNative.FreeBMP(ref bmp);

        if (bd != null) {
          b.UnlockBits(bd);
        }

        if (!success) {
          b?.Dispose();
        }
      }

      return b;
    }

    public static unsafe WriteableBitmap WBitmapFromBytes(byte[] data, double dpi) {
      fixed (byte* dataptr = data) {
        return WBitmapFromPointer((IntPtr)dataptr, data.LongLength, dpi);
      }
    }

    public static WriteableBitmap WBitmapFromPointer(IntPtr data, long length, double dpi) {
      var bmp = EasyBmpNative.DecodeFromBuffer(data, (UIntPtr) length);
      WriteableBitmap b = null;

      try {
        if (bmp.bmp == IntPtr.Zero) {
          return null;
        }

        b = new WriteableBitmap(bmp.width, bmp.height, dpi, dpi, System.Windows.Media.PixelFormats.Bgra32, null);
        b.Lock();

        if (!EasyBmpNative.WriteToMemory(ref bmp, b.BackBuffer, (UIntPtr) b.BackBufferStride)) {
          return null;
        }

        b.AddDirtyRect(new Int32Rect(0, 0, bmp.width, bmp.height));
      }
      finally {
        EasyBmpNative.FreeBMP(ref bmp);
        b?.Unlock();
      }

      return b;
    }
  }
}
