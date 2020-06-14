using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// ReSharper disable MemberCanBePrivate.Global

namespace ImageLibrary {
  /// <summary>
  /// Bitmap represents a block of memory storing actual pixel data, as well as metadata which specifes
  /// memory layout of the pixels.
  /// </summary>
  // Bitmap doesn't hold ownership of actual memory.
  // Bitmap provides universal interface to access pixel memory holding by different class.
  // Also, Bitmap can be passed to native code to do process.
  [StructLayout(LayoutKind.Sequential)]
  public struct Bitmap {
    // Scan0 represents the begin address of pixel data
    public readonly IntPtr Scan0;
    // Stride represents length per horizontal line in byte(uint8).
    // This value can be bigger than size of a valid line (width * channel * bepth / 8),
    // and in this case following data should not be modified
    public readonly int Stride;
    // valid area width
    public readonly int Width;
    // valid area height
    public readonly int Height;
    // 8 or 16. Also the bit width.
    // When value is 8, pixel order is bgr(a), every channel value is in range [0, 255] and type byte(uint8)
    // When value is 16, pixel order is rgb(a), every channel value is in range [0, 65535] and type ushort(uint16)
    public readonly int Depth;
    // 3 or 4. 4 means alpha present, 3 means absent.
    public readonly int Channel;

    public static Bitmap Invalid = new Bitmap(IntPtr.Zero, 0, 0, 0, 0, 0);
    public bool Valid => Scan0 != IntPtr.Zero;

    public Bitmap(IntPtr scan0, int stride, int width, int height, int depth, int channel) {
      Scan0 = scan0;
      Stride = stride;
      Width = width;
      Height = height;
      Depth = depth;
      Channel = channel;
    }
  }
}