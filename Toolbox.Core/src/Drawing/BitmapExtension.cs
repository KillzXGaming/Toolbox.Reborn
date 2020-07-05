using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Toolbox.Core.Imaging
{
    public static class BitmapExtension
    {
        /// <summary>
        /// Converts a bitmap's image data into a byte array.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] ImageToByte(Bitmap bitmap)
        {
            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        public static Bitmap ToArgb32(Bitmap bmp)
        {
            Bitmap clone = new Bitmap(bmp.Width, bmp.Height,
                   PixelFormat.Format32bppArgb);

            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(bmp, new Rectangle(0, 0, clone.Width, clone.Height));
            }
            return clone;
        }

        /// <summary>
        /// Swaps the blue and red channels from a byte array of image data
        /// The image given must have 4 bytes used per pixel.
        /// </summary>
        /// <param name="bytes"></param>
        public static void ConvertBgraToRgba(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var temp = bytes[i];
                bytes[i] = bytes[i + 2];
                bytes[i + 2] = temp;
            }
        }

        /// <summary>
        /// Creates a bitmap from a given byte array, width, height and pixel format
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="pixelFormat"></param>
        /// <returns></returns>
        public static Bitmap CreateBitmap(byte[] Buffer, int Width, int Height, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            Rectangle Rect = new Rectangle(0, 0, Width, Height);

            Bitmap Img = new Bitmap(Width, Height, pixelFormat);

            BitmapData ImgData = Img.LockBits(Rect, ImageLockMode.WriteOnly, Img.PixelFormat);

            if (Buffer.Length > ImgData.Stride * Img.Height)
                throw new Exception($"Invalid Buffer Length ({Buffer.Length})!!!");

            Marshal.Copy(Buffer, 0, ImgData.Scan0, Buffer.Length);

            Img.UnlockBits(ImgData);

            return Img;
        }

        public static Bitmap Resize(Image original, Size size)
        {
            return ResizeImage(original, size.Width, size.Height);
        }

        public static Bitmap Resize(Image original, uint width, uint height)
        {
            return ResizeImage(original, (int)width, (int)height);
        }

        public static Bitmap Resize(Image original, int width, int height)
        {
            return ResizeImage(original, width, height);
        }

        public static Bitmap ResizeImage(Image image, int width, int height,
       InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic,
       SmoothingMode smoothingMode = SmoothingMode.HighQuality)
        {
            if (width == 0) width = 1;
            if (height == 0) height = 1;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = interpolationMode;
                graphics.SmoothingMode = smoothingMode;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap ShowChannel(Bitmap b, STChannelType channel)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                 ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 4;

                byte red, green, blue, alpha;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];
                        alpha = p[3];

                        if (channel == STChannelType.Red)
                        {
                            p[0] = red;
                            p[1] = red;
                            p[2] = red;
                            p[3] = 255;
                        }
                        else if (channel == STChannelType.Green)
                        {
                            p[0] = green;
                            p[1] = green;
                            p[2] = green;
                            p[3] = 255;
                        }
                        else if (channel == STChannelType.Blue)
                        {
                            p[0] = blue;
                            p[1] = blue;
                            p[2] = blue;
                            p[3] = 255;
                        }
                        else if (channel == STChannelType.Alpha)
                        {
                            p[0] = alpha;
                            p[1] = alpha;
                            p[2] = alpha;
                            p[3] = 255;
                        }

                        p += 4;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return b;
        }

        public static Bitmap SetChannel(Bitmap b,
           STChannelType channelR,
           STChannelType channelG,
           STChannelType channelB,
           STChannelType channelA)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
 ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 4;

                byte red, green, blue, alpha;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];
                        alpha = p[3];

                        p[2] = SetChannelByte(channelR, red, green, blue, alpha);
                        p[1] = SetChannelByte(channelG, red, green, blue, alpha);
                        p[0] = SetChannelByte(channelB, red, green, blue, alpha);
                        p[3] = SetChannelByte(channelA, red, green, blue, alpha);

                        p += 4;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return b;
        }

        private static byte SetChannelByte(STChannelType channel, byte r, byte g, byte b, byte a)
        {
            switch (channel)
            {
                case STChannelType.Red: return r;
                case STChannelType.Green: return g;
                case STChannelType.Blue: return b;
                case STChannelType.Alpha: return a;
                case STChannelType.One: return 255;
                case STChannelType.Zero: return 0;
                default:
                    throw new Exception("Unknown channel type! " + channel);
            }
        }

        public static bool SetChannels(Bitmap b, bool UseRed, bool UseBlue, bool UseGreen, bool UseAlpha)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
     ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 4;

                byte red, green, blue, alpha;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];
                        alpha = p[3];

                        if (!UseRed)
                            red = 0;
                        if (!UseGreen)
                            green = 0;
                        if (!UseBlue)
                            blue = 0;
                        if (!UseAlpha)
                            alpha = 0;

                        p[2] = red;
                        p[1] = green;
                        p[0] = blue;
                        p[3] = alpha;

                        p += 4;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return true;
        }

        public static Bitmap FillColor(int Width, int Height, Color color)
        {
            Bitmap Bmp = new Bitmap(Width, Height);
            using (Graphics gfx = Graphics.FromImage(Bmp))
            using (SolidBrush brush = new SolidBrush(color))
            {
                gfx.FillRectangle(brush, 0, 0, Width, Height);
            }
            return Bmp;
        }

        public static Bitmap GrayScale(Image b, bool removeAlpha = false)
        {
            return GrayScale(new Bitmap(b), removeAlpha);
        }

        public static Bitmap GrayScale(Bitmap b, bool removeAlpha = false)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
        ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 4;

                byte red, green, blue, alpha;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];
                        if (removeAlpha)
                            alpha = 255;
                        else
                            alpha = p[3];

                        p[0] = p[1] = p[2] = (byte)(.299 * red
                            + .587 * green
                            + .114 * blue);

                        p += 4;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return b;
        }
        public static bool Invert(Bitmap b)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width * 3;
                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        p[0] = (byte)(255 - p[0]);
                        ++p;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return true;
        }

        public static Bitmap CreateImageThumbnail(Bitmap image, int width, int height)
        {
            int tw, th, tx, ty;

            int w = image.Width;
            int h = image.Height;

            double whRatio = (double)w / h;
            if (image.Width >= image.Height)
            {
                tw = width;
                th = (int)(tw / whRatio);
            }
            else
            {
                th = height;
                tw = (int)(th * whRatio);
            }

            tx = (width - tw) / 2;
            ty = (height - th) / 2;

            Bitmap thumb = new Bitmap(width, height, image.PixelFormat);

            Graphics g = Graphics.FromImage(thumb);

            //  g.Clear(Color.White);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(image, new Rectangle(tx, ty, tw, th),
            new Rectangle(0, 0, w, h),

            GraphicsUnit.Pixel);

            return thumb;

        }

        public static Bitmap AdjustBrightness(Image image, float level)
        {
            ImageAttributes attributes = new ImageAttributes();

            ColorMatrix cm = new ColorMatrix(new float[][]
            {
            new float[] { level, 0, 0, 0, 0},
            new float[] {0, level, 0, 0, 0},
            new float[] {0, 0, level, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1},
            });
            attributes.SetColorMatrix(cm);

            Point[] points =
            {
            new Point(0, 0),
            new Point(image.Width, 0),
            new Point(0, image.Height),
           };
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            Bitmap bm = new Bitmap(image.Width, image.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }
            return bm;
        }

        public static Bitmap AdjustGamma(Image image, float gamma)
        {
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetGamma(gamma);

            Point[] points =
            {
            new Point(0, 0),
            new Point(image.Width, 0),
            new Point(0, image.Height),
           };
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            Bitmap bm = new Bitmap(image.Width, image.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }
            return bm;
        }
    }
}
