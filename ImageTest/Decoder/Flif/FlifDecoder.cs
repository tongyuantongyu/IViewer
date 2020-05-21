using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageDecoder.Flif.Extern;
using ImageDecoder.Toolbox;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageDecoder.Flif {
  public static class FlifDecoder {
    public static unsafe Bitmap BitmapFromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return BitmapFromPointer((IntPtr) dataptr, data.LongLength);
      }
    }

    public static Bitmap BitmapFromPointer(IntPtr data, long length) {
      var decoder = LibFlifNative.FlifCreateDecoder();
      Bitmap b = null;
      BitmapData bd = null;
      var success = false;
      try {
        if (LibFlifNative.FlifDecoderDecodeMemory(decoder, data, (UIntPtr) length) == 0) {
          return null;
        }

        var image = LibFlifNative.FlifDecoderGetImage(decoder, UIntPtr.Zero);
        var width = LibFlifNative.FlifImageGetWidth(image);
        var height = LibFlifNative.FlifImageGetHeight(image);
        var bitDepth = LibFlifNative.FlifImageGetDepth(image);
        if (bitDepth > 8) {
          b = new Bitmap(width, height, PixelFormat.Format64bppArgb);
          bd = b.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.ReadWrite, PixelFormat.Format64bppArgb);
          for (var line = 0; line < height; line++) {
            var position = bd.Scan0 + bd.Stride * line;
            LibFlifNative.FlifImageReadRowRgba16(image, line, position, (UIntPtr) bd.Stride);
          }

          Tools.BBGGRRAA2RRGGBBAA(bd.Scan0, (UIntPtr) (width * height));
          Tools.GDICOLORFIX(bd.Scan0, (UIntPtr) (width * height * 4), 2.2);
        }
        else {
          b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
          bd = b.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
          for (var line = 0; line < height; line++) {
            var position = bd.Scan0 + bd.Stride * line;
            LibFlifNative.FlifImageReadRowRgba8(image, line, position, (UIntPtr) bd.Stride);
          }
          Tools.BGRA2RGBA(bd.Scan0, (UIntPtr) (width * height));
        }

        LibFlifNative.FlifDestroyImage(image);
        success = true;
      }
      finally {
        // LibFlifNative.FlifDestroyDecoder(decoder);

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
      WriteableBitmap b = null;
      var decoder = LibFlifNative.FlifCreateDecoder();
      try {
        if (LibFlifNative.FlifDecoderDecodeMemory(decoder, data, (UIntPtr) length) == 0) {
          return null;
        }

        var image = LibFlifNative.FlifDecoderGetImage(decoder, UIntPtr.Zero);
        var width = LibFlifNative.FlifImageGetWidth(image);
        var height = LibFlifNative.FlifImageGetHeight(image);
        var bitDepth = LibFlifNative.FlifImageGetDepth(image);
        if (bitDepth > 8) {
          b = new WriteableBitmap(width, height, dpi, dpi, System.Windows.Media.PixelFormats.Rgba64, null);
          b.Lock();
          for (var line = 0; line < height; line++) {
            var position = b.BackBuffer + b.BackBufferStride * line;
            LibFlifNative.FlifImageReadRowRgba16(image, line, position, (UIntPtr) b.BackBufferStride);
          }

        }
        else {
          b = new WriteableBitmap(width, height, dpi, dpi, System.Windows.Media.PixelFormats.Bgra32, null);
          b.Lock();
          for (var line = 0; line < height; line++) {
            var position = b.BackBuffer + b.BackBufferStride * line;
            LibFlifNative.FlifImageReadRowRgba8(image, line, position, (UIntPtr) b.BackBufferStride);
          }
          Tools.BGRA2RGBA(b.BackBuffer, (UIntPtr) (width * height));
        }

        LibFlifNative.FlifDestroyImage(image);
        b.AddDirtyRect(new Int32Rect(0, 0, width, height));
      }
      finally {
        // LibFlifNative.FlifDestroyDecoder(decoder);
        b?.Unlock();
      }

      return b;
    }
  }
}