using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ImageDecoder.Flif.Extern {
  using FlifDecoderPointer = IntPtr;
  using FlifImagePointer = IntPtr;
  using FlifInfoPointer = IntPtr;

  [StructLayout(LayoutKind.Sequential)]
  public struct FLIF_INFO {
    public int width;
    public int height;
    public byte channels;
    public byte bit_depth;
    public UIntPtr num_images;
  }

  public static class LibFlifNative {

    [DllImport("libflif_dec.dll", EntryPoint = "flif_create_decoder", CallingConvention = CallingConvention.Cdecl)]
    public static extern FlifDecoderPointer FlifCreateDecoder();

    [DllImport("libflif_dec.dll", EntryPoint = "flif_decoder_decode_memory", CallingConvention = CallingConvention.Cdecl)]
    public static extern int FlifDecoderDecodeMemory(FlifDecoderPointer decoder, [In] IntPtr buffer, UIntPtr size);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_decoder_num_images", CallingConvention = CallingConvention.Cdecl)]
    public static extern UIntPtr FlifDecoderNumImages(FlifDecoderPointer decoder);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_decoder_get_image", CallingConvention = CallingConvention.Cdecl)]
    public static extern FlifImagePointer FlifDecoderGetImage(FlifDecoderPointer decoder, UIntPtr index);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_image_get_width", CallingConvention = CallingConvention.Cdecl)]
    public static extern int FlifImageGetWidth(FlifImagePointer image);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_image_get_height", CallingConvention = CallingConvention.Cdecl)]
    public static extern int FlifImageGetHeight(FlifImagePointer image);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_image_get_nb_channels", CallingConvention = CallingConvention.Cdecl)]
    public static extern byte FlifImageGetNbChannels(FlifImagePointer image);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_image_get_depth", CallingConvention = CallingConvention.Cdecl)]
    public static extern byte FlifImageGetDepth(FlifImagePointer image);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_image_get_frame_delay", CallingConvention = CallingConvention.Cdecl)]
    public static extern int FlifImageGetFrameDelay(FlifImagePointer image);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_image_read_row_RGBA8", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FlifImageReadRowRgba8(FlifImagePointer image, int row,  [In] IntPtr buffer, UIntPtr size);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_image_read_row_RGBA16", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FlifImageReadRowRgba16(FlifImagePointer image, int row,  [In] IntPtr buffer, UIntPtr size);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_destroy_image", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FlifDestroyImage(FlifImagePointer image);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_destroy_decoder", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FlifDestroyDecoder(FlifDecoderPointer decoder);

    [DllImport("libflif_dec.dll", EntryPoint = "flif_destroy_info", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FlifDestroyInfo(FlifInfoPointer info);
  }

}
