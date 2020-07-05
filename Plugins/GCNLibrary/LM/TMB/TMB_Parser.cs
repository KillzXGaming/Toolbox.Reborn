using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.LM
{
    //Thanks to this, ported over
    //https://github.com/opeyx/Dolhouse/blob/master/Dolhouse/Dolhouse/Engine/TMB.cs

    /// <summary>
    /// Represents a file that controls timing of fade effects.
    /// </summary>
    public class TMB_Parser
    {
        /// <summary>
        /// The amount of frames in the animation.
        /// </summary>
        public ushort Duration { get; set; }

        /// <summary>
        /// The sequences used to control the animation.
        /// </summary>
        public List<Sequence> Sequences = new List<Sequence>();

        public TMB_Parser() { }

        public TMB_Parser(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            ushort sequenceCount = reader.ReadUInt16();
            Duration = reader.ReadUInt16();
            for (int i = 0; i < sequenceCount; i++)
                Sequences.Add(new Sequence(reader));
        }

        private void Write(FileWriter writer)
        {
            writer.SetByteOrder(true);
            writer.Write((ushort)Sequences.Count);
            writer.Write((ushort)Duration);
            writer.Write(uint.MaxValue); //sequence offset

            //Save key data
            List<float> buffer = new List<float>();
            for (int i = 0; i < Sequences.Count; i++) {
                SetKeyGroupDataF32(buffer, Sequences[i]);
            }
            writer.Write(buffer.ToArray());
            buffer.Clear();

            writer.WriteUint32Offset(0x4);
            for (int i = 0; i < Sequences.Count; i++)
                Sequences[i].Write(writer);
        }

        private void SetKeyGroupDataF32(List<float> buffer, Sequence sequence)
        {
            var values = GetKeyGroupDataF32(sequence);
            if (values.Length == 1 && buffer.Contains(values[0]))
            {
                sequence.BeginIndex = (ushort)buffer.IndexOf(values[0]);
            }
            else
            {
                sequence.BeginIndex = (ushort)buffer.Count;
                buffer.AddRange(values);
            }
        }

        private float[] GetKeyGroupDataF32(Sequence sequence)
        {
            List<float> values = new List<float>();
            for (int i = 0; i < sequence.KeyFrames.Length; i++) {
                values.Add(sequence.KeyFrames[i].Frame);
                values.AddRange(sequence.KeyFrames[i].Values);
            }
            return values.ToArray();
        }

        public class Sequence
        {
            /// <summary>
            /// Sequence Name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Float data start index.
            /// </summary>
            public ushort BeginIndex { get; set; }

            /// <summary>
            /// List of keyframes within this sequence.
            /// </summary>
            public KeyFrame[] KeyFrames { get; set; }

            public Sequence(FileReader reader) {
                Name = reader.ReadString(28);
                uint keyFrameCount = reader.ReadUInt32();
                BeginIndex = reader.ReadUInt16();
                ushort elementCount = reader.ReadUInt16();

                //Data starts 8 bytes, stride of 4 (size of float)
                using (reader.TemporarySeek(8 + (BeginIndex * 4), SeekOrigin.Begin))
                {
                    KeyFrames = new KeyFrame[keyFrameCount];
                    for (int i = 0; i < keyFrameCount; i++)
                    {
                        KeyFrames[i] = new KeyFrame()
                        {
                            Frame = reader.ReadSingle(),
                            Values = reader.ReadSingles(elementCount - 1),
                        };
                    }
                }
            }

            public void Write(FileWriter writer)
            {
                writer.WriteString(Name, 28);
                writer.Write(KeyFrames.Length);
                writer.Write(BeginIndex);
                writer.Write((ushort)KeyFrames.FirstOrDefault().Values.Length + 1);
            }
        }

        public class KeyFrame
        {
            public float Frame { get; set; }

            public float[] Values { get; set; }
        }
    }
}
