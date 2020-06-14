using System.Windows;

namespace ImageLibrary {

  /// <summary>
  /// IImageSource is the type decoder returns, which may store all decoded data in memory,
  /// or defer actual decode upon request of Bitmap of a parition. 
  /// </summary>
  public interface IBitmapSource {
    Bitmap GetBitmap(Int32Rect pos);
    Bitmap FullBitmap { get; }
    int Width { get; }
    int Height { get; }
    int Depth { get; }
    int Channel { get; }
  }
}
