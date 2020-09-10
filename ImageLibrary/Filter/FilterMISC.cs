using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace ImageLibrary.Filter {
    class FilterMISC
    {
        //判断图片格式
        static int getType(Bitmap src)
        {
            int label = 0;
            if (src.Depth == 8 && src.Channel == 3) label = 0;
            else if (src.Depth == 16 && src.Channel == 3) label = 1;
            else if (src.Depth == 8 && src.Channel == 4) label = 2;
            else if (src.Depth == 16 && src.Channel == 4) label = 3;
            return label;
        }
        //读取图片信息到byte数组中，第一维度是像素编号，第二维度是该像素的各个通道值。
        public static byte[,] readBitmap(Bitmap src)
        {
            var label = getType(src);

            var pixNum = src.Width * src.Height;
            // src.Depth == 8 ? (src.Channel == 3 ? 3 : 4) : (src.Channel == 3 ? 6 : 8);
            var channel_bytesNum = 3;
            var channel = src.Channel;
            switch (label)
            {
                case 0: { channel_bytesNum = 3; break; }
                case 1: { channel_bytesNum = 6;break; }
                case 2: { channel_bytesNum = 3;break; }
                case 3: { channel_bytesNum = 6;break; }
            }
            var pixList = new byte[pixNum,channel_bytesNum];
            int plus = 0;
            for(int m = 0; m < pixNum; m++)
            {
                for(int k = 0; k < channel_bytesNum; k++)
                {
                    pixList[m,k] = Marshal.ReadByte(src.Scan0, plus);
                    plus++;
                    if (plus % 4 == 3&&channel==4&&channel_bytesNum==3) plus++;
                    if (channel_bytesNum == 6 && channel == 4 && plus % 8 == 6) plus += 2;
                }
            }
            return pixList;


        }
        
        public static byte[] readBitmap2(Bitmap src)
        {

            var times = src.Width * src.Height;

            var pixList = new byte[times*src.Channel*src.Depth/8];

            for (int m = 0; m < times * src.Channel * src.Depth / 8; m++)
            {
                
                    pixList[m] = Marshal.ReadByte(src.Scan0, m);
                
            }
            return pixList;


        }

        public static void writeBitmap(Bitmap src, byte[,] vs)
        {
            var label = getType(src);

            var pixNum = src.Width * src.Height;
            // src.Depth == 8 ? (src.Channel == 3 ? 3 : 4) : (src.Channel == 3 ? 6 : 8);
            var channel_bytesNum = 3;
            switch (label)
            {
                case 0: { channel_bytesNum = 3; break; }
                case 1: { channel_bytesNum = 6; break; }
                case 2: { channel_bytesNum = 3; break; }
                case 3: { channel_bytesNum = 6; break; }
            }
            
            /*
            if (depth == 8)
            {                               3                         4
                format = channel == 3 ? PixelFormats.Bgr24 : PixelFormats.Bgra32;
            }
            else
            {                                 6                      8
                format = channel == 3 ? PixelFormats.Rgb48 : PixelFormats.Rgba64;
            }*/
            // src.Depth == 8 ? (src.Channel == 3 ? 3 : 4) : (src.Channel == 3 ? 6 : 8);
            int plus = 0;
            for(int m = 0; m < pixNum; m++)
            {
                for(int k = 0; k < channel_bytesNum; k++)
                {
                    Marshal.WriteByte(src.Scan0, plus, vs[m,k]);
                    plus++;
                }
            }
        }

        public static int[,] bytesToInt(byte[,] src,Bitmap bp)
        {
            var label = getType(bp);

            var pixNum = bp.Width * bp.Height;
            // src.Depth == 8 ? (src.Channel == 3 ? 3 : 4) : (src.Channel == 3 ? 6 : 8);
            var channel_bytesNum = 3;
            switch (label)
            {
                case 0: { channel_bytesNum = 3; break; }
                case 1: { channel_bytesNum = 6; break; }
                case 2: { channel_bytesNum = 3; break; }
                case 3: { channel_bytesNum = 6; break; }
            }

            var channel = 3;
            int[,] values = new int[pixNum, channel];

            if (label==0||label==2)
            {
                for (int i = 0; i < pixNum; i++)
                {
                    
                    for (int k = 0; k < channel_bytesNum; k++)
                    {
                        int value = (int)((src[i, k] & 0xFF)
                                | ((0x00) << 8)
                                | ((0x00) << 16)
                                | ((0x00) << 24));
                        values[i, k] = value;
                    }
                }
            }
            else 
            {
                for (int i = 0; i < pixNum; i++)
                {
                    //int[,] values = new int[pixNum, depth];
                    for (int k = 0; k < channel_bytesNum; k=k+2)
                    {
                        int value = (int)((src[i, k] & 0xFF)
                                | ((src[i,k+1]) << 8)
                                | ((0x00) << 16)
                                | ((0x00) << 24));
                        values[i, k/2] = value;
                    }
                }
            }
            //System.out.println(Arrays.toString(arrayInt));
            return values;
        }

        /*
        public static int[] bytesToInt(byte[] src)
        {
            int[] values = new int[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                int value = (int)((src[i] & 0xFF)
                        | ((0x00) << 8)
                        | ((0x00) << 16)
                        | ((0x00) << 24));
                values[i] = value;


            }
            return values;
        }*/

        public static byte[,] intTobytes(int[,] src,Bitmap bp)
        {
            var label = getType(bp);

            var pixNum = bp.Width * bp.Height;
            // src.Depth == 8 ? (src.Channel == 3 ? 3 : 4) : (src.Channel == 3 ? 6 : 8);
            var channel_bytesNum = 3;
            switch (label)
            {
                case 0: { channel_bytesNum = 3; break; }
                case 1: { channel_bytesNum = 6; break; }
                case 2: { channel_bytesNum = 3; break; }
                case 3: { channel_bytesNum = 6; break; }
            }
            
            // bp.Depth == 8 ? (bp.Channel == 3 ? 3 : 4) : (bp.Channel == 3 ? 6 : 8);
            byte[,] values = new byte[pixNum, channel_bytesNum];

            for(int i = 0; i < pixNum; i++)
            {
                if (channel_bytesNum == 3)
                {
                    for (int k = 0; k < channel_bytesNum; k = k + 1)
                    {
                        values[i, k] = (byte)(src[i, k] & 0xFF);

                        //values[i, k + 1] = (byte)((src[i,k] >> 8) & 0xFF);
                    }
                }
                else if (channel_bytesNum == 6)
                {
                    for(int k = 0; k < channel_bytesNum; k = k + 2)
                    {
                        values[i,k]= (byte)(src[i, k] & 0xFF);
                        values[i, k + 1] = (byte)((src[i, k] >> 8) & 0xFF);
                    }
                }
            }
            return values;
        }



        /*FileStream fs = new FileStream("t.txt", FileMode.Create);
            fs.Write(memory, 0, memory.Length);
            fs.Flush();
            fs.Close();*/

            /*
        public static Bitmap toOurBitmap(System.Drawing.Bitmap src)
        {
            IntPtr scan0;
            int stride=-1, width=-1, height=-1, depth=-1, channel=-1;
            width = src.Width;height = src.Height;
            MemoryStream memoryStream = new MemoryStream();
            //我猜这里是bmp，但是imageFormat里同时含有jpg,png等多种格式，应该不要紧吧？
            src.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            //两边规定的format应该是通用的，参考Misc的写法
            if (src.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb) { depth = 8;channel = 3; }
            else if (src.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb) { depth = 8;channel = 4; }
            else if (src.PixelFormat == System.Drawing.Imaging.PixelFormat.Format48bppRgb) { depth = 16;channel = 3; }
            else if (src.PixelFormat == System.Drawing.Imaging.PixelFormat.Format64bppArgb) { depth = 16;channel = 4; }

            if (stride == -1 ||  width == -1 | height == -1 || depth == -1 || channel == -1) return new Bitmap(IntPtr.Zero, 0, 0, 0, 0,0);

            //流保存到数组中
            var times = width * height * depth * channel / 8;
            var memory = new byte[times];
            memoryStream.Read(memory, 0, times);
            memoryStream.Seek(0, SeekOrigin.Begin);

            //之后写成staitc的。
            GCHandle memoryHandle = GCHandle.Alloc(memory, GCHandleType.Pinned);

            scan0 = memoryHandle.AddrOfPinnedObject();

            return new Bitmap(scan0, stride, width, height, depth, channel);
        }*/
        

    }
}
