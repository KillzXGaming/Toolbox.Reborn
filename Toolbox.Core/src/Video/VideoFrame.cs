using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public abstract class VideoFrame
    {
        public int Frame { get; set; }

        public VideoFrame(int frame) {
            Frame = frame;
        }

        public bool FlipImage { get; set; }

        public abstract byte[] GetImageData();
    }
}
