using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Filter {
  interface IFilter {
    void Filter(Bitmap src, Bitmap dst, object options = null);
  }

  interface IInplaceFilter {
    void Filter(Bitmap src, object options = null);
  }
}
