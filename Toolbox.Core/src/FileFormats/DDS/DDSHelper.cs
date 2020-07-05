using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;

namespace Toolbox.Core
{
    public class DDSHelper
    {
        public static List<STGenericTexture.Surface> GetArrayFaces(DDS dds, uint Length, int DepthLevel = 0)
        {
            using (FileReader reader = new FileReader(dds.ImageData))
            {
                var format = dds.Platform.OutputFormat;

                var Surfaces = new List<STGenericTexture.Surface>();
                uint formatSize = TextureFormatHelper.GetBytesPerPixel(format);

                bool isBlock = TextureFormatHelper.IsBCNCompressed(format);
                uint Offset = 0;

                if (dds.Depth > 1 && dds.MipCount > 1)
                {
                    var Surface = new STGenericTexture.Surface();

                    uint MipWidth = dds.Width, MipHeight = dds.Height;
                    for (int j = 0; j < dds.MipCount; ++j)
                    {
                        MipWidth = (uint)Math.Max(1, dds.Width >> j);
                        MipHeight = (uint)Math.Max(1, dds.Height >> j);
                        for (byte d = 0; d < dds.Depth; ++d)
                        {
                            uint size = (MipWidth * MipHeight); //Total pixels
                            if (isBlock)
                            {
                                size = ((MipWidth + 3) >> 2) * ((MipHeight + 3) >> 2) * formatSize;
                                if (size < formatSize)
                                    size = formatSize;
                            }
                            else
                            {
                                size = (uint)(size * (TextureFormatHelper.GetBytesPerPixel(format))); //Bytes per pixel
                            }


                            //Only add mips to the depth level needed
                            if (d == DepthLevel)
                                Surface.mipmaps.Add(reader.getSection((int)Offset, (int)size));

                            Offset += size;

                            //Add the current depth level and only once
                            if (d == DepthLevel && j == 0)
                                Surfaces.Add(Surface);
                        }
                    }
                }
                else
                {
                    for (byte d = 0; d < dds.Depth; ++d)
                    {
                        for (byte i = 0; i < Length; ++i)
                        {
                            var Surface = new STGenericTexture.Surface();

                            uint MipWidth = dds.Width, MipHeight = dds.Height;
                            for (int j = 0; j < dds.MipCount; ++j)
                            {
                                MipWidth = (uint)Math.Max(1, dds.Width >> j);
                                MipHeight = (uint)Math.Max(1, dds.Height >> j);

                                uint size = (MipWidth * MipHeight); //Total pixels
                                if (isBlock)
                                {
                                    size = ((MipWidth + 3) >> 2) * ((MipHeight + 3) >> 2) * formatSize;
                                    if (size < formatSize)
                                        size = formatSize;
                                }
                                else
                                {
                                    size = (uint)(size * (TextureFormatHelper.GetBytesPerPixel(format))); //Bytes per pixel
                                }

                                Surface.mipmaps.Add(reader.getSection((int)Offset, (int)size));
                                Offset += size;
                            }

                            if (d == DepthLevel)
                                Surfaces.Add(Surface);
                        }
                    }
                }

                return Surfaces;
            }
        }
    }
}
