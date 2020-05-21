using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageDecoder.Webp.Extern;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageDecoder.Webp {
  public static class WebpDecoder {
    public static unsafe Bitmap BitmapFromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return BitmapFromPointer((IntPtr)dataptr, data.LongLength);
      }
    }

    public static Bitmap BitmapFromPointer(IntPtr data, long length) {
      var features = new WebPBitstreamFeatures();

      if (LibWebpNative.WebPGetFeaturesInternal(data, (UIntPtr) length, ref features,
        LibWebpNative.WEBP_DECODER_ABI_VERSION) != VP8StatusCode.VP8_STATUS_OK) {
        throw new Exception("Failed.");
      }

      Bitmap b = null;
      BitmapData bd = null;
      var success = false;

      try {
        b = new Bitmap(features.width, features.height, PixelFormat.Format32bppArgb);
        bd = b.LockBits(new Rectangle(0, 0, features.width, features.height),
          ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        var config = new WebPDecoderConfig();
        if (LibWebpNative.WebPInitDecoderConfigInternal(ref config, LibWebpNative.WEBP_DECODER_ABI_VERSION) == 0) {
          throw new Exception("Failed.");
        }

        config.output.colorspace = WEBP_CSP_MODE.MODE_BGRA;
        config.output.u.RGBA.rgba = bd.Scan0;
        config.output.u.RGBA.stride = bd.Stride;
        config.output.u.RGBA.size = (UIntPtr) (bd.Stride * bd.Height);
        config.output.is_external_memory = 1;
        if (LibWebpNative.WebPDecode(data, (UIntPtr) length, ref config) != VP8StatusCode.VP8_STATUS_OK) {
          throw new Exception("Failed.");
        }

        success = true;
      }
      finally {
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
      var features = new WebPBitstreamFeatures();

      if (LibWebpNative.WebPGetFeaturesInternal(data, (UIntPtr) length, ref features,
        LibWebpNative.WEBP_DECODER_ABI_VERSION) != VP8StatusCode.VP8_STATUS_OK) {
        throw new Exception("Failed.");
      }

      WriteableBitmap b = null;

      try {
        b = new WriteableBitmap(features.width, features.height, dpi, dpi, System.Windows.Media.PixelFormats.Bgra32, null);
        var config = new WebPDecoderConfig();
        if (LibWebpNative.WebPInitDecoderConfigInternal(ref config, LibWebpNative.WEBP_DECODER_ABI_VERSION) == 0) {
          throw new Exception("Failed.");
        }

        b.Lock();

        config.output.colorspace = WEBP_CSP_MODE.MODE_BGRA;
        config.output.u.RGBA.rgba = b.BackBuffer;
        config.output.u.RGBA.stride = b.BackBufferStride;
        config.output.u.RGBA.size = (UIntPtr) (b.BackBufferStride * features.height);
        config.output.is_external_memory = 1;
        var r = LibWebpNative.WebPDecode(data, (UIntPtr) length, ref config);
        if (r != VP8StatusCode.VP8_STATUS_OK) {
          throw new Exception("Failed.");
        }

        b.AddDirtyRect(new Int32Rect(0, 0, features.width, features.height));
      }
      finally {
        b?.Unlock();
      }

      return b;
    }
  }
}
