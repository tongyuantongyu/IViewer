using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageDecoder.Heif.Extern;
using ImageDecoder.Toolbox;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable MemberCanBePrivate.Global

namespace ImageDecoder.Heif {
  using heif_item_id = UInt32;

  public static class HeifDecoder {
    private static bool CheckErrPtr(IntPtr errPtr) {
      var errStruct = Marshal.PtrToStructure<HeifError>(errPtr);
      var result = errStruct.code != 0;
      LibHeifNative.HeifFreeHeifError(errPtr);
      return result;
    }

    public static unsafe Bitmap BitmapFromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return BitmapFromPointer((IntPtr)dataptr, data.LongLength);
      }
    }

    public static unsafe Bitmap BitmapFromPointer(IntPtr data, long length) {
      var ctx = UIntPtr.Zero;
      var imageHandle = UIntPtr.Zero;
      var options = UIntPtr.Zero;
      var image = UIntPtr.Zero;
      Bitmap b = null;
      BitmapData bd = null;
      var success = false;

      try {
        ctx = LibHeifNative.HeifContextAlloc();

        var err = LibHeifNative.HeifContextReadFromMemoryWithoutCopy(ctx, data, (UIntPtr) length, UIntPtr.Zero);
        if (CheckErrPtr(err)) {
          throw new Exception("Failed.");
        }

        var imageCount = LibHeifNative.HeifContextGetNumberOfTopLevelImages(ctx);
        if (imageCount == 0) {
          throw new Exception("Failed.");
        }

        var imageIDs = new heif_item_id[imageCount];
        fixed (heif_item_id* ptr = imageIDs) {
          imageCount = LibHeifNative.HeifContextGetListOfTopLevelImageIDs(ctx, (UIntPtr) ptr, imageCount);
        }

        err = LibHeifNative.HeifContextGetImageHandle(ctx, imageIDs[0], ref imageHandle);
        if (CheckErrPtr(err)) {
          throw new Exception("Failed.");
        }

        var depth = LibHeifNative.HeifImageHandleGetLumaBitsPerPixel(imageHandle);
        if (depth < 0) {
          throw new Exception("Failed.");
        }

        options = LibHeifNative.HeifDecodingOptionsAlloc();

        var hasAlpha = LibHeifNative.HeifImageHandleHasAlphaChannel(imageHandle) != 0;

        HeifChroma chroma;
        PixelFormat format;
        if (depth <= 8) {
          chroma = HeifChroma.HeifChromaInterleavedRgba;
          format = PixelFormat.Format32bppArgb;
        }
        else if (hasAlpha) {
          chroma = HeifChroma.HeifChromaInterleavedRrggbbaaLe;
          format = PixelFormat.Format64bppArgb;
        }
        else {
          chroma = HeifChroma.HeifChromaInterleavedRrggbbLe;
          format = PixelFormat.Format48bppRgb;
        }
        err = LibHeifNative.HeifDecodeImage(imageHandle, ref image, HeifColorspace.HeifColorspaceRgb,
          chroma, options);

        if (CheckErrPtr(err) || image == UIntPtr.Zero) {
          throw new Exception("Failed.");
        }

        var width = LibHeifNative.HeifImageGetWidth(image, HeifChannel.HeifChannelInterleaved);
        var height = LibHeifNative.HeifImageGetHeight(image, HeifChannel.HeifChannelInterleaved);
        b = new Bitmap(width, height, format);
        bd = b.LockBits(new Rectangle(0, 0, width, height),
          ImageLockMode.ReadWrite, format);

        var strideSource = 0;
        var strideDest = bd.Stride;

        var raw = LibHeifNative.HeifImageGetPlaneReadonly(image, HeifChannel.HeifChannelInterleaved, ref strideSource);

        var ptrBegin = bd.Scan0;
        if (strideSource != strideDest) {
          for (var line = 0; line < height; line++) {
            var src = raw + line * strideSource;
            var dst = ptrBegin + line * strideDest;
            Buffer.MemoryCopy(src.ToPointer(), dst.ToPointer(), strideSource, strideDest);
          }
        }
        else {
          Buffer.MemoryCopy(raw.ToPointer(), ptrBegin.ToPointer(), strideSource * height, strideDest * height);
        }

        Console.WriteLine();

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (chroma) {
          case HeifChroma.HeifChromaInterleavedRgba:
            Tools.BGRA2RGBA(bd.Scan0, (UIntPtr) (width * height));
            break;
          case HeifChroma.HeifChromaInterleavedRrggbbaaLe:
            Tools.BBGGRRAA2RRGGBBAA(bd.Scan0, (UIntPtr) (width * height));
            Tools.DEPTHCONVERT(bd.Scan0, (UIntPtr) (width * height * 4), (UIntPtr) depth);
            Tools.GDICOLORFIX(bd.Scan0, (UIntPtr) (width * height * 4), 2.2);
            break;
          case HeifChroma.HeifChromaInterleavedRrggbbLe:
            Tools.BBGGRRAA2RRGGBBAA(bd.Scan0, (UIntPtr) (width * height));
            Tools.DEPTHCONVERT(bd.Scan0, (UIntPtr) (width * height * 3), (UIntPtr) depth);
            Tools.GDICOLORFIX(bd.Scan0, (UIntPtr) (width * height * 3), 2.2);
            break;
        }

        success = true;
      }
      finally {
        if (ctx != UIntPtr.Zero) {
          LibHeifNative.HeifContextFree(ctx);
        }

        if (imageHandle != UIntPtr.Zero) {
          LibHeifNative.HeifImageHandleRelease(imageHandle);
        }

        if (options != UIntPtr.Zero) {
          LibHeifNative.HeifDecodingOptionsFree(options);
        }

        if (image != UIntPtr.Zero) {
          LibHeifNative.HeifImageRelease(image);
        }

        if (bd != null) {
          b.UnlockBits(bd);
        }

        if (!success) {
          b?.Dispose();
        }
      }

      return b;
    }

    public static unsafe WriteableBitmap WBitmapFromBytes(byte[] data, double dpi) {
      fixed (byte* dataptr = data) {
        return WBitmapFromPointer((IntPtr)dataptr, data.LongLength, dpi);
      }
    }

    public static unsafe WriteableBitmap WBitmapFromPointer(IntPtr data, long length, double dpi) {
      var ctx = UIntPtr.Zero;
      var imageHandle = UIntPtr.Zero;
      var options = UIntPtr.Zero;
      var image = UIntPtr.Zero;
      WriteableBitmap b = null;

      try {
        ctx = LibHeifNative.HeifContextAlloc();

        var err = LibHeifNative.HeifContextReadFromMemoryWithoutCopy(ctx, data, (UIntPtr) length, UIntPtr.Zero);
        if (CheckErrPtr(err)) {
          throw new Exception("Failed.");
        }

        var imageCount = LibHeifNative.HeifContextGetNumberOfTopLevelImages(ctx);
        if (imageCount == 0) {
          throw new Exception("Failed.");
        }

        var imageIDs = new heif_item_id[imageCount];
        fixed (heif_item_id* ptr = imageIDs) {
          imageCount = LibHeifNative.HeifContextGetListOfTopLevelImageIDs(ctx, (UIntPtr) ptr, imageCount);
        }

        err = LibHeifNative.HeifContextGetImageHandle(ctx, imageIDs[0], ref imageHandle);
        if (CheckErrPtr(err)) {
          throw new Exception("Failed.");
        }

        var depth = LibHeifNative.HeifImageHandleGetLumaBitsPerPixel(imageHandle);
        if (depth < 0) {
          throw new Exception("Failed.");
        }

        options = LibHeifNative.HeifDecodingOptionsAlloc();

        var hasAlpha = LibHeifNative.HeifImageHandleHasAlphaChannel(imageHandle) != 0;

        HeifChroma chroma;
        System.Windows.Media.PixelFormat format;
        if (depth <= 8) {
          chroma = HeifChroma.HeifChromaInterleavedRgba;
          format = System.Windows.Media.PixelFormats.Bgra32;
        }
        else if (hasAlpha) {
          chroma = HeifChroma.HeifChromaInterleavedRrggbbaaLe;
          format = System.Windows.Media.PixelFormats.Rgba64;
        }
        else {
          chroma = HeifChroma.HeifChromaInterleavedRrggbbLe;
          format = System.Windows.Media.PixelFormats.Rgb48;
        }
        err = LibHeifNative.HeifDecodeImage(imageHandle, ref image, HeifColorspace.HeifColorspaceRgb,
          chroma, options);

        if (CheckErrPtr(err) || image == UIntPtr.Zero) {
          throw new Exception("Failed.");
        }

        var width = LibHeifNative.HeifImageGetWidth(image, HeifChannel.HeifChannelInterleaved);
        var height = LibHeifNative.HeifImageGetHeight(image, HeifChannel.HeifChannelInterleaved);
        b = new WriteableBitmap(width, height, dpi, dpi, format, null);
        b.Lock();

        var strideSource = 0;
        var strideDest = b.BackBufferStride;

        var raw = LibHeifNative.HeifImageGetPlaneReadonly(image, HeifChannel.HeifChannelInterleaved, ref strideSource);

        var ptrBegin = b.BackBuffer;
        if (strideSource != strideDest) {
          for (var line = 0; line < height; line++) {
            var src = raw + line * strideSource;
            var dst = ptrBegin + line * strideDest;
            Buffer.MemoryCopy(src.ToPointer(), dst.ToPointer(), strideSource, strideDest);
          }
        }
        else {
          Buffer.MemoryCopy(raw.ToPointer(), ptrBegin.ToPointer(), strideSource * height, strideDest * height);
        }

        Console.WriteLine();

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (chroma) {
          case HeifChroma.HeifChromaInterleavedRgba:
            Tools.BGRA2RGBA(b.BackBuffer, (UIntPtr) (width * height));
            break;
          case HeifChroma.HeifChromaInterleavedRrggbbaaLe:
            // Tools.BBGGRRAA2RRGGBBAA(bd.Scan0, (UIntPtr) (width * height));
            Tools.DEPTHCONVERT(b.BackBuffer, (UIntPtr) (width * height * 4), (UIntPtr) depth);
            // Tools.GDICOLORFIX(bd.Scan0, (UIntPtr) (width * height * 4), 2.2);
            break;
          case HeifChroma.HeifChromaInterleavedRrggbbLe:
            // Tools.BBGGRRAA2RRGGBBAA(bd.Scan0, (UIntPtr) (width * height));
            Tools.DEPTHCONVERT(b.BackBuffer, (UIntPtr) (width * height * 3), (UIntPtr) depth);
            // Tools.GDICOLORFIX(bd.Scan0, (UIntPtr) (width * height * 3), 2.2);
            break;
        }

        b.AddDirtyRect(new Int32Rect(0, 0, width, height));
      }
      finally {
        if (ctx != UIntPtr.Zero) {
          LibHeifNative.HeifContextFree(ctx);
        }

        if (imageHandle != UIntPtr.Zero) {
          LibHeifNative.HeifImageHandleRelease(imageHandle);
        }

        if (options != UIntPtr.Zero) {
          LibHeifNative.HeifDecodingOptionsFree(options);
        }

        if (image != UIntPtr.Zero) {
          LibHeifNative.HeifImageRelease(image);
        }

        b?.Unlock();
      }

      return b;
    }
  }
}
