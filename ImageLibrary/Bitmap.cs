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
    public readonly IntPtr Scan0;
    public readonly int Stride;
    public readonly int Width;
    public readonly int Height;
    public readonly int Depth;
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