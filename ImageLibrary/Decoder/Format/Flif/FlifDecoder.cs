using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageDecoder.Flif.Extern;
using ImageLibrary.Tools;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageLibrary.Decoder.Format.Flif {
  public static class FlifDecoder {
    public static DetectResult MagicDetect(byte[] header) {
      const string magic = "FLIF";
      return StringMagicDetect.Detect(magic, header);
    }

    public static unsafe IBitmapSource FromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return FromPointer((IntPtr)dataptr, data.LongLength);
      }
    }

    public static IBitmapSource FromPointer(IntPtr data, long length) {
      MemoryBitmapSource b;
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
          b = new MemoryBitmapSource(width, height, 16, 4);
          for (var line = 0; line < height; line++) {
            var position = b.Scan0 + b.Stride * line;
            LibFlifNative.FlifImageReadRowRgba16(image, line, position, (UIntPtr) b.Stride);
          }

        }
        else {
          b = new MemoryBitmapSource(width, height, 8, 4);
          for (var line = 0; line < height; line++) {
            var position = b.Scan0 + b.Stride * line;
            LibFlifNative.FlifImageReadRowRgba8(image, line, position, (UIntPtr) b.Stride);
          }
          PixelTool.BGRA2RGBA(b.Scan0, (UIntPtr) (width * height));
        }

        LibFlifNative.FlifDestroyImage(image);
      }
      finally {
        // LibFlifNative.FlifDestroyDecoder(decoder);
      }

      return b;
    }
  }
}