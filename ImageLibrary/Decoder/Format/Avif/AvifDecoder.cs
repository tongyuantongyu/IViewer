using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageLibrary.Decoder.Format.Avif.Extern;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageLibrary.Decoder.Format.Avif {
  public static class AvifDecoder {
    public static DetectResult MagicDetect(byte[] header) {
      // TODO: Use Metadata extractor to detect format
      return DetectResult.NotSure;
    }

    public static unsafe IBitmapSource FromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return FromPointer((IntPtr)dataptr, data.LongLength);
      }
    }
    
    public static IBitmapSource FromPointer(IntPtr data, long length) {
      var file = new AvifRoData {data = data, size = (UIntPtr) length};
      var decoder = LibAvifNative.AvifDecoderCreate();
      MemoryBitmapSource b;
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
        int targetDepth;
        int targetChannel;
        if (depth <= 8) {
          chroma = AvifRgbFormat.AvifRgbFormatBgra;
          targetDepth = 8;
          targetChannel = 4;
        }
        else if (hasAlpha) {
          chroma = AvifRgbFormat.AvifRgbFormatRgba;
          targetDepth = 16;
          targetChannel = 4;
        }
        else {
          chroma = AvifRgbFormat.AvifRgbFormatRgb;
          targetDepth = 16;
          targetChannel = 3;
        }

        var rgb = new AvifRGBImage();
        LibAvifNative.AvifRGBImageSetDefaults(ref rgb, ref image);

        b = new MemoryBitmapSource(width, height, targetDepth, targetChannel);

        rgb.format = chroma;
        rgb.depth = depth <= 8 ? 8 : 16;
        rgb.pixels = b.Scan0;
        rgb.rowBytes = b.Stride;

        result = LibAvifNative.AvifImageYUVToRGB(ref image, ref rgb);

        if (result != AvifResult.AvifResultOk) {
          return null;
        }

      }
      finally {
        LibAvifNative.AvifDecoderDestroy(ref decoder);
      }

      return b;
    }
  }
}
