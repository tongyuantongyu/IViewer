using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  class TestFilter {

    [StructLayout(LayoutKind.Sequential)]
    private struct Pixel {
      public byte R;
      public byte G;
      public byte B;
      public byte A;
    }

    public void Test() {
      IntPtr a = IntPtr.Zero;
      var pixel = Marshal.PtrToStructure<Pixel>(a);
      Marshal.StructureToPtr(pixel, a, false);
    }
  }
}
