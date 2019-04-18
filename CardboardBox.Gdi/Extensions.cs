using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CardboardBox.Gdi
{
    public static class Extensions
    {
        public static Font FindBestFontSize(this Graphics gfx, string data, Size room, Font prefered)
        {
            var realSize = gfx.MeasureString(data, prefered);
            var heightScaleRatio = room.Height / realSize.Height;
            var widthScaleRatio = room.Width / realSize.Width;
            var scaleRatio = (heightScaleRatio < widthScaleRatio) ? heightScaleRatio : widthScaleRatio;
            var scaleFontSize = prefered.Size * scaleRatio;
            return new Font(prefered.FontFamily, scaleFontSize);
        }

        public static Bitmap Scale(this Bitmap bitmap, float maxHeight, float maxWidth)
        {
            if (bitmap.Height <= maxHeight && bitmap.Width <= maxWidth)
                return bitmap;

            var scale = Math.Min(maxHeight / bitmap.Height, maxWidth / bitmap.Width);

            return bitmap.ResizeImage((int)(bitmap.Width * scale), (int)(bitmap.Height * scale));
        }

        public static Bitmap ResizeImage(this Bitmap value, int newWidth, int newHeight)
        {
            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (var gfx = Graphics.FromImage(resizedImage))
            {
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.CompositingQuality = CompositingQuality.HighQuality;

                gfx.DrawImage(value, 0, 0, newWidth, newHeight);
            }
            return resizedImage;
        }

        public static Bitmap CropToCircle(this Bitmap srcImage, Color backGround, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            var dstImage = new Bitmap(srcImage.Width, srcImage.Height, format);
            using (var g = Graphics.FromImage(dstImage))
            {
                g.Clear(backGround);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, dstImage.Width, dstImage.Height);
                g.SetClip(path);
                g.DrawImage(srcImage, 0, 0);
            }
            return dstImage;
        }

        public static Bitmap ToBitmap(this byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                return new Bitmap(ms);
            }
        }

        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            using (var b = new Bitmap(bitmap))
            {
                b.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (pen == null)
                throw new ArgumentNullException("pen");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (brush == null)
                throw new ArgumentNullException("brush");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        public static FontFamily LoadFont(this byte[] fontData)
        {
            var pv = new PrivateFontCollection();
            var h = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            var p = h.AddrOfPinnedObject();

            FontFamily family;
            try
            {
                pv.AddMemoryFont(p, fontData.Length);
                family = pv.Families.FirstOrDefault();
            }
            catch
            {
                family = new FontFamily("Tahoma");
            }
            finally
            {
                h.Free();
            }

            return family;
        }
    }
}
