using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Windows.Media.Media3D;

namespace ImageLibrary.Filter {
   public class Rev:IFilter
    {
        //底片
        public void Filter(Bitmap src,Bitmap dst,object _)//, Bitmap dst, object options = null)
        {
            
            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes,src);
            for(int i = 0; i < pixNum; i++)
            {
                for(int k = 0; k < 3; k++)
                {
                    pixInts[i, k] = 255 - pixInts[i, k];
                }
            }
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts,src);
            FilterMISC.writeBitmap(dst, pixBytes2);

            /*
            byte[] byteTest = FilterMISC.readBitmap2(src);
            string insting = string.Join(",", byteTest);

            FileStream fs = new FileStream("output.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.Write(insting);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();*/
        }
    /*
        //灰度滤镜
        public static void grayColor(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes,src);
            for (int i = 0; i < pixNum; i++)
            {
                
                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                //var thisPixA = pixInts[i, 3];
                var avg = (thisPixB*0.11 + thisPixG*0.59 + thisPixR*0.3) ;
                pixInts[i, 0] =Convert.ToInt32(avg)>255?255: Convert.ToInt32(avg);
                pixInts[i, 1] = Convert.ToInt32(avg) > 255 ? 255 : Convert.ToInt32(avg);
                pixInts[i, 2] = Convert.ToInt32(avg) > 255 ? 255 : Convert.ToInt32(avg);
                //pixInts[i, 3] = avg;
            }
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }
        //黑白
        public static void blackOrWhite(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes,src);
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
            



            for (int i = 0; i < pixNum; i++)
            {

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                
                var avg = (thisPixB + thisPixG + thisPixR) / 3;
                if (avg >= 100)
                {
                    pixInts[i, 0] = 255;
                    pixInts[i, 1] = 255;
                    pixInts[i, 2] = 255;
                    
                }
                else
                {
                    pixInts[i, 0] = 0;
                    pixInts[i, 1] = 0;
                    pixInts[i, 2] = 0;
                    
                }
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }
        //去色
        public static void deleteColor(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);
         
            for (int i = 0; i < pixNum; i++)
            {

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                pixInts[i, 0] = pixInts[i, 1] = pixInts[i, 2] = (Math.Max(Math.Max(thisPixB, thisPixG), thisPixR)+
                    Math.Min(Math.Min(thisPixB, thisPixG), thisPixR))/2;
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }
        //单色滤镜
        public static void redOnly(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

            for (int i = 0; i < pixNum; i++)
            {

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                pixInts[i, 0] = pixInts[i, 1] = 0; 
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }
        public static void blueOnly(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

            for (int i = 0; i < pixNum; i++)
            {

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                pixInts[i, 2] = pixInts[i, 1] = 0;
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }
        public static void greenOnly(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

            for (int i = 0; i < pixNum; i++)
            {

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                pixInts[i, 0] = pixInts[i, 2] = 0;
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }
        //高斯模糊
        static int handleEdge(int i,int x,int w)
        {
            var m = x + 1;
            if (m < 0) m = -m;
            else if (m >= w) m = w + i - x;
            return m;
        }
        public static void gauss(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
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

            for(var i = -radius; i <= radius; i++)
            {
                for(var j = -radius; j <= radius; j++)
                {
                    var gxy = a * Math.Exp((i * i + j * j) * b);
                    gaussMatrix.Add(gxy);
                    gaussSum += gxy;
                }
            }

            var gaussNum = (radius + 1) * (radius + 1);
            for (var i = 0; i < gaussNum; i++) gaussMatrix[i] =gaussMatrix[i] /gaussSum;

            for(var x =radius+1; x < width/2; x++)
            {
                for(var y = radius+1; y < height-radius; y++)
                {
                    var R = 0;var G = 0;var B = 0;
                    for(var i = -radius; i < radius; i++)
                    {
                        var m =handleEdge(i, x, width);
                        for(var j = -radius; j <= radius; j++)
                        {
                            var mm = handleEdge(j, y, height);
                            var currentPixId = mm * width +m ;
                            var jj = j + radius;
                            var ii = i + radius;
                            R += Convert.ToInt32(pixInts[currentPixId, 2] * gaussMatrix[jj * gaussEdge + ii]);
                            G+= Convert.ToInt32(pixInts[currentPixId, 1] * gaussMatrix[jj * gaussEdge + ii]);
                            B+= Convert.ToInt32(pixInts[currentPixId, 0] * gaussMatrix[jj * gaussEdge + ii]);
                        }
                    }
                    var pixId = (y * width + x) ;
                    pixInts[pixId,2] =Convert.ToInt32( R*1.5)>255?255: Convert.ToInt32(R * 1.5);
                    pixInts[pixId, 1] =Convert.ToInt32( G*1.5)>255?255: Convert.ToInt32(G * 1.5);
                    pixInts[pixId, 0] =Convert.ToInt32( B*1.5)>255?255: Convert.ToInt32(B * 1.5);


                }
            }

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }

        //怀旧风格
        public static void getOld(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

            for (int i = 0; i < pixNum; i++)
            {

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                
                var t0=Convert.ToInt32(0.272 * thisPixR + 0.534 * thisPixG + 0.131 * thisPixB);
                var t1= Convert.ToInt32(0.349 * thisPixR + 0.686 * thisPixG + 0.168 * thisPixB);
                var t2= Convert.ToInt32(0.393 * thisPixR + 0.769 * thisPixG + 0.189 * thisPixB);
                pixInts[i, 0] = t0 > 255 ? 255 : t0;
                pixInts[i, 1] = t1 > 255 ? 255 : t1;
                pixInts[i, 2] = t2 > 255 ? 255 : t2;
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }

        //油画风,仅对真实场景。
        public static void colorFul(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

            for (int i = 0; i < pixNum; i++)
            {

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                var t0 =Convert.ToInt32( thisPixB*256/(thisPixG+thisPixR+1));
                var t1 =Convert.ToInt32( thisPixG*256/(thisPixB+thisPixR+1));
                var t2 =Convert.ToInt32( thisPixR*256/(thisPixG+thisPixB+1));

                pixInts[i, 0] = t0 > 255 ? 255 : (t0 < 0 ? 0 : t0);
                pixInts[i, 1] = t1 > 255 ? 255 : (t1 < 0 ? 0 : t1);
                pixInts[i, 2] = t2 > 255 ? 255 : (t2 < 0 ? 0 : t2);
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }

        //浮雕
        public static void relief(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

            for (int i = 0; i < pixNum; i++)
            {
                if (i == pixNum - 1) break;

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                var nextPixB = pixInts[i + 1, 0];
                var nextPixG = pixInts[i + 1, 1];
                var nextPixR = pixInts[i + 1, 2];
                pixInts[i, 0] = thisPixB - nextPixB + 128;
                pixInts[i, 1] = thisPixG - nextPixG + 128;
                pixInts[i, 2] = thisPixR - nextPixR + 128;
                    //pixInts[i, 1] = pixInts[i, 2] = (Math.Max(Math.Max(thisPixB, thisPixG), thisPixR) +
                    //Math.Min(Math.Min(thisPixB, thisPixG), thisPixR)) / 2;
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }

        //卡通
        public static void cartoon(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

            for (int i = 0; i < pixNum; i++)
            {
                if (i == pixNum - 1) break;

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];

                var B= Math.Abs(thisPixB - thisPixG + thisPixB + thisPixR) * thisPixG / 256;
                var G = Math.Abs(thisPixB - thisPixG + thisPixB + thisPixR) * thisPixR / 256;
                var R= Math.Abs(thisPixG - thisPixB + thisPixG + thisPixR) * thisPixR / 256;
                pixInts[i, 0] = B>255?255:B;
                pixInts[i, 1] =G>255?255:G;
                pixInts[i, 2] = R>255?255:R;

            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }
        //素描
        public static void sketch(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {


            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);
            int height = src.Height;int width = src.Width;

            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < height; k++)
                {
                    if (i == width - 1 || k == height - 1) continue;

                    var thisPixB = pixInts[k*width+i, 0];
                    var thisPixG = pixInts[k * width + i, 1];
                    var thisPixR = pixInts[k * width + i, 2];
                    
                    var avgthis = (thisPixB * 0.11 + thisPixG * 0.59 + thisPixR * 0.3);
                    var nextB = pixInts[(k + 1) * width + i + 1, 0];
                    var nextG = pixInts[(k + 1) * width + i + 1, 1];
                    var nextR = pixInts[(k + 1) * width + i + 1, 2];
                    var avgnext= (nextB * 0.11 + nextG * 0.59 + nextR * 0.3);

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

        //三色交换
        public static void exchange(Bitmap src, Bitmap dst)//, Bitmap dst, object options = null)
        {

            //var times = src.Width * src.Height * src.Channel * src.Depth / 8;
            var pixNum = src.Width * src.Height;
            byte[,] pixBytes = FilterMISC.readBitmap(src);
            int[,] pixInts = FilterMISC.bytesToInt(pixBytes, src);

            for (int i = 0; i < pixNum; i++)
            {

                var thisPixB = pixInts[i, 0];
                var thisPixG = pixInts[i, 1];
                var thisPixR = pixInts[i, 2];
                var t0 = thisPixG;
                var t1 = thisPixR;
                var t2 = thisPixB;

                pixInts[i, 0] = t0 > 255 ? 255 : (t0 < 0 ? 0 : t0);
                pixInts[i, 1] = t1 > 255 ? 255 : (t1 < 0 ? 0 : t1);
                pixInts[i, 2] = t2 > 255 ? 255 : (t2 < 0 ? 0 : t2);
            }
            //Console.WriteLine(pixInts[0, 3]);
            byte[,] pixBytes2 = FilterMISC.intTobytes(pixInts, src);
            FilterMISC.writeBitmap(dst, pixBytes2);

        }*/
    }
}
