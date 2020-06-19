using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ImageLibrary.Resizer {
  public enum VnImageKernelType {
    VnImageKernelNearest = -2147483648,
    VnImageKernelAverage = -2147483647,
    VnImageKernelBilinear = -2147483646,
    VnImageKernelBicubic = -2147483644,
    VnImageKernelCatmull = -2147483643,
    VnImageKernelMitchell = -2147483642,
    VnImageKernelCardinal = -2147483641,
    VnImageKernelSpline = 8,
    VnImageKernelBspline = -2147483639,
    VnImageKernelLanczos = 10,
    VnImageKernelLanczos2 = 11,
    VnImageKernelLanczos3 = 12,
    VnImageKernelLanczos4 = 13,
    VnImageKernelLanczos5 = 14,
    VnImageKernelBokeh = 15,
    VnImageKernelGaussian = -2147483632,
    VnImageKernelCoverage = -2147483631
  }

  internal class InterpolateResizer : IResizer {
    public static IResizer Resizer => new InterpolateResizer();
    public void Resize(Bitmap src, Bitmap dst, object options = null) {
      Debug.WriteLine("Call resize start.");
      var kernelType = VnImageKernelType.VnImageKernelLanczos;
      if (options != null) {
        if (options is VnImageKernelType k) {
          kernelType = k;
        }
        else {
          throw new ArgumentException("Bad option.");
        }
      }

      if (!ResizeImage(ref src, ref dst, kernelType)) {
        throw new ArgumentException("Resize failed.");
      }
      Debug.WriteLine("Call resize end.");
    }

    [DllImport("Imagine.dll", EntryPoint = "ResizeImage", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool ResizeImage(ref Bitmap src, ref Bitmap dst, VnImageKernelType kernel);
  }
}