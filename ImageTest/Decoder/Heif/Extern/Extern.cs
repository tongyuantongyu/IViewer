using System;
using System.Runtime.InteropServices;
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace ImageDecoder.Heif.Extern {
  using HeifContextPointer = UIntPtr;
  using HeifImageHandlePointer = UIntPtr;
  using HeifImagePointer = UIntPtr;
  using HeifItemId = UInt32;
  using HeifDecodingOptionsPointer = UIntPtr;

  public enum HeifCompressionFormat {
    HeifCompressionUndefined = 0,
    HeifCompressionHevc = 1,
    HeifCompressionAvc = 2,
    HeifCompressionJpeg = 3
  }
  public enum HeifColorspace {
    HeifColorspaceUndefined=99,
    HeifColorspaceYCbCr=0,
    HeifColorspaceRgb  =1,
    HeifColorspaceMonochrome=2
  }
  public enum HeifChroma {
    HeifChromaUndefined=99,
    HeifChromaMonochrome=0,
    HeifChroma420=1,
    HeifChroma422=2,
    HeifChroma444=3,
    HeifChromaInterleavedRgb =10,
    HeifChromaInterleavedRgba=11,
    HeifChromaInterleavedRrggbbBe  =12,
    HeifChromaInterleavedRrggbbaaBe=13,
    HeifChromaInterleavedRrggbbLe  =14,
    HeifChromaInterleavedRrggbbaaLe=15
  }
  public enum HeifChannel {
    HeifChannelY = 0,
    HeifChannelCb = 1,
    HeifChannelCr = 2,
    HeifChannelR = 3,
    HeifChannelG = 4,
    HeifChannelB = 5,
    HeifChannelAlpha = 6,
    HeifChannelInterleaved = 10
  }
  public enum HeifErrorCode {
    HeifErrorOk = 0,
    HeifErrorInputDoesNotExist = 1,
    HeifErrorInvalidInput = 2,
    HeifErrorUnsupportedFiletype = 3,
    HeifErrorUnsupportedFeature = 4,
    HeifErrorUsageError = 5,
    HeifErrorMemoryAllocationError = 6,
    HeifErrorDecoderPluginError = 7,
    HeifErrorEncoderPluginError = 8,
    HeifErrorEncodingError = 9
  }

  public enum HeifSuberrorCode {
    HeifSuberrorUnspecified = 0,
    HeifSuberrorEndOfData = 100,
    HeifSuberrorInvalidBoxSize = 101,
    HeifSuberrorNoFtypBox = 102,
    HeifSuberrorNoIdatBox = 103,
    HeifSuberrorNoMetaBox = 104,
    HeifSuberrorNoHdlrBox = 105,
    HeifSuberrorNoHvcCBox = 106,
    HeifSuberrorNoPitmBox = 107,
    HeifSuberrorNoIpcoBox = 108,
    HeifSuberrorNoIpmaBox = 109,
    HeifSuberrorNoIlocBox = 110,
    HeifSuberrorNoIinfBox = 111,
    HeifSuberrorNoIprpBox = 112,
    HeifSuberrorNoIrefBox = 113,
    HeifSuberrorNoPictHandler = 114,
    HeifSuberrorIpmaBoxReferencesNonexistingProperty = 115,
    HeifSuberrorNoPropertiesAssignedToItem = 116,
    HeifSuberrorNoItemData = 117,
    HeifSuberrorInvalidGridData = 118,
    HeifSuberrorMissingGridImages = 119,
    HeifSuberrorInvalidCleanAperture = 120,
    HeifSuberrorInvalidOverlayData = 121,
    HeifSuberrorOverlayImageOutsideOfCanvas = 122,
    HeifSuberrorAuxiliaryImageTypeUnspecified = 123,
    HeifSuberrorNoOrInvalidPrimaryItem = 124,
    HeifSuberrorNoInfeBox = 125,
    HeifSuberrorUnknownColorProfileType = 126,
    HeifSuberrorWrongTileImageChromaFormat = 127,
    HeifSuberrorInvalidFractionalNumber = 128,
    HeifSuberrorInvalidImageSize = 129,
    HeifSuberrorInvalidPixiBox = 130,
    HeifSuberrorSecurityLimitExceeded = 1000,
    HeifSuberrorNonexistingItemReferenced = 2000,
    HeifSuberrorNullPointerArgument = 2001,
    HeifSuberrorNonexistingImageChannelReferenced = 2002,
    HeifSuberrorUnsupportedPluginVersion = 2003,
    HeifSuberrorUnsupportedWriterVersion = 2004,
    HeifSuberrorUnsupportedParameter = 2005,
    HeifSuberrorInvalidParameterValue = 2006,
    HeifSuberrorUnsupportedCodec = 3000,
    HeifSuberrorUnsupportedImageType = 3001,
    HeifSuberrorUnsupportedDataVersion = 3002,
    HeifSuberrorUnsupportedColorConversion = 3003,
    HeifSuberrorUnsupportedItemConstructionMethod = 3004,
    HeifSuberrorUnsupportedBitDepth = 4000,
    HeifSuberrorCannotWriteOutputData = 5000,
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
  public struct HeifError {
    public HeifErrorCode code;
    public HeifSuberrorCode subcode;
    [MarshalAs(UnmanagedType.LPStr)]
    public string message;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct HeifDecodingOptions {
    public byte version;
    public byte ignore_transformations;
    public UIntPtr start_progress;
    public UIntPtr on_progress;
    public UIntPtr end_progress;
    public UIntPtr progress_user_data;
  };

  public static class LibHeifNative {
    [DllImport("libheif.dll", EntryPoint = "heif_context_alloc", CallingConvention = CallingConvention.Cdecl)]
    public static extern HeifContextPointer HeifContextAlloc();

    [DllImport("libheif.dll", EntryPoint = "heif_context_free", CallingConvention = CallingConvention.Cdecl)]
    public static extern void HeifContextFree(HeifContextPointer ctx);

    [DllImport("libheif.dll", EntryPoint = "p_heif_context_read_from_memory_without_copy", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr HeifContextReadFromMemoryWithoutCopy(HeifContextPointer ctx, [In] IntPtr mem, UIntPtr size, [In] UIntPtr unused);

    [DllImport("libheif.dll", EntryPoint = "p_heif_context_get_primary_image_ID", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr HeifContextGetPrimaryImageID(HeifContextPointer ctx, ref HeifItemId id);

    [DllImport("libheif.dll", EntryPoint = "p_heif_context_get_primary_image_handle", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr HeifContextGetPrimaryImageHandle(HeifContextPointer ctx, ref HeifImageHandlePointer imgHdl);

    [DllImport("libheif.dll", EntryPoint = "heif_context_get_number_of_top_level_images", CallingConvention = CallingConvention.Cdecl)]
    public static extern int HeifContextGetNumberOfTopLevelImages(HeifContextPointer ctx);

    [DllImport("libheif.dll", EntryPoint = "heif_context_get_list_of_top_level_image_IDs", CallingConvention = CallingConvention.Cdecl)]
    public static extern int HeifContextGetListOfTopLevelImageIDs(HeifContextPointer ctx, UIntPtr idArray, int count);

    [DllImport("libheif.dll", EntryPoint = "p_heif_context_get_image_handle", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr HeifContextGetImageHandle(HeifContextPointer ctx, HeifItemId id, ref HeifImageHandlePointer imgHdl);

    [DllImport("libheif.dll", EntryPoint = "heif_image_handle_release", CallingConvention = CallingConvention.Cdecl)]
    public static extern void HeifImageHandleRelease(HeifImageHandlePointer handle);
    
    [DllImport("libheif.dll", EntryPoint = "heif_decoding_options_alloc", CallingConvention = CallingConvention.Cdecl)]
    public static extern HeifDecodingOptionsPointer HeifDecodingOptionsAlloc();

    [DllImport("libheif.dll", EntryPoint = "heif_decoding_options_free", CallingConvention = CallingConvention.Cdecl)]
    public static extern void HeifDecodingOptionsFree(HeifDecodingOptionsPointer options);

    [DllImport("libheif.dll", EntryPoint = "heif_image_handle_get_luma_bits_per_pixel", CallingConvention = CallingConvention.Cdecl)]
    public static extern int HeifImageHandleGetLumaBitsPerPixel(HeifImageHandlePointer handle);

    [DllImport("libheif.dll", EntryPoint = "heif_image_handle_has_alpha_channel", CallingConvention = CallingConvention.Cdecl)]
    public static extern int HeifImageHandleHasAlphaChannel(HeifImagePointer img);

    [DllImport("libheif.dll", EntryPoint = "heif_image_get_width", CallingConvention = CallingConvention.Cdecl)]
    public static extern int HeifImageGetWidth(HeifImagePointer img, HeifChannel channel);

    [DllImport("libheif.dll", EntryPoint = "heif_image_get_height", CallingConvention = CallingConvention.Cdecl)]
    public static extern int HeifImageGetHeight(HeifImagePointer img, HeifChannel channel);

    [DllImport("libheif.dll", EntryPoint = "heif_image_get_bits_per_pixel_range", CallingConvention = CallingConvention.Cdecl)]
    public static extern int HeifImageGetBitsPerPixelRange(HeifImagePointer img, HeifChannel channel);

    [DllImport("libheif.dll", EntryPoint = "p_heif_decode_image", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr HeifDecodeImage(HeifImageHandlePointer inHandle, ref HeifImagePointer outImg, HeifColorspace colorspace, HeifChroma chroma, HeifDecodingOptionsPointer options);

    [DllImport("libheif.dll", EntryPoint = "heif_image_get_plane_readonly", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr HeifImageGetPlaneReadonly(HeifImagePointer image, HeifChannel channel, ref int outStride);

    [DllImport("libheif.dll", EntryPoint = "heif_image_release", CallingConvention = CallingConvention.Cdecl)]
    public static extern void HeifImageRelease(HeifImagePointer img);

    [DllImport("libheif.dll", EntryPoint = "heif_free_heif_error", CallingConvention = CallingConvention.Cdecl)]
    public static extern void HeifFreeHeifError(IntPtr errPtr);
  }

}
