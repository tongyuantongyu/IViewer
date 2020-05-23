using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppImageTest {
  public class Setting {
    public string N { get; set; }

    public Setting() {
      Random random=new Random();
      N = (random.Next()%20).ToString();
    }
  }
}
