using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageLibrary.Decoder.Format.Bmp.Extern;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageLibrary.Decoder.Format.Bmp {
  public static class BmpDecoder {
    public static DetectResult MagicDetect(byte[] header) {
      const string magic = "BM????\x00\x00\x00\x00";
      return StringMagicDetect.Detect(magic, header);
    }

    public static unsafe IBitmapSource FromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return FromPointer((IntPtr)dataptr, data.LongLength);
      }
    }

    public static IBitmapSource FromPointer(IntPtr data, long length) {
      var bmp = EasyBmpNative.DecodeFromBuffer(data, (UIntPtr) length);
      MemoryBitmapSource b;

      try {
        if (bmp.bmp == IntPtr.Zero) {
          return null;
        }

        b = new MemoryBitmapSource(bmp.width, bmp.height, 8, 4);

        if (!EasyBmpNative.WriteToMemory(ref bmp, b.Scan0, (UIntPtr) b.Stride)) {
          return null;
        }
      }
      finally {
        EasyBmpNative.FreeBMP(ref bmp);
      }

      return b;
    }
  }
}
