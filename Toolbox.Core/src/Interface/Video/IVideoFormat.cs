using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface IVideoFormat
    {
        /// <summary>
        /// The graphic data for this video
        /// </summary>
        VideoComponent VideoData { get; }

        /// <summary>
        /// The audio data for this video
        /// </summary>
        AudioComponent AudioData { get; }

        int FrameCount { get; }

        /// <summary>
        /// The rate of the frame playback.
        /// </summary>
        float FrameRate { get; }
    }
}
