using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  public class Sketch :IFilter {
    //素描
    public void Filter(Bitmap src, Bitmap dst,object _)//, Bitmap dst, object options = null)
    {


      var pixNum = src.Width * src.Height;
      byte[,] pixBytes = FilterMISC.readBitmap(src);
      int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);
      int height = src.Height; int width = src.Width;

      for (int i = 0; i < width; i++) {
        for (int k = 0; k < height; k++) {
          if (i == width - 1 || k == height - 1) continue;

          var thisPixB = pixInts[k * width + i, 0];
          var thisPixG = pixInts[k * width + i, 1];
          var thisPixR = pixInts[k * width + i, 2];

          var avgthis = (thisPixB * 0.11 + thisPixG * 0.59 + thisPixR * 0.3);
          var nextB = pixInts[(k + 1) * width + i + 1, 0];
          var nextG = pixInts[(k + 1) * width + i + 1, 1];
          var nextR = pixInts[(k + 1) * width + i + 1, 2];
          var avgnext = (nextB * 0.11 + nextG * 0.59 + nextR * 0.3);

          var diff = Math.Abs(avgnext - avgthis);
          var gray = 0;
          if (diff >= 10) gray = 0;
          else gray = 255;
          pixInts[k * width + i, 0] = gray;
          pixInts[k * width + i, 1] = gray;
          pixInts[k * width + i, 2] = gray;
        }
      }

      byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
      FilterMISC.writeBitmap(dst, pixBytes2);

    }
  }
}
