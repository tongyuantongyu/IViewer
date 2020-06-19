using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Resizer {
  public interface IDoubler {
    void Double(Bitmap src, Bitmap dst, object options = null);
  }
}
