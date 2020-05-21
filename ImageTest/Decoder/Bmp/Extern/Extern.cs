using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ImageDecoder.Bmp.Extern {
  [StructLayout(LayoutKind.Sequential)]
  public struct BMPExport {
    public int width;
    public int height;
    public IntPtr bmp; // obscure for extern use.
  }
  public static class EasyBmpNative {
    [DllImport("easybmp.dll", EntryPoint = "DecodeFromBuffer", CallingConvention = CallingConvention.Cdecl)]
    public static extern ref BMPExport DecodeFromBuffer([In] IntPtr buffer, UIntPtr length);

    [DllImport("easybmp.dll", EntryPoint = "WriteToMemory", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool WriteToMemory(ref BMPExport bmp, IntPtr destination, UIntPtr stride);

    [DllImport("easybmp.dll", EntryPoint = "FreeBMP", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FreeBMP(ref BMPExport bmp);
  }
}
