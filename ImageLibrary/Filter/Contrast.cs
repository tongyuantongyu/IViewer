﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  class Contrast:IFilter {
    public void Filter(Bitmap src, Bitmap dst, object percentage)//, Bitmap dst, object options = null)
        {

      //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
      var pixNum = src.Width * src.Height;
      byte[,] pixBytes = FilterMISC.readBitmap(src);
      int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);
      for (int i = 0; i < pixNum; i++) {
        //y = (x - 127.5) * k + 127.5
        //k = tan((45 + 44 * c) / 180 * pi);
        var k = Math.Tan((45 + 44 *Convert.ToDouble( percentage)) / 180 * Math.PI);
        var thisPixB = pixInts[i, 0];
        var thisPixG = pixInts[i, 1];
        var thisPixR = pixInts[i, 2];
        var pixMax = Math.Max(thisPixR, Math.Max(thisPixB, thisPixG));
        var pixMin = Math.Min(thisPixR, Math.Min(thisPixB, thisPixG));
        var brightness = (pixMax + pixMax) / 2;
        var B = Convert.ToInt32((thisPixB -127.5)*k+127.5);
        var G = Convert.ToInt32((thisPixG - 127.5) * k + 127.5);
        var R = Convert.ToInt32((thisPixR - 127.5) * k + 127.5);
        //var thisPixA = pixInts[i, 3];
        var avg = (thisPixB * 0.11 + thisPixG * 0.59 + thisPixR * 0.3);
        pixInts[i, 0] = B > 255 ? 255 : (B < 0 ? 0 : B);
        pixInts[i, 1] = G > 255 ? 255 : (G < 0 ? 0 : G);
        pixInts[i, 2] = R > 255 ? 255 : (R < 0 ? 0 : R);
        //pixInts[i, 3] = avg;
      }
      byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
      FilterMISC.writeBitmap(dst, pixBytes2);

    }
  }
}
