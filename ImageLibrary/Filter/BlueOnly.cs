using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  class BlueOnly:IFilter {
    public void Filter(Bitmap src, Bitmap dst,object _)//, Bitmap dst, object options = null)
       {

      //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
      var pixNum = src.Width * src.Height;
      byte[,] pixBytes = FilterMISC.readBitmap(src);
      int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

      for (int i = 0; i < pixNum; i++) {

        var thisPixB = pixInts[i, 0];
        var thisPixG = pixInts[i, 1];
        var thisPixR = pixInts[i, 2];
        pixInts[i, 2] = pixInts[i, 1] = 0;
      }
      //Console.WriteLine(pixInts[0, 3]);
      byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
      FilterMISC.writeBitmap(dst, pixBytes2);

    }
  }
}
