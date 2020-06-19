using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageLibrary.Decoder.Format.Avif;
using ImageLibrary.Decoder.Format.Bmp;
using ImageLibrary.Decoder.Format.Heif;
using ImageLibrary.Decoder.Format.Webp;
using ImageLibrary.Resizer;
using MetadataExtractor;
using MetadataExtractor.Util;
using Directory = MetadataExtractor.Directory;

namespace ImageLibrary {
  public class Image {
    // TODO: use more concrete data container
    public object Metadata { get; private set; }
    private IBitmapSource source;

    public int Width => source.Width;
    public int Height => source.Height;

    private Image() {}

    public static Image FromFile(string path) {
      var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
      var image = new Image();

      try {
        var format = FileTypeDetector.DetectFileType(stream);
        stream.Seek(0, SeekOrigin.Begin);
        image.Metadata = ImageMetadataReader.ReadMetadata(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var data = new byte[stream.Length];
        stream.Read(data, 0, data.Length);

        switch (format) {
          case FileType.Bmp:
            image.source = BmpDecoder.FromBytes(data);
            break;
          case FileType.WebP:
            image.source = WebpDecoder.FromBytes(data);
            break;
          case FileType.QuickTime:
            var qtFt = ((IReadOnlyList<Directory>) image.Metadata)
              .First(dir => dir.Name == "QuickTime File Type")
              .Tags
              .First(tag => tag.Name == "QuickTime File Type")
              .Description;
            if (qtFt != "avif" && qtFt != "avis" && qtFt != "av01") {
              return null;
            }
            image.source = AvifDecoder.FromBytes(data);
            break;
          case FileType.Heif:
            image.source = HeifDecoder.FromBytes(data);
            break;
          default:
            return null;
        }

        return image.source != null ? image : null;
      }
      finally {
        stream.Dispose();
      }
    }

    public BitmapSource GetPartial(Int32Rect pos, double scale) {
      var src = source.GetBitmap(pos);
      var result = Misc.AllocWriteableBitmap(
        (int)(pos.Width * scale), (int)(pos.Height * scale), src.Depth, src.Channel);
      result.Lock();

      var dst = Misc.BitmapOfWritableBitmap(result);
      Nnedi3Resizer.Resizer.Resize(src, dst);

      result.AddDirtyRect(new Int32Rect(0, 0, result.PixelWidth, result.PixelHeight));
      result.Unlock();

      result.Freeze();
      return result;
    }

    public BitmapSource GetFull(double scale = 1) {
      // need scale
      if (Math.Abs(scale - 1) > 0.0001) {
        return GetPartial(new Int32Rect(0, 0, Width, Height), scale);
      }

      // no need scale
      var dst = Misc.AllocWriteableBitmap(source.Width, source.Height, source.Depth, source.Channel);
      Misc.CopyToWritableBitmap(dst, source.FullBitmap);
      dst.Freeze();
      return dst;

    }
  }
}
