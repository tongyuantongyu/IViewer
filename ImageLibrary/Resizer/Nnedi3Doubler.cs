using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Resizer {
  class Nnedi3Doubler : IDoubler {

    public static IDoubler Doubler => new Nnedi3Doubler();

    public void Double(Bitmap src, Bitmap dst, object options = null) {
      if (!DoubleImage(ref src, ref dst)) {
        throw new ArgumentException("Doubling failed.");
      }
    }

    [DllImport("libnnedi3.dll", EntryPoint = "DoubleImage", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool DoubleImage(ref Bitmap src, ref Bitmap dst);
  }
}
