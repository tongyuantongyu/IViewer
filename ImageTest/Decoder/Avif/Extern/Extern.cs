using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ImageDecoder.Avif.Extern {

  public enum AvifResult {
    AvifResultOk = 0,
    AvifResultUnknownError = 1,
    AvifResultInvalidFtyp = 2,
    AvifResultNoContent = 3,
    AvifResultNoYuvFormatSelected = 4,
    AvifResultReformatFailed = 5,
    AvifResultUnsupportedDepth = 6,
    AvifResultEncodeColorFailed = 7,
    AvifResultEncodeAlphaFailed = 8,
    AvifResultBmffParseFailed = 9,
    AvifResultNoAv1ItemsFound = 10,
    AvifResultDecodeColorFailed = 11,
    AvifResultDecodeAlphaFailed = 12,
    AvifResultColorAlphaSizeMismatch = 13,
    AvifResultIspeSizeMismatch = 14,
    AvifResultNoCodecAvailable = 15,
    AvifResultNoImagesRemaining = 16,
    AvifResultInvalidExifPayload = 17,
    AvifResultInvalidImageGrid = 18
  }

  public enum AvifPixelFormat {
    AvifPixelFormatNone = 0,
    AvifPixelFormatYuv444 = 1,
    AvifPixelFormatYuv422 = 2,
    AvifPixelFormatYuv420 = 3,
    AvifPixelFormatYv12 = 4
  }

  [Flags]
  public enum AvifRange {
    AvifRangeLimited = 0,
    AvifRangeFull = 128
  }

  public enum AvifNclxColourPrimaries {
    AvifNclxColourPrimariesUnknown = 0,
    AvifNclxColourPrimariesBt709 = 1,
    AvifNclxColourPrimariesIec6196624 = 1,
    AvifNclxColourPrimariesUnspecified = 2,
    AvifNclxColourPrimariesBt470M = 4,
    AvifNclxColourPrimariesBt470Bg = 5,
    AvifNclxColourPrimariesBt601 = 6,
    AvifNclxColourPrimariesSmpte240 = 7,
    AvifNclxColourPrimariesGenericFilm = 8,
    AvifNclxColourPrimariesBt2020 = 9,
    AvifNclxColourPrimariesXyz = 10,
    AvifNclxColourPrimariesSmpte431 = 11,
    AvifNclxColourPrimariesSmpte432 = 12,
    AvifNclxColourPrimariesEbu3213 = 22
  }

  public enum AvifNclxTransferCharacteristics {
    AvifNclxTransferCharacteristicsUnknown = 0,
    AvifNclxTransferCharacteristicsBt709 = 1,
    AvifNclxTransferCharacteristicsUnspecified = 2,
    AvifNclxTransferCharacteristicsBt470M = 4,
    AvifNclxTransferCharacteristicsBt470Bg = 5,
    AvifNclxTransferCharacteristicsBt601 = 6,
    AvifNclxTransferCharacteristicsSmpte240 = 7,
    AvifNclxTransferCharacteristicsLinear = 8,
    AvifNclxTransferCharacteristicsLog100 = 9,
    AvifNclxTransferCharacteristicsLog100Sqrt10 = 10,
    AvifNclxTransferCharacteristicsIec61966 = 11,
    AvifNclxTransferCharacteristicsBt1361 = 12,
    AvifNclxTransferCharacteristicsSrgb = 13,
    AvifNclxTransferCharacteristicsBt202010Bit = 14,
    AvifNclxTransferCharacteristicsBt202012Bit = 15,
    AvifNclxTransferCharacteristicsSmpte2084 = 16,
    AvifNclxTransferCharacteristicsSmpte428 = 17,
    AvifNclxTransferCharacteristicsHlg = 18
  }

  public enum AvifNclxMatrixCoefficients {
    AvifNclxMatrixCoefficientsIdentity = 0,
    AvifNclxMatrixCoefficientsBt709 = 1,
    AvifNclxMatrixCoefficientsUnspecified = 2,
    AvifNclxMatrixCoefficientsFcc = 4,
    AvifNclxMatrixCoefficientsBt470Bg = 5,
    AvifNclxMatrixCoefficientsBt601 = 6,
    AvifNclxMatrixCoefficientsSmpte240 = 7,
    AvifNclxMatrixCoefficientsYcgco = 8,
    AvifNclxMatrixCoefficientsBt2020Ncl = 9,
    AvifNclxMatrixCoefficientsBt2020Cl = 10,
    AvifNclxMatrixCoefficientsSmpte2085 = 11,
    AvifNclxMatrixCoefficientsChromaDerivedNcl = 12,
    AvifNclxMatrixCoefficientsChromaDerivedCl = 13,
    AvifNclxMatrixCoefficientsIctcp = 14
  }

  public enum AvifProfileFormat {
    AvifProfileFormatNone = 0,
    AvifProfileFormatIcc = 1,
    AvifProfileFormatNclx = 2
  }

  [Flags]
  public enum AvifTransformationFlags {
    AvifTransformNone = 0,
    AvifTransformPasp = 1,
    AvifTransformClap = 2,
    AvifTransformIrot = 4,
    AvifTransformImir = 8
  }

  public enum AvifRgbFormat {
    AvifRgbFormatRgb = 0,
    AvifRgbFormatRgba = 1,
    AvifRgbFormatArgb = 2,
    AvifRgbFormatBgr = 3,
    AvifRgbFormatBgra = 4,
    AvifRgbFormatAbgr = 5
  }

  public enum AvifCodecChoice {
    AvifCodecChoiceAuto = 0,
    AvifCodecChoiceAom = 1,
    AvifCodecChoiceDav1D = 2,
    AvifCodecChoiceLibgav1 = 3,
    AvifCodecChoiceRav1E = 4
  }

  public enum AvifDecoderSource {
    AvifDecoderSourceAuto = 0,
    AvifDecoderSourcePrimaryItem = 1,
    AvifDecoderSourceTracks = 2
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifRoData {
    public IntPtr data;
    public UIntPtr size;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifImageTiming {
    public long timescale; // timescale of the media (Hz)
    public double pts; // presentation timestamp in seconds (ptsInTimescales / timescale)
    public long ptsInTimescales; // presentation timestamp in "timescales"
    public double duration; // in seconds (durationInTimescales / timescale)
    public long durationInTimescales; // duration in "timescales"
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifIOStats {
    public UIntPtr colorOBUSize;
    public UIntPtr alphaOBUSize;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifPixelAspectRatioBox {
    // 'pasp' from ISO/IEC 14496-12:2015 12.1.4.3

    // define the relative width and height of a pixel
    public int hSpacing;
    public int vSpacing;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifCleanApertureBox {
    // 'clap' from ISO/IEC 14496-12:2015 12.1.4.3

    // a fractional number which defines the exact clean aperture width, in counted pixels, of the video image
    public int widthN;
    public int widthD;

    // a fractional number which defines the exact clean aperture height, in counted pixels, of the video image
    public int heightN;
    public int heightD;

    // a fractional number which defines the horizontal offset of clean aperture centre minus (width‐1)/2. Typically 0.
    public int horizOffN;
    public int horizOffD;

    // a fractional number which defines the vertical offset of clean aperture centre minus (height‐1)/2. Typically 0.
    public int vertOffN;
    public int vertOffD;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifImageRotation {
    // 'irot' from ISO/IEC 23008-12:2017 6.5.10

    // angle * 90 specifies the angle (in anti-clockwise direction) in units of degrees.
    public char angle; // legal values: [0-3]
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifImageMirror {
    // 'imir' from ISO/IEC 23008-12:2017 6.5.12

    // axis specifies a vertical (axis = 0) or horizontal (axis = 1) axis for the mirroring operation.
    public char axis; // legal values: [0, 1]
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifImage {
    // Image information
    public int width;
    public int height;
    public int depth; // all planes must share this depth; if depth>8, all planes are uint16_t internally

    public AvifPixelFormat yuvFormat;
    public AvifRange yuvRange;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.SysUInt)]
    public IntPtr[] yuvPlanes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U4)]
    public int[] yuvRowBytes;

    public char decoderOwnsYUVPlanes;

    public AvifRange alphaRange;
    public IntPtr alphaPlane;
    public int alphaRowBytes;
    public char decoderOwnsAlphaPlane;

    // Profile information
    public AvifProfileFormat profileFormat;
    public IntPtr iccData;
    public UIntPtr iccSize;

    public AvifNclxColourPrimaries nclxColourPrimaries;
    public AvifNclxTransferCharacteristics nclxTransferCharacteristics;
    public AvifNclxMatrixCoefficients nclxMatrixCoefficients;
    public AvifRange nclxColorRange;

    // Transformations - These metadata values are encoded/decoded when transformFlags are set
    // appropriately, but do not impact/adjust the actual pixel buffers used (images won't be
    // pre-cropped or mirrored upon decode). Basic explanations from the standards are offered in
    // comments above, but for detailed explanations, please refer to the HEIF standard (ISO/IEC
    // 23008-12:2017) and the BMFF standard (ISO/IEC 14496-12:2015).
    //
    // To encode any of these boxes, set the values in the associated box, then enable the flag in
    // transformFlags. On decode, only honor the values in boxes with the associated transform flag set.
    public int transformFlags;
    public AvifPixelAspectRatioBox pasp;
    public AvifCleanApertureBox clap;
    public AvifImageRotation irot;
    public AvifImageMirror imir;

    // Metadata - set with avifImageSetMetadata*() before write, check .size>0 for existence after read
    public IntPtr exifData;
    public UIntPtr exifSize;
    public IntPtr xmpData;
    public UIntPtr xmpSize;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifDecoder {
    // Defaults to AVIF_CODEC_CHOICE_AUTO: Preference determined by order in availableCodecs table (avif.c)
    public AvifCodecChoice codecChoice;

    // avifs can have multiple sets of images in them. This specifies which to decode.
    // Set this via avifDecoderSetSource().
    public AvifDecoderSource requestedSource;

    // The current decoded image, owned by the decoder. Is invalid if the decoder hasn't run or has run
    // out of images. The YUV and A contents of this image are likely owned by the decoder, so be
    // sure to copy any data inside of this image before advancing to the next image or reusing the
    // decoder. It is legal to call avifImageYUVToRGB() on this in between calls to avifDecoderNextImage(),
    // but use avifImageCopy() if you want to make a permanent copy of this image's contents.
    public IntPtr image;

    // Counts and timing for the current image in an image sequence. Uninteresting for single image files.
    public int imageIndex; // 0-based
    public int imageCount; // Always 1 for non-sequences
    public AvifImageTiming imageTiming; //
    public long timescale; // timescale of the media (Hz)
    public double duration; // in seconds (durationInTimescales / timescale)
    public long durationInTimescales; // duration in "timescales"

    // The width and height as reported by the AVIF container, if any. There is no guarantee
    // these match the decoded images; they are merely reporting what is independently offered
    // from the container's boxes.
    // * If decoding an "item" and the item is associated with an ImageSpatialExtentsBox,
    //   it will use the box's width/height
    // * Else if decoding tracks, these will be the integer portions of the TrackHeaderBox width/height
    // * Else both will be set to 0.
    public int containerWidth;
    public int containerHeight;

    // The bit depth as reported by the AVIF container, if any. There is no guarantee
    // this matches the decoded images; it is merely reporting what is independently offered
    // from the container's boxes.
    // * If decoding an "item" and the item is associated with an av1C property,
    //   it will use the box's depth flags.
    // * Else if decoding tracks and there is a SampleDescriptionBox of type av01 containing an av1C box,
    //   it will use the box's depth flags.
    // * Else it will be set to 0.
    public int containerDepth;

    // stats from the most recent read, possibly 0s if reading an image sequence
    public AvifIOStats ioStats;

    // Internals used by the decoder
    public IntPtr data;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AvifRGBImage {
    public int width; // must match associated avifImage
    public int height; // must match associated avifImage
    public int depth; // legal depths [8, 10, 12, 16]. if depth>8, pixels must be uint16_t internally
    public AvifRgbFormat format; // all channels are always full range

    public IntPtr pixels; // Decode memory begin
    public int rowBytes; // aka Stride
  }

  public static class LibAvifNative {
    [DllImport("libavif.dll", EntryPoint = "avifDecoderCreate", CallingConvention = CallingConvention.Cdecl)]
    public static extern ref AvifDecoder AvifDecoderCreate();

    [DllImport("libavif.dll", EntryPoint = "avifDecoderDestroy", CallingConvention = CallingConvention.Cdecl)]
    public static extern void AvifDecoderDestroy(ref AvifDecoder decoder);

    [DllImport("libavif.dll", EntryPoint = "avifDecoderParse", CallingConvention = CallingConvention.Cdecl)]
    public static extern AvifResult AvifDecoderParse(ref AvifDecoder decoder, ref AvifRoData rawInput);

    [DllImport("libavif.dll", EntryPoint = "avifDecoderNextImage", CallingConvention = CallingConvention.Cdecl)]
    public static extern AvifResult AvifDecoderNextImage(ref AvifDecoder decoder);

    [DllImport("libavif.dll", EntryPoint = "avifDecoderNthImage", CallingConvention = CallingConvention.Cdecl)]
    public static extern AvifResult AvifDecoderNthImage(ref AvifDecoder decoder, int frameIndex);

    [DllImport("libavif.dll", EntryPoint = "avifRGBImageSetDefaults", CallingConvention = CallingConvention.Cdecl)]
    public static extern void AvifRGBImageSetDefaults(ref AvifRGBImage rgb, ref AvifImage image);

    [DllImport("libavif.dll", EntryPoint = "avifImageYUVToRGB", CallingConvention = CallingConvention.Cdecl)]
    public static extern AvifResult AvifImageYUVToRGB(ref AvifImage image, ref AvifRGBImage rgb);
  }
}