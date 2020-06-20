using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  public class GetOld :IFilter {
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

        var t0 = Convert.ToInt32(0.272 * thisPixR + 0.534 * thisPixG + 0.131 * thisPixB);
        var t1 = Convert.ToInt32(0.349 * thisPixR + 0.686 * thisPixG + 0.168 * thisPixB);
        var t2 = Convert.ToInt32(0.393 * thisPixR + 0.769 * thisPixG + 0.189 * thisPixB);
        pixInts[i, 0] = t0 > 255 ? 255 : t0;
        pixInts[i, 1] = t1 > 255 ? 255 : t1;
        pixInts[i, 2] = t2 > 255 ? 255 : t2;
      }
      //Console.WriteLine(pixInts[0, 3]);
      byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
      FilterMISC.writeBitmap(dst, pixBytes2);

    }
  }
}
