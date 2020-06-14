using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageLibrary.Decoder.Format.Heif.Extern;
using ImageLibrary.Tools;

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable MemberCanBePrivate.Global

namespace ImageLibrary.Decoder.Format.Heif {
  using heif_item_id = UInt32;

  public static class HeifDecoder {
    private static bool CheckErrPtr(IntPtr errPtr) {
      var errStruct = Marshal.PtrToStructure<HeifError>(errPtr);
      var result = errStruct.code != 0;
      LibHeifNative.HeifFreeHeifError(errPtr);
      return result;
    }

    public static DetectResult MagicDetect(byte[] header) {
      // TODO: Use Metadata extractor to detect format
      return DetectResult.NotSure;
    }

    public static unsafe IBitmapSource FromBytes(byte[] data) {
      fixed (byte* dataptr = data) {
        return FromPointer((IntPtr)dataptr, data.LongLength);
      }
    }

    public static unsafe IBitmapSource FromPointer(IntPtr data, long length) {
      var ctx = UIntPtr.Zero;
      var imageHandle = UIntPtr.Zero;
      var options = UIntPtr.Zero;
      var image = UIntPtr.Zero;
      MemoryBitmapSource b;

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
        int targetDepth;
        int targetChannel;
        if (depth <= 8) {
          chroma = HeifChroma.HeifChromaInterleavedRgba;
          targetDepth = 8;
          targetChannel = 4;
        }
        else if (hasAlpha) {
          chroma = HeifChroma.HeifChromaInterleavedRrggbbaaLe;
          targetDepth = 16;
          targetChannel = 4;
        }
        else {
          chroma = HeifChroma.HeifChromaInterleavedRrggbbLe;
          targetDepth = 16;
          targetChannel = 3;
        }
        err = LibHeifNative.HeifDecodeImage(imageHandle, ref image, HeifColorspace.HeifColorspaceRgb,
          chroma, options);

        if (CheckErrPtr(err) || image == UIntPtr.Zero) {
          throw new Exception("Failed.");
        }

        var width = LibHeifNative.HeifImageGetWidth(image, HeifChannel.HeifChannelInterleaved);
        var height = LibHeifNative.HeifImageGetHeight(image, HeifChannel.HeifChannelInterleaved);
        b = new MemoryBitmapSource(width, height, targetDepth, targetChannel);

        var strideSource = 0;
        var strideDest = b.Stride;

        var raw = LibHeifNative.HeifImageGetPlaneReadonly(image, HeifChannel.HeifChannelInterleaved, ref strideSource);

        var ptrBegin = b.Scan0;
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
            PixelTool.BGRA2RGBA(b.Scan0, (UIntPtr) (width * height));
            break;
          case HeifChroma.HeifChromaInterleavedRrggbbaaLe:
            PixelTool.DEPTHCONVERT(b.Scan0, (UIntPtr) (width * height * 4), (UIntPtr) depth);
            break;
          case HeifChroma.HeifChromaInterleavedRrggbbLe:
            PixelTool.DEPTHCONVERT(b.Scan0, (UIntPtr) (width * height * 3), (UIntPtr) depth);
            break;
        }
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
      }

      return b;
    }
  }
}
