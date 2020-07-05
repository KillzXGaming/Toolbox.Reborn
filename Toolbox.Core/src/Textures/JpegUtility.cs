using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Toolbox.Core.Imaging
{
    public class JpegUtility
    {
        //Information on this can be found here ww.amnoid.de/gc/

        /// <summary>
        /// Turns a jpeg thp encoded image to RGBA image data.
        /// Commonly found in gamecube video file formats such as .thp and .movie.
        /// </summary>
        /// <param name="ImageData"></param>
        /// <returns></returns>
        public static byte[] DecodeTHPJpeg(byte[] ImageData)
        {
            var jpegFile = ConvertJpeg(ImageData, (int)ImageData.Length);
            var jpgBitmap = new Bitmap((Stream)new MemoryStream(jpegFile));
            var rgba = BitmapExtension.ToArgb32(jpgBitmap);
            return BitmapExtension.ImageToByte(rgba);
        }

        static byte[] ConvertJpeg(byte[] data, int size)
        {
            int start = 0, end = 0;
            int newSize = countRequiredSize(data, size, ref start, ref end);
            byte[] buffer = new byte[newSize];
            convertToRealJpeg(buffer, data, size, start, end);
            return buffer;
        }

        static int countRequiredSize(byte[] data, int size, ref int start, ref int end)
        {
            start = 2 * size;
            int count = 0;

            int j;
            for (j = size - 1; data[j] == 0; --j)
                ; //search end of data

            if (data[j] == 0xd9) //thp file
                end = j - 1;
            else if (data[j] == 0xff) //mth file
                end = j - 2;

            for (int i = 0; i < end; ++i)
            {
                if (data[i] == 0xff)
                {
                    //if i == srcSize - 1, then this would normally overrun src - that's why 4 padding
                    //bytes are included at the end of src
                    if (data[i + 1] == 0xda && start == 2 * size)
                        start = i;
                    if (i > start)
                        ++count;
                }
            }
            return size + count;
        }

        static void convertToRealJpeg(byte[] dest, byte[] src, int srcSize, int start, int end)
        {
            int di = 0;
            for (int i = 0; i < srcSize; i++, di++)
            {
                dest[di] = src[i];
                //if i == srcSize - 1, then this would normally overrun src - that's why 4 padding
                //bytes are included at the end of src
                if (src[i] == 0xff && i > start && i < end)
                {
                    ++di;
                    dest[di] = 0;
                }
            }
        }
    }
}
