using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.IO;
using Toolbox.Core.Imaging;

namespace Toolbox.Core
{
    public class CTR_3DS
    {
        //From https://github.com/gdkchan/SPICA/blob/42c4181e198b0fd34f0a567345ee7e75b54cb58b/SPICA/PICA/Converters/TextureConverter.cs

        public enum Orientation
        {
            Default = 0,
            Rotate90 = 4,
            Transpose = 8,
        }

        public enum PICASurfaceFormat
        {
            RGBA8,
            RGB8,
            RGBA5551,
            RGB565,
            RGBA4,
            LA8,
            HiLo8,
            L8,
            A8,
            LA4,
            L4,
            A4,
            ETC1,
            ETC1A4
        }

        private static int[] FmtBPP = new int[] { 32, 24, 16, 16, 16, 16, 16, 8, 8, 8, 4, 4, 4, 8 };

        public static int[] SwizzleLUT =
{
             0,  1,  8,  9,  2,  3, 10, 11,
            16, 17, 24, 25, 18, 19, 26, 27,
             4,  5, 12, 13,  6,  7, 14, 15,
            20, 21, 28, 29, 22, 23, 30, 31,
            32, 33, 40, 41, 34, 35, 42, 43,
            48, 49, 56, 57, 50, 51, 58, 59,
            36, 37, 44, 45, 38, 39, 46, 47,
            52, 53, 60, 61, 54, 55, 62, 63
        };

        public static System.Drawing.Bitmap DecodeBlockToBitmap(byte[] Input, int Width, int Height, PICASurfaceFormat picaFormat)
        {
            return BitmapExtension.CreateBitmap(ImageUtility.ConvertBgraToRgba(DecodeBlock(Input, Width, Height, picaFormat)),
                  Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public static int Morton7(int value)
        {
            return ((value >> 2) & 0x04) | ((value >> 1) & 0x02) | (value & 0x01);
        }

        public static byte[] DecodeTiled(int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            int stride = width;

            for (int TY = 0; TY < height; TY += 8)
            {
                for (int TX = 0; TX < width; TX += 8)
                {
                    for (int i = 0; i < 0x40; i++)
                    {
                        int x = Morton7(i);
                        int y = Morton7(i >> 1);
                        int dstOffs = ((TY + y) * stride + TX + x) * 4;

                    }
                }
            }

            return output;
        }

        static byte Convert4To8(int value)
        {
            return (byte)((value << 4) | value);
        }

        public class SwizzleSettings
        {
            public Orientation Orientation = Orientation.Default;
        }

        public static byte[] DecodeBlock(byte[] Input, int Width, int Height, PICASurfaceFormat picaFormat, SwizzleSettings settings = null)
        {
            if (settings == null) settings = new SwizzleSettings();

            if (picaFormat == PICASurfaceFormat.ETC1 || picaFormat == PICASurfaceFormat.ETC1A4)
            {
                if (settings.Orientation == Orientation.Transpose)
                    return ETC1.ETC1Decompress(Input, Width, Height, picaFormat == PICASurfaceFormat.ETC1A4);
                else
                    return FlipVertical(Width, Height, ETC1.ETC1Decompress(Input, Width, Height, picaFormat == PICASurfaceFormat.ETC1A4));
            }

            byte[] Output = new byte[Width * Height * 4];

            int Increment = FmtBPP[(int)picaFormat] / 8;
            if (Increment == 0) Increment = 1;

            Console.WriteLine($"Increment {Increment} Input {Input.Length} {Width} {Height} {picaFormat}");

            int stride = Width;

            int IOffset = 0;
            for (int TY = 0; TY < Height; TY += 8)
            {
                for (int TX = 0; TX < Width; TX += 8)
                {
                    for (int Px = 0; Px < 64; Px++)
                    {
                        int X = Morton7(Px);
                        int Y = Morton7(Px >> 1);
                        int OOffet = ((TY + Y) * stride + TX + X) * 4;
                        if (OOffet + 4 >= Output.Length)
                            break;

                        DecodeFormat(Output, Input, OOffet, IOffset, picaFormat);
                        IOffset += Increment;
                    }
                }
            }

            int tile_width = (int)Math.Ceiling(Width / 8.0f);
            int tile_height = (int)Math.Ceiling(Height / 8.0f);

            stride = Width;
            IOffset = 0;
            for (int TY = 0; TY < tile_width; TY += 8) {
                for (int TX = 0; TX < tile_height; TX += 8) {
                    for (int x = 0; x < 2; x++) {
                        for (int y = 0; y < 2; y++) {
                            for (int x2 = 0; x2 < 2; x2++)
                            {
                                for (int y2 = 0; y2 < 2; y2++)
                                {
                                    for (int x3 = 0; x3< 2; x3++)
                                    {
                                        for (int y3 = 0; y3 < 2; y3++)
                                        {
                                            var pixel_x = (x3 + (x2 * 2) + (x * 4) + (TX * 8));
                                            var pixel_y = (y3 + (y2 * 2) + (y * 4) + (TY * 8));

                                            if (pixel_y >= Height || pixel_y >= Height)
                                                continue;
                                            // same for the x and the input data width
                                            if (pixel_x >= Width || pixel_x >= Width)
                                                continue;



                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (int Px = 0; Px < 64; Px++)
                    {
                        int X = Morton7(Px);
                        int Y = Morton7(Px >> 1);
                        int OOffet = ((TY + Y) * stride + TX + X) * 4;
                        if (OOffet + 4 >= Output.Length)
                            break;

                        DecodeFormat(Output, Input, OOffet, IOffset, picaFormat);
                        IOffset += Increment;
                    }
                }
            }


            if (settings.Orientation == Orientation.Transpose)
                return Output;
            else
                return FlipVertical(Width, Height, Output);
        }

        private static void DecodeFormat(byte[] Output, byte[] Input, int OOffet, int IOffset, PICASurfaceFormat picaFormat)
        {
            switch (picaFormat)
            {
                case PICASurfaceFormat.RGBA8:
                    Output[OOffet + 0] = Input[IOffset + 3];
                    Output[OOffet + 1] = Input[IOffset + 2];
                    Output[OOffet + 2] = Input[IOffset + 1];
                    Output[OOffet + 3] = Input[IOffset + 0];
                    break;
                case PICASurfaceFormat.RGB8:
                    Output[OOffet + 0] = Input[IOffset + 2];
                    Output[OOffet + 1] = Input[IOffset + 1];
                    Output[OOffet + 2] = Input[IOffset + 0];
                    Output[OOffet + 3] = 0xff;
                    break;
                case PICASurfaceFormat.RGBA5551:
                    DecodeRGBA5551(Output, OOffet, GetUShort(Input, IOffset));
                    break;
                case PICASurfaceFormat.RGB565:
                    DecodeRGB565(Output, OOffet, GetUShort(Input, IOffset));
                    break;
                case PICASurfaceFormat.RGBA4:
                    DecodeRGBA4(Output, OOffet, GetUShort(Input, IOffset));
                    break;
                case PICASurfaceFormat.LA8:
                    Output[OOffet + 0] = Input[IOffset + 1];
                    Output[OOffet + 1] = Input[IOffset + 1];
                    Output[OOffet + 2] = Input[IOffset + 1];
                    Output[OOffet + 3] = Input[IOffset + 0];
                    break;
                case PICASurfaceFormat.HiLo8:
                    Output[OOffet + 0] = Input[IOffset + 1];
                    Output[OOffet + 1] = Input[IOffset + 0];
                    Output[OOffet + 2] = 0;
                    Output[OOffet + 3] = 0xff;
                    break;
                case PICASurfaceFormat.L8:
                    Output[OOffet + 0] = Input[IOffset];
                    Output[OOffet + 1] = Input[IOffset];
                    Output[OOffet + 2] = Input[IOffset];
                    Output[OOffet + 3] = 0xff;
                    break;
                case PICASurfaceFormat.A8:
                    Output[OOffet + 0] = 0xff;
                    Output[OOffet + 1] = 0xff;
                    Output[OOffet + 2] = 0xff;
                    Output[OOffet + 3] = Input[IOffset];
                    break;
                case PICASurfaceFormat.LA4:
                    byte val = Convert4To8((Input[IOffset] & 0xF0) >> 4);
                    byte a = Convert4To8((Input[IOffset] & 0x0F));

                    Output[OOffet + 0] = val;
                    Output[OOffet + 1] = val;
                    Output[OOffet + 2] = val;
                    Output[OOffet + 3] = a;
                    break;
                case PICASurfaceFormat.L4:
                    int L = (Input[IOffset >> 1] >> ((IOffset & 1) << 2)) & 0xf;
                    Output[OOffet + 0] = (byte)((L << 4) | L);
                    Output[OOffet + 1] = (byte)((L << 4) | L);
                    Output[OOffet + 2] = (byte)((L << 4) | L);
                    Output[OOffet + 3] = 0xff;
                    break;
                case PICASurfaceFormat.A4:
                    int A = (Input[IOffset >> 1] >> ((IOffset & 1) << 2)) & 0xf;
                    Output[OOffet + 0] = 0xff;
                    Output[OOffet + 1] = 0xff;
                    Output[OOffet + 2] = 0xff;
                    Output[OOffet + 3] = (byte)((A << 4) | A);
                    break;
            }
        }

        private static byte[] FlipVertical(int Width, int Height, byte[] Input)
        {
            byte[] FlippedOutput = new byte[Width * Height * 4];

            int Stride = Width * 4;
            for (int Y = 0; Y < Height; Y++)
            {
                int IOffs = Stride * Y;
                int OOffs = Stride * (Height - 1 - Y);

                for (int X = 0; X < Width; X++)
                {
                    FlippedOutput[OOffs + 0] = Input[IOffs + 0];
                    FlippedOutput[OOffs + 1] = Input[IOffs + 1];
                    FlippedOutput[OOffs + 2] = Input[IOffs + 2];
                    FlippedOutput[OOffs + 3] = Input[IOffs + 3];

                    IOffs += 4;
                    OOffs += 4;
                }
            }
            return FlippedOutput;
        }

        //Much help from encoding thanks to this
        // https://github.com/Cruel/3dstex/blob/master/src/Encoder.cpp
        public static byte[] EncodeBlock(byte[] Input, int Width, int Height, PICASurfaceFormat PicaFormat)
        {
            int ImageSize = CalculateLength(Width, Height, PicaFormat);

            if (PicaFormat == PICASurfaceFormat.ETC1)
                return SmashForge.RG_ETC1.encodeETC(BitmapExtension.CreateBitmap(Input, Width, Height));
            else if (PicaFormat == PICASurfaceFormat.ETC1A4)
                return SmashForge.RG_ETC1.encodeETCa4(BitmapExtension.CreateBitmap(Input, Width, Height));

            var mem = new System.IO.MemoryStream();
            using (var writer = new FileWriter(mem))
            {
                for (int TY = 0; TY < Height; TY += 8)
                {
                    for (int TX = 0; TX < Width; TX += 8)
                    {
                        for (int Px = 0; Px < 64; Px++)
                        {
                            int X = SwizzleLUT[Px] & 7;
                            int Y = (SwizzleLUT[Px] - X) >> 3;

                            int IOffs = (TX + X + ((TY + Y) * Width)) * 4;

                            if (PicaFormat == PICASurfaceFormat.RGBA8)
                            {
                                writer.Write(Input[IOffs + 3]);
                                writer.Write(Input[IOffs + 0]);
                                writer.Write(Input[IOffs + 1]);
                                writer.Write(Input[IOffs + 2]);
                            }
                            else if (PicaFormat == PICASurfaceFormat.RGB8)
                            {
                                writer.Write(Input[IOffs + 0]);
                                writer.Write(Input[IOffs + 1]);
                                writer.Write(Input[IOffs + 2]);
                            }
                            else if (PicaFormat == PICASurfaceFormat.A8)
                            {
                                writer.Write(Input[IOffs]);
                            }
                            else if (PicaFormat == PICASurfaceFormat.L8)
                            {
                                writer.Write(ConvertBRG8ToL(
                                    new byte[]
                                    {
                                            Input[IOffs + 0],
                                            Input[IOffs + 1],
                                            Input[IOffs + 2]
                                    }));
                            }
                            else if (PicaFormat == PICASurfaceFormat.LA8)
                            {
                                writer.Write(Input[IOffs + 3]);
                                writer.Write(ConvertBRG8ToL(
                                    new byte[]
                                    {
                                            Input[IOffs + 0],
                                            Input[IOffs + 1],
                                            Input[IOffs + 2]
                                    }));
                            }
                            else if (PicaFormat == PICASurfaceFormat.RGB565)
                            {
                                ushort R = (ushort)(Convert8To5(Input[IOffs + 0]));
                                ushort G = (ushort)(Convert8To6(Input[IOffs + 1]) << 5);
                                ushort B = (ushort)(Convert8To5(Input[IOffs + 2]) << 11);

                                writer.Write((ushort)(R | G | B));
                            }
                            else if (PicaFormat == PICASurfaceFormat.RGBA4)
                            {
                                ushort R = (ushort)(Convert8To4(Input[IOffs]) << 4);
                                ushort G = (ushort)(Convert8To4(Input[IOffs + 1]) << 8);
                                ushort B = (ushort)(Convert8To4(Input[IOffs + 2]) << 12);
                                ushort A = (ushort)(Convert8To4(Input[IOffs + 3]));

                                writer.Write((ushort)(R | G | B | A));
                            }
                            else if (PicaFormat == PICASurfaceFormat.RGBA5551)
                            {
                                ushort R = (ushort)(Convert8To5(Input[IOffs + 0]) << 1);
                                ushort G = (ushort)(Convert8To5(Input[IOffs + 1]) << 6);
                                ushort B = (ushort)(Convert8To5(Input[IOffs + 2]) << 11);
                                ushort A = (ushort)(Convert8To1(Input[IOffs + 3]));

                                writer.Write((ushort)(R | G | B | A));
                            }
                            else if (PicaFormat == PICASurfaceFormat.LA4)
                            {
                                byte A = Input[IOffs + 3];
                                byte L = ConvertBRG8ToL(
                                    new byte[]
                                    {
                                            Input[IOffs + 0],
                                            Input[IOffs + 1],
                                            Input[IOffs + 2]
                                    });
                                writer.Write((byte)((A >> 4) | (L & 0xF0)));
                            }
                            else if (PicaFormat == PICASurfaceFormat.L4)
                            {
                                //Skip alpha channel
                                byte L1 = ConvertBRG8ToL(
                                    new byte[]
                                    {
                                            Input[IOffs + 0],
                                            Input[IOffs + 1],
                                            Input[IOffs + 2]
                                    });
                                byte L2 = ConvertBRG8ToL(
                                    new byte[]
                                    {
                                                Input[IOffs + 4],
                                                Input[IOffs + 5],
                                                Input[IOffs + 6]
                                    });

                                writer.Write((byte)((L1 >> 4) | (L2 & 0xF0)));
                                Px++;
                            }
                            else if (PicaFormat == PICASurfaceFormat.A4)
                            {
                                byte A1 = (byte)(Input[IOffs + 3] >> 4);
                                byte A2 = (byte)(Input[IOffs + 7] & 0xF0);
                                writer.Write((byte)(A1 | A2));
                                Px++;
                            }
                            else if (PicaFormat == PICASurfaceFormat.HiLo8)
                            {
                                writer.Write(Input[IOffs]);
                                writer.Write(Input[IOffs + 1]);
                            }
                        }
                    }
                }
            }

            byte[] newOutput = mem.ToArray();
            //    if (newOutput.Length != ImageSize)
            //       throw new Exception($"Invalid image size! Expected {ImageSize} got {newOutput.Length}");

            if (newOutput.Length > 0)
                return newOutput;
            else
                return new byte[CalculateLength(Width, Height, PicaFormat)];
        }

        // Convert helpers from Citra Emulator (citra/src/common/color.h)
        private static byte Convert8To1(byte val) { return (byte)(val == 0 ? 0 : 1); }
        private static byte Convert8To4(byte val) { return (byte)(val >> 4); }
        private static byte Convert8To5(byte val) { return (byte)(val >> 3); }
        private static byte Convert8To6(byte val) { return (byte)(val >> 2); }

        private static byte ConvertBRG8ToL(byte[] bytes)
        {
            byte L = (byte)(bytes[0] * 0.0722f);
            L += (byte)(bytes[1] * 0.7152f);
            L += (byte)(bytes[2] * 0.2126f);

            return L;
        }

        private static void DecodeRGB565(byte[] Buffer, int Address, ushort Value)
        {
            int R = ((Value >> 0) & 0x1f) << 3;
            int G = ((Value >> 5) & 0x3f) << 2;
            int B = ((Value >> 11) & 0x1f) << 3;

            SetColor(Buffer, Address, 0xff,
                B | (B >> 5),
                G | (G >> 6),
                R | (R >> 5));
        }

        private static void DecodeRGBA4(byte[] Buffer, int Address, ushort Value)
        {
            int R = (Value >> 4) & 0xf;
            int G = (Value >> 8) & 0xf;
            int B = (Value >> 12) & 0xf;

            SetColor(Buffer, Address, (Value & 0xf) | (Value << 4),
                B | (B << 4),
                G | (G << 4),
                R | (R << 4));
        }

        private static void DecodeRGBA5551(byte[] Buffer, int Address, ushort Value)
        {
            int R = ((Value >> 1) & 0x1f) << 3;
            int G = ((Value >> 6) & 0x1f) << 3;
            int B = ((Value >> 11) & 0x1f) << 3;

            SetColor(Buffer, Address, (Value & 1) * 0xff,
                B | (B >> 5),
                G | (G >> 5),
                R | (R >> 5));
        }

        private static void SetColor(byte[] Buffer, int Address, int A, int B, int G, int R)
        {
            Buffer[Address + 0] = (byte)B;
            Buffer[Address + 1] = (byte)G;
            Buffer[Address + 2] = (byte)R;
            Buffer[Address + 3] = (byte)A;
        }

        private static ushort GetUShort(byte[] Buffer, int Address)
        {
            return (ushort)(
                Buffer[Address + 0] << 0 |
                Buffer[Address + 1] << 8);
        }

        public static int CalculateLength(int Width, int Height, PICASurfaceFormat Format)
        {
            int Length = (Width * Height * FmtBPP[(int)Format]) / 8;

            if ((Length & 0x7f) != 0)
            {
                Length = (Length & ~0x7f) + 0x80;
            }

            return Length;
        }

    }
}
