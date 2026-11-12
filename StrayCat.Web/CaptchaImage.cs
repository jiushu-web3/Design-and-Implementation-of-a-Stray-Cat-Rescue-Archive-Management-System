using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;

namespace StrayCat.Web
{
    public static class CaptchaImage
    {
        public static Bitmap Draw(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                code = "0000";
            }
            Bitmap bitmap = new Bitmap(120, 42);
            Graphics graphics = Graphics.FromImage(bitmap);
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.FromArgb(255, 253, 248));
                byte[] noise = new byte[48];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(noise);
                }
                using (Pen pen = new Pen(Color.FromArgb(90, 196, 168, 154)))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        graphics.DrawLine(
                            pen,
                            noise[i] % 120,
                            noise[i + 3] % 42,
                            noise[i + 6] % 120,
                            noise[i + 9] % 42);
                    }
                }
                using (Font font = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    for (int i = 0; i < code.Length; i++)
                    {
                        int x = 8 + (i * 28);
                        int y = 6 + (noise[20 + i] % 8);
                        Color color = Color.FromArgb(120 + (noise[i] % 50), 40 + (noise[i + 12] % 40), 24);
                        using (Brush brush = new SolidBrush(color))
                        {
                            graphics.TranslateTransform(x, y);
                            graphics.RotateTransform((noise[30 + i] % 21) - 10);
                            graphics.DrawString(code.Substring(i, 1), font, brush, 0, 0);
                            graphics.ResetTransform();
                        }
                    }
                }
                for (int i = 0; i < 18; i++)
                {
                    bitmap.SetPixel(noise[i] % 120, noise[(i * 2) % 48] % 42, Color.FromArgb(180, 170, 160));
                }
            }
            finally
            {
                graphics.Dispose();
            }
            return bitmap;
        }
    }
}
