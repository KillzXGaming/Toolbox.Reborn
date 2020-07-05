using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;
using Toolbox.Core;
using Toolbox.Core.Imaging;

namespace GCNLibrary
{
    public class THP : IFileFormat, IVideoFormat, IDisposable
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "THP Video" };
        public string[] Extension { get; set; } = new string[] { "*.thp" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(3, "THP");
            }
        }

        private VideoComponent videoData = new VideoComponent();
        private AudioComponent audioData = new AudioComponent();

        public VideoComponent VideoData => videoData;
        public AudioComponent AudioData => audioData;

        public float FrameRate => Header.FileHeader.FramesPerSecond;
        public int FrameCount => Header.Frames.Count;

        public THP_Parser Header;

        private Stream _stream;

        public void Load(Stream stream) {
            FileInfo.KeepOpen = true;
            _stream = stream;
            Header = new THP_Parser(stream);
            foreach (var frame in Header.Frames)
            {
                var tex = new FrameImage(videoData.Frames.Count);
                tex.Frame = frame;
                videoData.Width = Header.Video.Width;
                videoData.Height = Header.Video.Height;
                videoData.Frames.Add(tex);
            }
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public void Dispose() {
            _stream?.Close();
            _stream = null;
        }

        public class FrameImage : VideoFrame
        {
            public FrameImage(int frame) : base(frame) { }

            public THP_Parser.FrameHeader Frame;

            public byte[] Data
            {
                get
                {
                    using (var reader = new FileReader(Frame.ImageData, true)) {
                        return reader.ReadBytes((int)reader.BaseStream.Length);
                    }
                }
            }

            public override byte[] GetImageData() {
                return JpegUtility.DecodeTHPJpeg(Data);
            }
        }
    }
}
