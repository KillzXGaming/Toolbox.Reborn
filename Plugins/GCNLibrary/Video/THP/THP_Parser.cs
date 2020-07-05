using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using Toolbox.Core;
using Toolbox.Core.Imaging;
using Toolbox.Core.IO;
using System.Runtime.InteropServices;

namespace GCNLibrary
{
    public class THP_Parser
    {
        public Header FileHeader;

        public VideoHeader Video;
        public AudioHeader Audio;

        public List<FrameHeader> Frames = new List<FrameHeader>();

        public THP_Parser(Stream stream)
        {
            using (var reader = new FileReader(stream, true))
            {
                reader.SetByteOrder(true);
                FileHeader = reader.ReadStruct<Header>();

                bool isVersion11 = FileHeader.Version == 0x00011000;

                reader.SeekBegin(FileHeader.ComponentsOffset);

                uint numComponents = reader.ReadUInt32();
                byte[] components = reader.ReadBytes(16);
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == (byte)ComponentType.Video)
                        Video = new VideoHeader(reader, isVersion11);
                    if (components[i] == (byte)ComponentType.Audio)
                        Audio = new AudioHeader(reader, isVersion11);
                }
                bool hasVideo = components.Any(x => x == (byte)ComponentType.Video);
                bool hasAudio = components.Any(x => x == (byte)ComponentType.Audio);

                reader.SeekBegin(FileHeader.FirstFrameOffset);
                for (int i = 0; i < FileHeader.FrameCount; i++) {
                    long startFrame = reader.Position;

                    var frame = new FrameHeader(reader, hasAudio ? Audio.NumChannels : 0);
                    Frames.Add(frame);

                    if (i == 0)
                        reader.SeekBegin(startFrame + FileHeader.FirstFrameLength);
                    else
                        reader.SeekBegin(startFrame + Frames[i - 1].NextFrameSize);
                }
            }
        }

        public void Save(Stream stream)
        {

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public uint Magic;
            public uint Version;
            public uint MaxBufferSize;
            public uint MaxAudioSamples;
            public float FramesPerSecond;
            public uint FrameCount;
            public uint FirstFrameLength;
            public uint TotalFrameLength;
            public uint ComponentsOffset;
            public uint Unknown;
            public uint FirstFrameOffset;
            public uint LastFrameOffset;
        }

        public class VideoHeader
        {
            public uint Width;
            public uint Height;
            public VideoFormat Format; //only used in version 0x11000

            public VideoHeader(FileReader reader, bool version11)
            {
                Width = reader.ReadUInt32();
                Height = reader.ReadUInt32();
                if (version11)
                    Format = (VideoFormat)reader.ReadUInt32();
            }
        }

        public class AudioHeader
        {
            public uint NumChannels;
            public uint Frequentie;
            public uint NumSamples;
            public uint NumData; //only used in version 0x11000

            public AudioHeader(FileReader reader, bool version11)
            {
                NumChannels = reader.ReadUInt32();
                Frequentie = reader.ReadUInt32();
                NumSamples = reader.ReadUInt32();
                if (version11)
                    NumData = reader.ReadUInt32();
            }
        }

        public class FrameHeader
        {
            public uint NextFrameSize;
            public uint PrevFrameSize;
            public uint ImageSize;
            public uint AudioSize;

            public Stream ImageData { get; set; }

            public AudioData[] AudioData { get; set; }

            public FrameHeader(FileReader reader, uint numAudioChannels) {
                long pos = reader.Position;
                NextFrameSize = reader.ReadUInt32();
                PrevFrameSize = reader.ReadUInt32();
                ImageSize = reader.ReadUInt32();
                if (numAudioChannels != 0)
                    AudioSize = reader.ReadUInt32();

                ImageData = new SubStream(reader.BaseStream, reader.Position, ImageSize);
                if (AudioSize != 0)
                {
                    AudioData = new AudioData[1];
                    for (int i = 0; i < 1; i++)
                        AudioData[i] =  new AudioData(reader, AudioSize - 80);
                }
            }
        }

        public class AudioData
        {
            public uint ChannelSize;
            public uint NumSamples;
            public short[] Table1;
            public short[] Table2;
            public short Channel1;
            public short Channel2;
            public short Channel3;
            public short Channel4;

            public Stream Data { get; set; }

            public AudioData(FileReader reader, uint dataSize)
            {
                ChannelSize = reader.ReadUInt32();
                NumSamples = reader.ReadUInt32();
                Table1 = reader.ReadInt16s(16);
                Table2 = reader.ReadInt16s(16);
                Channel1 = reader.ReadInt16();
                Channel2 = reader.ReadInt16();
                Channel3 = reader.ReadInt16();
                Channel4 = reader.ReadInt16();
                Data = new SubStream(reader.BaseStream, reader.Position, dataSize);
            }
        }

        public enum ComponentType
        {
            None = -1,
            Video = 0,
            Audio = 1,
        }

        public enum VideoFormat : uint
        {
            NoInterlace,
            OddInterlace,
            EvenInterlace,
        }
    }
}
