using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageDecoder.Avif.Extern;
using ImageDecoder.Toolbox;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageDecoder.Avif {
  public static class AvifDecoder {
    public static unsafe Bitmap BitmapFromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return BitmapFromPointer((IntPtr)dataptr, data.LongLength);
      }
    }

    public static Bitmap BitmapFromPointer(IntPtr data, long length) {
      var file = new AvifRoData {data = data, size = (UIntPtr) length};
      var decoder = LibAvifNative.AvifDecoderCreate();
      Bitmap b = null;
      BitmapData bd = null;
      var success = false;
      try {
        var result = LibAvifNative.AvifDecoderParse(ref decoder, ref file);
        if (result != AvifResult.AvifResultOk) {
          return null;
        }

        result = LibAvifNative.AvifDecoderNextImage(ref decoder);
        if (result != AvifResult.AvifResultOk) {
          return null;
        }

        var image = Marshal.PtrToStructure<AvifImage>(decoder.image);
        var width = image.width;
        var height = image.height;
        var depth = image.depth;
        var hasAlpha = image.alphaPlane != IntPtr.Zero;

        AvifRgbFormat chroma;
        PixelFormat format;
        if (depth <= 8) {
          chroma = AvifRgbFormat.AvifRgbFormatBgra;
          format = PixelFormat.Format32bppArgb;
        }
        else if (hasAlpha) {
          chroma = AvifRgbFormat.AvifRgbFormatBgra;
          format = PixelFormat.Format64bppArgb;
        }
        else {
          chroma = AvifRgbFormat.AvifRgbFormatBgr;
          format = PixelFormat.Format48bppRgb;
        }

        var rgb = new AvifRGBImage();
        LibAvifNative.AvifRGBImageSetDefaults(ref rgb, ref image);

        b = new Bitmap(width, height, format);
        bd = b.LockBits(new Rectangle(0, 0, width, height),
          ImageLockMode.ReadWrite, format);

        rgb.format = chroma;
        rgb.depth = depth <= 8 ? 8 : 16;
        rgb.pixels = bd.Scan0;
        rgb.rowBytes = bd.Stride;

        result = LibAvifNative.AvifImageYUVToRGB(ref image, ref rgb);

        if (result != AvifResult.AvifResultOk) {
          return null;
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (format) {
          case PixelFormat.Format64bppArgb:
            Tools.GDICOLORFIX(bd.Scan0, (UIntPtr) (width * height * 4), 2.2);
            break;
          case PixelFormat.Format48bppRgb:
            Tools.GDICOLORFIX(bd.Scan0, (UIntPtr) (width * height * 3), 2.2);
            break;
        }

        success = true;
      }
      finally {
        AvifResult result;
        do {
          result = LibAvifNative.AvifDecoderNextImage(ref decoder);
        } while (result == AvifResult.AvifResultOk);

        LibAvifNative.AvifDecoderDestroy(ref decoder);

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
      var file = new AvifRoData {data = data, size = (UIntPtr) length};
      var decoder = LibAvifNative.AvifDecoderCreate();
      WriteableBitmap b = null;
      try {
        var result = LibAvifNative.AvifDecoderParse(ref decoder, ref file);
        if (result != AvifResult.AvifResultOk) {
          return null;
        }

        result = LibAvifNative.AvifDecoderNextImage(ref decoder);
        if (result != AvifResult.AvifResultOk) {
          return null;
        }

        var image = Marshal.PtrToStructure<AvifImage>(decoder.image);
        var width = image.width;
        var height = image.height;
        var depth = image.depth;
        var hasAlpha = image.alphaPlane != IntPtr.Zero;

        AvifRgbFormat chroma;
        System.Windows.Media.PixelFormat format;
        if (depth <= 8) {
          chroma = AvifRgbFormat.AvifRgbFormatBgra;
          format = System.Windows.Media.PixelFormats.Bgra32;
        }
        else if (hasAlpha) {
          chroma = AvifRgbFormat.AvifRgbFormatRgba;
          format = System.Windows.Media.PixelFormats.Rgba64;
        }
        else {
          chroma = AvifRgbFormat.AvifRgbFormatRgb;
          format = System.Windows.Media.PixelFormats.Rgb48;
        }

        var rgb = new AvifRGBImage();
        LibAvifNative.AvifRGBImageSetDefaults(ref rgb, ref image);

        b = new WriteableBitmap(width, height, dpi, dpi, format, null);
        b.Lock();

        rgb.format = chroma;
        rgb.depth = depth <= 8 ? 8 : 16;
        rgb.pixels = b.BackBuffer;
        rgb.rowBytes = b.BackBufferStride;

        result = LibAvifNative.AvifImageYUVToRGB(ref image, ref rgb);

        if (result != AvifResult.AvifResultOk) {
          return null;
        }

        b.AddDirtyRect(new Int32Rect(0, 0, width, height));

      }
      finally {
        LibAvifNative.AvifDecoderDestroy(ref decoder);
        b?.Unlock();
      }

      return b;
    }
  }
}
