using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  class Cartoon:IFilter {
    //卡通
    public void Filter(Bitmap src, Bitmap dst,object _)//, Bitmap dst, object options = null)
    {


      var pixNum = src.Width * src.Height;
      byte[,] pixBytes = FilterMISC.readBitmap(src);
      int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

      for (int i = 0; i < pixNum; i++) {
        if (i == pixNum - 1) break;

        var thisPixB = pixInts[i, 0];
        var thisPixG = pixInts[i, 1];
        var thisPixR = pixInts[i, 2];

        var B = Math.Abs(thisPixB - thisPixG + thisPixB + thisPixR) * thisPixG / 256;
        var G = Math.Abs(thisPixB - thisPixG + thisPixB + thisPixR) * thisPixR / 256;
        var R = Math.Abs(thisPixG - thisPixB + thisPixG + thisPixR) * thisPixR / 256;
        pixInts[i, 0] = B > 255 ? 255 : B;
        pixInts[i, 1] = G > 255 ? 255 : G;
        pixInts[i, 2] = R > 255 ? 255 : R;

      }
      //Console.WriteLine(pixInts[0, 3]);
      byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
      FilterMISC.writeBitmap(dst, pixBytes2);

    }
  }
}
