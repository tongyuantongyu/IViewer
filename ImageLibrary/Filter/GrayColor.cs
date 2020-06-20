using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  public class GrayColor :IFilter {
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
        //var thisPixA = pixInts[i, 3];
        var avg = (thisPixB * 0.11 + thisPixG * 0.59 + thisPixR * 0.3);
        pixInts[i, 0] = Convert.ToInt32(avg) > 255 ? 255 : Convert.ToInt32(avg);
        pixInts[i, 1] = Convert.ToInt32(avg) > 255 ? 255 : Convert.ToInt32(avg);
        pixInts[i, 2] = Convert.ToInt32(avg) > 255 ? 255 : Convert.ToInt32(avg);
        //pixInts[i, 3] = avg;
      }
      byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
      FilterMISC.writeBitmap(dst, pixBytes2);

    }
  }
}
