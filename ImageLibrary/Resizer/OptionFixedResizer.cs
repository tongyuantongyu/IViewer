using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Resizer {
  public class OptionFixedResizer : IResizer {
    private readonly IResizer resizer;
    private readonly object option;

    public OptionFixedResizer(IResizer resizer, object option) {
      this.resizer = resizer;
      this.option = option;
    }

    public void Resize(Bitmap src, Bitmap dst, object options = null) {
      resizer.Resize(src, dst, option);
    }
  }
}
