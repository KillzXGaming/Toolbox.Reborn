using System;
using System.Collections.Generic;
using System.Drawing;
using Toolbox.Core.Imaging;

namespace Toolbox.Core.Hashes
{
    public class ChecksumQuick
    {
        public static unsafe uint GetChecksum(Bitmap image) {
            return GetChecksum(BitmapExtension.ImageToByte(image));
        }

        public static unsafe uint GetChecksum(byte[] array)
        {
            unchecked
            {
                uint checksum = 0;
                fixed (byte* arrayBase = array)
                {
                    byte* arrayPointer = arrayBase;
                    for (int i = array.Length - 1; i >= 0; i--)
                    {
                        checksum += *arrayPointer;
                        arrayPointer++;
                    }
                }
                return checksum;
            }
        }
    }
}
