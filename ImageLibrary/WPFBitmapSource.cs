using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageLibrary {
  class WPFBitmapSource : IBitmapSource {
    private readonly WriteableBitmap wb;

    public Bitmap GetBitmap(Int32Rect pos) {
      if (pos.X < 0 || pos.Y < 0 || pos.Width < 0 || pos.Height < 0) {
        throw new ArgumentException($"Bad area: {pos}");
      }

      if (pos.Width > Width || pos.Height > Height) {
        throw new ArgumentException($"Area overflow: Image: {Width}x{Height}, Area: x={pos.Width}, y={pos.Height}");
      }

      var scan0 = Scan0 + pos.Y * Stride + pos.X * wb.Format.BitsPerPixel;
      return new Bitmap(scan0, Stride, pos.Width, pos.Height, Depth, Channel);
    }

    public WPFBitmapSource(BitmapSource source) {
      if (source.CanFreeze) {
        source.Freeze();
      }

      if (source.Format == PixelFormats.Bgr24 || source.Format == PixelFormats.Bgra32) {
        wb = new WriteableBitmap(source);
        return;
      }

      var hasAlpha = source.Format == PixelFormats.Pbgra32 ||
                     source.Format == PixelFormats.Prgba64 ||
                     source.Format == PixelFormats.Rgba64 ||
                     source.Format == PixelFormats.Prgba128Float ||
                     source.Format == PixelFormats.Rgba128Float;

      var converted = new FormatConvertedBitmap();
      converted.BeginInit();
      converted.Source = source;
      converted.DestinationFormat = hasAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr24;
      converted.EndInit();

      wb = new WriteableBitmap(converted);
      FullBitmap = new Bitmap(Scan0, Stride, Width, Height, Depth, Channel);
    }

    public Bitmap FullBitmap { get; }
    public IntPtr Scan0 => wb.BackBuffer;
    public int Stride => wb.BackBufferStride;

    public int Width => wb.PixelWidth;
    public int Height => wb.PixelHeight;
    public int Depth => 8;
    public int Channel => wb.Format == PixelFormats.Bgr24 ? 3 : 4;
  }
}
