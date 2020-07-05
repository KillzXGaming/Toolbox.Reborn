using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toolbox.Core.Animations
{
    /// <summary>
    /// Represents a group that stores animation tracks and sub groups.
    /// </summary>
    public class STAnimGroup
    {
        /// <summary>
        /// The name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The category to place the group when displayed in the timeline.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// A list of sub groups
        /// </summary>
        public List<STAnimGroup> SubAnimGroups = new List<STAnimGroup>();

        /// <summary>
        /// Gets all the tracks used by this group for animation playback.
        /// </summary>
        /// <returns></returns>
        public virtual List<STAnimationTrack> GetTracks()
        {
            return new List<STAnimationTrack>();
        }

        public override string ToString()
        {
            return Name;
        }

        public Quaternion InterpolateLinearRotation(float frame, float frame1, float frame2, 
            Quaternion q1, Quaternion q2)
        {
            if (frame1 == frame)
                return q1;
            else if (frame2 == frame)
                return q2;
            else
                return Quaternion.Slerp(q1, q2, (frame - frame1) / (frame2 - frame1));
        }
    }
}
