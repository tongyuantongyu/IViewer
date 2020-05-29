using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Decoder {
  public enum DetectResult {
    Yes,
    No,
    NotSure
  }

  // public abstract class ImageDecoder {
  //   public abstract DetectResult MagicDetect(byte[] header);
  //   public abstract IImageSource FromBytes(byte[] data);
  //   public abstract IImageSource FromPointer(IntPtr data, long length);
  // }

  internal static class StringMagicDetect {
    public static DetectResult Detect(string magic, byte[] header) {
      var matchedResult = header.Length >= magic.Length ? DetectResult.Yes : DetectResult.NotSure;
      return magic
        .Zip(header, (except, real) => except == '?' || (byte)except == real)
        .All(x => x) ? matchedResult : DetectResult.No;
    }
  }
}
