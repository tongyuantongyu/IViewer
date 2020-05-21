using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ImageDecoder.Flif;

namespace ImageTest {
  public partial class Form1 : Form {
    private bool isMoving;

    private int movX;
    private int movY;

    public Form1() {
      InitializeComponent();
      DoubleBuffered = true;
      SetStyle(ControlStyles.ResizeRedraw, true);
    }

    private void onMouseDown(object sender, MouseEventArgs e) {
      // Assign this method to mouse_Down event of Form or Panel,whatever you want
      if (4 >= MousePosition.Y - DesktopLocation.Y || MousePosition.Y - DesktopLocation.Y >= 40 ||
          4 >= MousePosition.X - DesktopLocation.X || MousePosition.X - DesktopLocation.X >= Width - 4 ||
          WindowState != FormWindowState.Normal) {
        return;
      }

      isMoving = true;
      movX = e.X;
      movY = e.Y;
    }

    private void onMouseMove(object sender, MouseEventArgs e) {
      // Assign this method to Mouse_Move event of that Form or Panel
      if (isMoving) {
        SetDesktopLocation(MousePosition.X - movX, MousePosition.Y - movY);
      }
    }

    private void onMouseUp(object sender, MouseEventArgs e) {
      // Assign this method to Mouse_Up event of Form or Panel.
      isMoving = false;
    }

    private void pictureBox1_Paint(object sender, PaintEventArgs e) {
      // var b = new Bitmap(1, 1, PixelFormat.Format64bppArgb);
      // var bitmap = (Bitmap) Image.FromFile("trans.png");
      // bitmap.SetResolution(b.HorizontalResolution, b.VerticalResolution);

      // var bd = _b.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.ReadOnly, PixelFormat.Format64bppPArgb);
      // Console.WriteLine("{0}",
      //   Convert.ToString(Marshal.ReadInt64(bd.Scan0), 2).PadLeft(64, '0'));
      // _b.UnlockBits(bd);
      // _b.Dispose();
      var d = File.ReadAllBytes("trans-16.flif");
      var bitmap = FlifDecoder.BitmapFromBytes(d);
      // e.Graphics.DrawImage(bitmap, 0, 0);
      // Console.WriteLine("{0} {1} {2} {3}", bitmap.Width, bitmap.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution);
      e.Graphics.DrawImageUnscaled(bitmap, 0, 0);
    }

    private void Form1_KeyUp(object sender, KeyEventArgs e) {
      switch (e.KeyCode) {
        case Keys.Escape:
          Application.Exit();
          break;
        case Keys.Space:
          switch (WindowState) {
            case FormWindowState.Normal:
              WindowState = FormWindowState.Maximized;
              break;
            case FormWindowState.Maximized:
              WindowState = FormWindowState.Normal;
              break;
          }

          break;
      }
    }

    private void pictureBox1_DoubleClick(object sender, System.EventArgs e) {
      if (MousePosition.Y - DesktopLocation.Y >= 40) {
        return;
      }

      switch (WindowState) {
        case FormWindowState.Normal:
          WindowState = FormWindowState.Maximized;
          break;
        case FormWindowState.Maximized:
          WindowState = FormWindowState.Normal;
          break;
      }
    }
  }
}