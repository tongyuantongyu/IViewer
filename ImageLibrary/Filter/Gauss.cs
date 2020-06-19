using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  class Gauss : IFilter {
    static int handleEdge(int i, int x, int w) {
      var m = x + 1;
      if (m < 0) m = -m;
      else if (m >= w) m = w + i - x;
      return m;
    }
    public void Filter(Bitmap src, Bitmap dst, object _)//, Bitmap dst, object options = null)
    {
      byte[,] pixBytes = FilterMISC.readBitmap(src);
      int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);
      var height = src.Height;
      var width = src.Width;
      var radius = 5;
      var sigma = radius / 3.0;
      var gaussEdge = radius * 2 + 1;

      List<double> gaussMatrix = new List<double>();
      var gaussSum = 0.0;
      var a = 1 / (2 * sigma * sigma * Math.PI);
      var b = -a * Math.PI;

      for (var i = -radius; i <= radius; i++) {
        for (var j = -radius; j <= radius; j++) {
          var gxy = a * Math.Exp((i * i + j * j) * b);
          gaussMatrix.Add(gxy);
          gaussSum += gxy;
        }
      }

      var gaussNum = (radius + 1) * (radius + 1);
      for (var i = 0; i < gaussNum; i++) gaussMatrix[i] = gaussMatrix[i] / gaussSum;

      for (var x = radius + 1; x < width / 2; x++) {
        for (var y = radius + 1; y < height - radius; y++) {
          var R = 0; var G = 0; var B = 0;
          for (var i = -radius; i < radius; i++) {
            var m = handleEdge(i, x, width);
            for (var j = -radius; j <= radius; j++) {
              var mm = handleEdge(j, y, height);
              var currentPixId = mm * width + m;
              var jj = j + radius;
              var ii = i + radius;
              R += Convert.ToInt32(pixInts[currentPixId, 2] * gaussMatrix[jj * gaussEdge + ii]);
              G += Convert.ToInt32(pixInts[currentPixId, 1] * gaussMatrix[jj * gaussEdge + ii]);
              B += Convert.ToInt32(pixInts[currentPixId, 0] * gaussMatrix[jj * gaussEdge + ii]);
            }
          }
          var pixId = (y * width + x);
          pixInts[pixId, 2] = Convert.ToInt32(R * 1.5) > 255 ? 255 : Convert.ToInt32(R * 1.5);
          pixInts[pixId, 1] = Convert.ToInt32(G * 1.5) > 255 ? 255 : Convert.ToInt32(G * 1.5);
          pixInts[pixId, 0] = Convert.ToInt32(B * 1.5) > 255 ? 255 : Convert.ToInt32(B * 1.5);


        }
      }
    }
  }
  byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
  FilterMISC.writeBitmap(dst, pixBytes2);
}
