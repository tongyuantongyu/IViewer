using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageLibrary.Decoder.Format.Wpf {
  class WpfDecoder {
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
      
      return new WPFBitmapSource(image);
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
      
      return new WPFBitmapSource(image);
    }
  }
}
