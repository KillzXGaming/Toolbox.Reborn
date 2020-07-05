using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.Animations;

namespace Toolbox.Core
{
    public class VideoComponent
    {
        public List<VideoFrame> Frames = new List<VideoFrame>();

        public uint Width { get; set; }

        public uint Height { get; set; }

        public virtual VideoFrame GetFrame(int frame)
        {
           for (int i = 0; i < Frames.Count; i++) {
                if (Frames[i].Frame == frame)
                    return Frames[i];
            }
            return null;
        }
    }
}
