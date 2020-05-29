using System;
using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace ImageLibrary.Decoder.Format.Webp.Extern {

  public enum WEBP_CSP_MODE {
    MODE_RGB = 0,
    MODE_RGBA = 1,
    MODE_BGR = 2,
    MODE_BGRA = 3,
    MODE_ARGB = 4,
    MODE_RGBA_4444 = 5,
    MODE_RGB_565 = 6,
    MODE_rgbA = 7,
    MODE_bgrA = 8,
    MODE_Argb = 9,
    MODE_rgbA_4444 = 10,
    MODE_YUV = 11,
    MODE_YUVA = 12,
    MODE_LAST = 13,
  }

  public enum VP8StatusCode {
    VP8_STATUS_OK = 0,
    VP8_STATUS_OUT_OF_MEMORY,
    VP8_STATUS_INVALID_PARAM,
    VP8_STATUS_BITSTREAM_ERROR,
    VP8_STATUS_UNSUPPORTED_FEATURE,
    VP8_STATUS_SUSPENDED,
    VP8_STATUS_USER_ABORT,
    VP8_STATUS_NOT_ENOUGH_DATA,
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WebPBitstreamFeatures {
    public int width;          // Width in pixels, as read from the bitstream.
    public int height;         // Height in pixels, as read from the bitstream.
    public int has_alpha;      // True if the bitstream contains an alpha channel.
    public int has_animation;  // True if the bitstream is an animation.
    public int format;         // 0 = undefined (/mixed), 1 = lossy, 2 = lossless

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
    public uint[] pad;         // padding for later use
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WebPRGBABuffer {
    public IntPtr rgba;
    public int stride;
    public UIntPtr size;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WebPYUVABuffer {
    public IntPtr y;
    public IntPtr u;
    public IntPtr v;
    public IntPtr a;
    public int y_stride;
    public int u_stride;
    public int v_stride;
    public int a_stride;
    public UIntPtr y_size;
    public UIntPtr u_size;
    public UIntPtr v_size;
    public UIntPtr a_size;
  }

  [StructLayout(LayoutKind.Explicit)]
  public struct WebPDecBuffer_Union_0001_u
  {
    [FieldOffset(0)]
    public WebPRGBABuffer RGBA;
    [FieldOffset(0)]
    public WebPYUVABuffer YUVA;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WebPDecBuffer {
    public WEBP_CSP_MODE colorspace; // Colorspace.
    public int width, height; // Dimensions.
    public int is_external_memory;    
    // If non-zero, 'internal_memory' pointer is not
    // used. If value is '2' or more, the external
    // memory is considered 'slow' and multiple
    // read/write will be avoided.
    public WebPDecBuffer_Union_0001_u u; // Nameless union of buffer parameters.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U4)]
    public uint[] pad; // padding for later use

    public IntPtr private_memory;
    // Internally allocated memory (only when
    // is_external_memory is 0). Should not be used
    // externally, but accessed via the buffer union.
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WebPDecoderOptions {
    public int bypass_filtering;               // if true, skip the in-loop filtering
    public int no_fancy_upsampling;            // if true, use faster pointwise upsampler
    public int use_cropping;                   // if true, cropping is applied _first_
    public int crop_left, crop_top;
    // top-left position for cropping.
    // Will be snapped to even values.
    public int crop_width, crop_height;        // dimension of the cropping area
    public int use_scaling;                    // if true, scaling is applied _afterward_
    public int scaled_width, scaled_height;    // final resolution
    public int use_threads;                    // if true, use multi-threaded decoding
    public int dithering_strength;             // dithering strength (0=Off, 100=full)
    public int flip;                           // flip output vertically
    public int alpha_dithering_strength;       // alpha dithering strength in [0..100]

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
    public uint[] pad;                         // padding for later use
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WebPDecoderConfig {
    public WebPBitstreamFeatures input;  // Immutable bitstream features (optional)
    public WebPDecBuffer output;         // Output buffer (can point to external mem)
    public WebPDecoderOptions options;   // Decoding options
  }

  public static class LibWebpNative {
    /// WEBP_DECODER_ABI_VERSION 0x0209    // MAJOR(8b) + MINOR(8b)
    public const int WEBP_DECODER_ABI_VERSION = 0x0209;

    /// <summary>Initialize WebP Decoder. Must be called before config being used in decoding. </summary>
    /// <param name="config">WebPDecoderConfig to be initialized as empty. </param>
    /// <param name="version">WebP Decoder ABI Version. Refuse to init if ABI is incompatible. </param>
    /// <returns>Status code. 0 for failed, 1 for success. </returns>
    [DllImport("libwebp.dll", EntryPoint = "WebPInitDecoderConfigInternal", CallingConvention = CallingConvention.Cdecl)]
    public static extern int WebPInitDecoderConfigInternal(ref WebPDecoderConfig config, int version);

    /// <summary>Get WebP Image Features. </summary>
    /// <param name="data">Pointer to WebP image Data. </param>
    /// <param name="data_size">Size to given WebP image Data. </param>
    /// <param name="features">Pointer to WebPBitstreamFeatures. Will be filled with info after call. </param>
    /// <param name="version">WebP Decoder ABI Version. Return VP8_STATUS_INVALID_PARAM if ABI is incompatible. </param>
    /// <returns>Status code. 0 for failed, 1 for success. </returns>
    [DllImport("libwebp.dll", EntryPoint = "WebPGetFeaturesInternal", CallingConvention = CallingConvention.Cdecl)]
    public static extern VP8StatusCode WebPGetFeaturesInternal([In] IntPtr data, UIntPtr data_size, ref WebPBitstreamFeatures features, int version);

    /// <summary>Decode WebP Image according to config, and fill result. </summary>
    /// <param name="data">Pointer to WebP image Data. </param>
    /// <param name="data_size">Size to given WebP image Data. </param>
    /// <param name="config">WebPDecoderConfig. Contains decoding config, and will be filled with decoded data after call. </param>
    /// <returns>Status code. 0 for failed, 1 for success. </returns>
    [DllImport("libwebp.dll", EntryPoint = "WebPDecode", CallingConvention = CallingConvention.Cdecl)]
    public static extern VP8StatusCode WebPDecode([In] IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

    /// <summary>Estimate WebP image quality. </summary>
    /// <param name="data">Pointer to WebP image Data. </param>
    /// <param name="size">Size to given WebP image Data. </param>
    /// <returns>Quality. -1 for unknown, 101 for lossless, otherwise in 0-100 range. </returns>
    [DllImport("libwebp.dll", EntryPoint = "VP8EstimateQuality", CallingConvention = CallingConvention.Cdecl)]
    public static extern int VP8EstimateQuality([In] UIntPtr data, UIntPtr size);
  }
}
