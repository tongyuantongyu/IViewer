using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageLibrary.Decoder.Format.Wpf {
  public static class WpfDecoder {
    public static IBitmapSource FromBytes(byte[] data) {
      var image = new BitmapImage();
      using (var mem = new MemoryStream(data)) {
        mem.Position = 0;
        image.BeginInit();
        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = null;
        image.StreamSource = mem;
        image.EndInit();
      }
      image.Freeze();
      
      return fromBitmapSource(image);
    }

    public static IBitmapSource FromStream(Stream stream) {
      var image = new BitmapImage();
      stream.Position = 0;
      image.BeginInit();
      image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
      image.CacheOption = BitmapCacheOption.OnLoad;
      image.UriSource = null;
      image.StreamSource = stream;
      image.EndInit();
      image.Freeze();

      return fromBitmapSource(image);
    }

    private static MemoryBitmapSource fromBitmapSource(BitmapSource image) {
      WriteableBitmap wb;

      if (image.Format == PixelFormats.Bgr24 || image.Format == PixelFormats.Bgra32) {
        wb = new WriteableBitmap(image);
      }
      else {
        var hasAlpha = image.Format == PixelFormats.Pbgra32 ||
                       image.Format == PixelFormats.Prgba64 ||
                       image.Format == PixelFormats.Rgba64 ||
                       image.Format == PixelFormats.Prgba128Float ||
                       image.Format == PixelFormats.Rgba128Float;

        var converted = new FormatConvertedBitmap();
        converted.BeginInit();
        converted.Source = image;
        converted.DestinationFormat = hasAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr24;
        converted.EndInit();

        wb = new WriteableBitmap(converted);
      }

      wb.Freeze();
      
      var img = new MemoryBitmapSource(wb.PixelWidth, wb.PixelHeight, 8, wb.Format.BitsPerPixel >> 3);
      Misc.CopyFromWritableBitmap(wb, img.FullBitmap);

      return img;
    }
  }
}
