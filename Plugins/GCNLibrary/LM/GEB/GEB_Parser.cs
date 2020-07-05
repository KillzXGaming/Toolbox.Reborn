using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.LM
{
    public class GEB_Parser
    {
        public Sprite[] Sprites { get; set; }

        public GEB_Parser() { }

        public GEB_Parser(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            uint count = reader.ReadUInt32();
            Sprites = new Sprite[count];
            for (int i = 0; i < count; i++)
                Sprites[i] = new Sprite(reader);
        }

        private void Write(FileWriter writer) {
            writer.SetByteOrder(true);
            writer.Write(Sprites.Length);
            for (int i = 0; i < Sprites.Length; i++)
                Sprites[i].Write(writer);
        }

        public class Sprite
        {
            public short FadeSpriteBoneIndex { get; set; }
            public short GlowSpriteBoneIndex { get; set; }

            public ColorRGB Color { get; set; }

            public Vector2[] Points { get; set; }
            public Vector2[] TexCoords { get; set; }

            public float FadeIntensityGlowSprite { get; set; }
            public Vector2 RelativePosition { get; set; }

            public Sprite()
            {
                TexCoords = new Vector2[4];
                Points = new Vector2[4];
            }

            public Sprite(FileReader reader)
            {
                FadeSpriteBoneIndex = reader.ReadInt16();
                GlowSpriteBoneIndex = reader.ReadInt16();
                Color = new ColorRGB(
                    reader.ReadByte(), reader.ReadByte(),
                    reader.ReadByte(), reader.ReadByte());
                Points = new Vector2[4];
                for (int i = 0; i < Points.Length; i++)
                {
                    Points[i] = ReadVec2(reader);
                    reader.ReadUInt32();//0
                }
                TexCoords = new Vector2[4];
                for (int i = 0; i < TexCoords.Length; i++)
                    TexCoords[i] = ReadVec2(reader);
                FadeIntensityGlowSprite = reader.ReadSingle();
                RelativePosition = ReadVec2(reader);
            }

            private Vector2 ReadVec2(FileReader reader)
            {
                return new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }

            public void Write(FileWriter writer)
            {
                writer.Write(FadeSpriteBoneIndex);
                writer.Write(GlowSpriteBoneIndex);
                writer.Write(Color.R);
                writer.Write(Color.G);
                writer.Write(Color.B);
                writer.Write(Color.A);
                for (int i = 0; i < Points.Length; i++) {
                    writer.Write(Points[i].X);
                    writer.Write(Points[i].Y);
                    writer.Write(0);
                }
                for (int i = 0; i < TexCoords.Length; i++)
                {
                    writer.Write(TexCoords[i].X);
                    writer.Write(TexCoords[i].Y);
                }
                writer.Write(FadeIntensityGlowSprite);
                writer.Write(RelativePosition.X);
                writer.Write(RelativePosition.Y);
            }
        }


        public class ColorRGB
        {
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }
            public byte A { get; set; }

            public ColorRGB() { }

            public ColorRGB(byte r, byte g, byte b, byte a)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }
        }

        public class Vector2
        {
            public float X { get; set; }
            public float Y { get; set; }

            public Vector2() { }

            public Vector2(float x, float y) {
                X = x;
                Y = y;
            }
        }
    }
}
