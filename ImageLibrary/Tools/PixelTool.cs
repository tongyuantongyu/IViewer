using System;
using System.Runtime.InteropServices;

namespace ImageLibrary.Tools {
  public static class PixelTool {
    [DllImport("PixelOp.dll", EntryPoint = "BGRA2RGBA", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BGRA2RGBA(IntPtr begin, [In] UIntPtr length);

    [DllImport("PixelOp.dll", EntryPoint = "BBGGRR2RRGGBB", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BBGGRR2RRGGBB(IntPtr begin, [In] UIntPtr length);

    [DllImport("PixelOp.dll", EntryPoint = "BBGGRRAA2RRGGBBAA", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BBGGRRAA2RRGGBBAA(IntPtr begin, [In] UIntPtr length);

    [DllImport("PixelOp.dll", EntryPoint = "DEPTHCONVERT", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DEPTHCONVERT(IntPtr begin, [In] UIntPtr length, [In] UIntPtr bitDepth);
  }
}
