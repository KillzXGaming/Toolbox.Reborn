using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.Animations
{
    /// <summary>
    /// Represents how a track will be interpolated between key frames
    /// </summary>
    public enum STInterpoaltionType
    {
        ///<summary>Stays at a single value at all times.</summary>
        Constant,
        ///<summary>Value changes only when a keyframe is reached, no interpoaltion used.</summary>
        Step,
        Linear,
        Hermite,
        Bezier,
        Bitmap,
    }

    /// <summary>
    /// Represents how a track is played after it reaches end frame.
    /// </summary>
    public enum STLoopMode
    {
        ///<summary>Repeats back from the start.</summary>
        Repeat,
        ///<summary>goes from the end frame to the start.</summary>
        Mirror,
        ///<summary>Stays at the very last frame when the current frame is higher that the end frame.</summary>
        Clamp,
    }
}
