using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ImageLibrary {
  class MemoryImageSource : IDisposable, IImageSource {

    public IntPtr Scan0 { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Depth { get; private set; }
    public int Channel { get; private set; }

    private int PixelSize => (Depth / 8) * Channel;
    public int Stride => Width * PixelSize;

    public Bitmap FullBitmap { get; }

    private GCHandle memoryHandle;

    public MemoryImageSource(int width, int height, int depth, int channel) {
      if (depth != 8 && depth != 16) {
        throw new ArgumentException($"Image depth can only be 8 or 16, got {depth}");
      }

      var len = depth / 8;

      if (channel != 3 && channel != 4) {
        throw new ArgumentException($"Image channel can only be 3 or 4, got {channel}");
      }

      var memory = new byte[width * height * len * depth * channel];
      memoryHandle = GCHandle.Alloc(memory, GCHandleType.Pinned);

      Scan0 = memoryHandle.AddrOfPinnedObject();
      Width = width;
      Height = height;
      Depth = depth;
      Channel = channel;

      FullBitmap = new Bitmap(Scan0, Stride, Width, Height, Depth, Channel);
    }

    public void Dispose() {
      Scan0 = IntPtr.Zero;
      memoryHandle.Free();
    }

    public Bitmap GetBitmap(int xmin, int xmax, int ymin, int ymax) {
      if (xmin < 0 || ymin < 0 || xmax < xmin || ymax < ymin) {
        throw new ArgumentException($"Bad area: x[{xmin}:{xmax}],y[{ymin}:{ymax}]");
      }

      if (xmax > Width || ymax > Height) {
        throw new ArgumentException($"Area overflow: Image: {Width}x{Height}, Area: x={xmax}, y={ymax}");
      }

      var scan0 = Scan0 + ymin * Stride + xmin * PixelSize;
      return new Bitmap(scan0, Stride, xmax - xmin, ymax - ymin, Depth, Channel);
    }
  }
}
