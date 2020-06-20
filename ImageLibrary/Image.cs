using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageLibrary.Decoder;
using ImageLibrary.Decoder.Format.Avif;
using ImageLibrary.Decoder.Format.Bmp;
using ImageLibrary.Decoder.Format.Flif;
using ImageLibrary.Decoder.Format.Heif;
using ImageLibrary.Decoder.Format.Webp;
using ImageLibrary.Decoder.Format.Wpf;
using ImageLibrary.Resizer;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Util;

namespace ImageLibrary {
  public class Image {
    private IBitmapSource source;

    private Image() { }

    // TODO: use more concrete data container
    public Metadata Metadata { get; private set; }

    public int Width => source.Width;
    public int Height => source.Height;

    public static Image FromFile(string path) {
      var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
      var image = new Image();

      try {
        var format = FileTypeDetector.DetectFileType(stream);
        stream.Seek(0, SeekOrigin.Begin);
        image.Metadata = new Metadata(path);
        stream.Seek(0, SeekOrigin.Begin);
        byte[] data;

        switch (format) {
          case FileType.Bmp:
            data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            image.source = BmpDecoder.FromBytes(data);
            break;
          case FileType.WebP:
            data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            image.source = WebpDecoder.FromBytes(data);
            break;
          case FileType.QuickTime:
            var qtFt = image.Metadata.Directories
              .OfType<QuickTimeFileTypeDirectory>()
              .FirstOrDefault()
              ?.GetDescription(1);
            if (qtFt != "avif" && qtFt != "avis" && qtFt != "av01") {
              return null;
            }

            data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            image.source = AvifDecoder.FromBytes(data);
            break;
          case FileType.Heif:
            data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            image.source = HeifDecoder.FromBytes(data);
            break;
          case FileType.Jpeg:
          case FileType.Tiff:
          case FileType.Png:
          case FileType.Gif:
          case FileType.Ico:
            image.source = WpfDecoder.FromStream(stream);
            break;
          case FileType.Unknown:
            if (stream.Length < 4) {
              return null;
            }

            var header = new byte[4];
            stream.Read(header, 0, 4);
            if (FlifDecoder.MagicDetect(header) == DetectResult.Yes) {
              data = new byte[stream.Length];
              stream.Seek(0, SeekOrigin.Begin);
              stream.Read(data, 0, data.Length);
              image.source = FlifDecoder.FromBytes(data);
            }

            break;
          default:
            return null;
        }

        return image.source != null ? image : null;
      }
      catch (Exception e) {
        return null;
      }
      finally {
        stream.Dispose();
      }
    }

    public BitmapSource GetPartial(IResizer resizer, Int32Rect pos, double scale) {
      var src = source.GetBitmap(pos);
      var result = Misc.AllocWriteableBitmap(
        (int)(pos.Width * scale), (int)(pos.Height * scale), src.Depth, src.Channel);
      result.Lock();

      var dst = Misc.BitmapOfWritableBitmap(result);
      resizer.Resize(src, dst);

      result.AddDirtyRect(new Int32Rect(0, 0, result.PixelWidth, result.PixelHeight));
      result.Unlock();

      result.Freeze();
      return result;
    }

    public BitmapSource GetFull(IResizer resizer, double scale = 1) {
      // need scale
      if (Math.Abs(scale - 1) > 0.0001) {
        return GetPartial(resizer, new Int32Rect(0, 0, Width, Height), scale);
      }

      // no need scale
      var dst = Misc.AllocWriteableBitmap(source.Width, source.Height, source.Depth, source.Channel);
      Misc.CopyToWritableBitmap(dst, source.FullBitmap);
      dst.Freeze();
      return dst;
    }
  }
}