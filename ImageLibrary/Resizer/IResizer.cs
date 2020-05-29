using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageLibrary.Resizer {
  public interface IResizer {
    void Resize(Bitmap src, Bitmap dst, object options = null);
  }
}
