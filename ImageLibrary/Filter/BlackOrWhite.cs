using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  public class BlackOrWhite :IFilter {
    public void Filter(Bitmap src, Bitmap dst,object _)//, Bitmap dst, object options = null)
        {

      //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
      var pixNum = src.Width * src.Height;
      byte[,] pixBytes = FilterMISC.readBitmap(src);
      int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);
      /*
      string thisstring = string.Empty;
      for(int i = 0; i < 2000; i++)
      {
          for(int k = 0; k < src.Channel; k++)
          {
              thisstring += pixInts[i, k].ToString()+",";
          }
          thisstring += "\n";
      }
      File.WriteAllText("pixints.txt",thisstring);
      */



      for (int i = 0; i < pixNum; i++) {

        var thisPixB = pixInts[i, 0];
        var thisPixG = pixInts[i, 1];
        var thisPixR = pixInts[i, 2];

        var avg = (thisPixB + thisPixG + thisPixR) / 3;
        if (avg >= 100) {
          pixInts[i, 0] = 255;
          pixInts[i, 1] = 255;
          pixInts[i, 2] = 255;

        }
        else {
          pixInts[i, 0] = 0;
          pixInts[i, 1] = 0;
          pixInts[i, 2] = 0;

        }
      }
      //Console.WriteLine(pixInts[0, 3]);
      byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
      FilterMISC.writeBitmap(dst, pixBytes2);

    }
  }
}
