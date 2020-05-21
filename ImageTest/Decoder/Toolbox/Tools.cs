using System;
using System.Runtime.InteropServices;

namespace ImageDecoder.Toolbox {
  public static class Tools {
    [DllImport("BitOp.dll", EntryPoint = "BGRA2RGBA", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BGRA2RGBA(IntPtr begin, [In] UIntPtr length);

    [DllImport("BitOp.dll", EntryPoint = "BBGGRR2RRGGBB", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BBGGRR2RRGGBB(IntPtr begin, [In] UIntPtr length);

    [DllImport("BitOp.dll", EntryPoint = "BBGGRRAA2RRGGBBAA", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BBGGRRAA2RRGGBBAA(IntPtr begin, [In] UIntPtr length);

    [DllImport("BitOp.dll", EntryPoint = "DEPTHCONVERT", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DEPTHCONVERT(IntPtr begin, [In] UIntPtr length, [In] UIntPtr bitDepth);

    [DllImport("BitOp.dll", EntryPoint = "GDICOLORFIX", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GDICOLORFIX(IntPtr begin, [In] UIntPtr length, [In] double gamma);
  }
}
