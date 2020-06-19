using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Resizer {
  public class AutoSwitchResizer : IResizer {
    private readonly IResizer enlargeResizer;
    private readonly IResizer shrinkResizer;
    private readonly IDoubler doubler;

    public AutoSwitchResizer(IResizer enlarge, IResizer shrink, IDoubler doubler = null) {
      enlargeResizer = enlarge;
      shrinkResizer = shrink;
      this.doubler = doubler;
    }

    public void Resize(Bitmap src, Bitmap dst, object options = null) {
      var scaleX = (double)dst.Width / src.Width;
      var scaleY = (double)dst.Height / src.Height;
      var scale = Math.Max(scaleX, scaleY);

      if (doubler == null) {
        if (scale > 1) {
          enlargeResizer.Resize(src, dst);
        }
        else {
          shrinkResizer.Resize(src, dst);
        }

        return;
      }

      if (Math.Abs(scaleX - 2) < 0.0001 && Math.Abs(scaleY - 2) < 0.0001) {
        doubler.Double(src, dst);
        return;
      }
      if (Math.Abs(scaleX - 4) < 0.0001 && Math.Abs(scaleY - 4) < 0.0001) {
        Four(src, dst);
        return;
      }

      if (scale > 4) {
        var temp = new byte[src.Width * src.Height * src.Channel * src.Depth << 1];
        var handle = GCHandle.Alloc(temp, GCHandleType.Pinned);
        var tempBitmap = new Bitmap(handle.AddrOfPinnedObject(),
          4 * src.Width * src.Channel * (src.Depth >> 3),
          4 * src.Width, 4 * src.Height, src.Depth, src.Channel);
        Four(src, tempBitmap);
        enlargeResizer.Resize(tempBitmap, dst);
        handle.Free();
        return;
      }

      if (scale > 2) {
        var temp = new byte[src.Width * src.Height * src.Channel * src.Depth << 1];
        var handle = GCHandle.Alloc(temp, GCHandleType.Pinned);
        var tempBitmap = new Bitmap(handle.AddrOfPinnedObject(),
          4 * src.Width * src.Channel * (src.Depth >> 3),
          4 * src.Width, 4 * src.Height, src.Depth, src.Channel);
        Four(src, tempBitmap);
        shrinkResizer.Resize(tempBitmap, dst);
        handle.Free();
        return;
      }

      if (scale > 1) {
        var temp = new byte[src.Width * src.Height * src.Channel * src.Depth >> 1];
        var handle = GCHandle.Alloc(temp, GCHandleType.Pinned);
        var tempBitmap = new Bitmap(handle.AddrOfPinnedObject(),
          2 * src.Width * src.Channel * (src.Depth >> 3),
          2 * src.Width, 2 * src.Height, src.Depth, src.Channel);
        doubler.Double(src, tempBitmap);
        shrinkResizer.Resize(tempBitmap, dst);
        handle.Free();
        return;
      }

      shrinkResizer.Resize(src, dst);
    }

    private void Four(Bitmap src, Bitmap dst, object options = null) {
      var temp = new byte[src.Width * src.Height * src.Channel * src.Depth >> 1];
      var handle = GCHandle.Alloc(temp, GCHandleType.Pinned);
      var tempBitmap = new Bitmap(handle.AddrOfPinnedObject(),
        2 * src.Width * src.Channel * (src.Depth >> 3),
        2 * src.Width, 2 * src.Height, src.Depth, src.Channel);
      doubler.Double(src, tempBitmap);
      doubler.Double(tempBitmap, dst);
      handle.Free();
    }
  }
}
