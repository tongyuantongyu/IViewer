using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageLibrary.Decoder.Format.Webp.Extern;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageLibrary.Decoder.Format.Webp {
  public static class WebpDecoder {
    public static DetectResult MagicDetect(byte[] header) {
      const string magic = "RIFF????WEBPVP8";
      return StringMagicDetect.Detect(magic, header);
    }

    public static unsafe IBitmapSource FromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return FromPointer((IntPtr)dataptr, data.LongLength);
      }
    }

    public static IBitmapSource FromPointer(IntPtr data, long length) {
      var features = new WebPBitstreamFeatures();

      if (LibWebpNative.WebPGetFeaturesInternal(data, (UIntPtr) length, ref features,
        LibWebpNative.WEBP_DECODER_ABI_VERSION) != VP8StatusCode.VP8_STATUS_OK) {
        throw new Exception("Failed.");
      }

      var b = new MemoryBitmapSource(features.width, features.height, 8, 4);
      var config = new WebPDecoderConfig();
      if (LibWebpNative.WebPInitDecoderConfigInternal(ref config, LibWebpNative.WEBP_DECODER_ABI_VERSION) == 0) {
        throw new Exception("Failed.");
      }

      config.output.colorspace = WEBP_CSP_MODE.MODE_BGRA;
      config.output.u.RGBA.rgba = b.Scan0;
      config.output.u.RGBA.stride = b.Stride;
      config.output.u.RGBA.size = (UIntPtr) (b.Stride * features.height);
      config.output.is_external_memory = 1;
      var r = LibWebpNative.WebPDecode(data, (UIntPtr) length, ref config);
      if (r != VP8StatusCode.VP8_STATUS_OK) {
        throw new Exception("Failed.");
      }

      return b;
    }
  }
}
