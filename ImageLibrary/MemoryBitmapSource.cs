using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ImageLibrary {
  class MemoryBitmapSource : IDisposable, IBitmapSource {

    public IntPtr Scan0 { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Depth { get; private set; }
    public int Channel { get; private set; }

    private int PixelSize => (Depth / 8) * Channel;
    public int Stride => Width * PixelSize;

    public Bitmap FullBitmap { get; }

    private GCHandle memoryHandle;

    public MemoryBitmapSource(int width, int height, int depth, int channel) {
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

    public Bitmap GetBitmap(Int32Rect pos) {
      if (pos.X < 0 || pos.Y < 0 || pos.Width < 0 || pos.Height < 0) {
        throw new ArgumentException($"Bad area: {pos}");
      }

      if (pos.Width > Width || pos.Height > Height) {
        throw new ArgumentException($"Area overflow: Image: {Width}x{Height}, Area: x={pos.Width}, y={pos.Height}");
      }

      var scan0 = Scan0 + pos.Y * Stride + pos.X * PixelSize;
      return new Bitmap(scan0, Stride, pos.Width, pos.Height, Depth, Channel);
    }
  }
}
