using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Core.Animations
{
    public class STKeyFrame
    {
        /// <summary>
        /// The frame of the key.
        /// </summary>
        public virtual float Frame { get; set; }

        /// <summary>
        /// The value of the key
        /// </summary>
        public virtual float Value { get; set; }

        /// <summary>
        /// The slope used for the key used for interpolation.
        /// </summary>
        public virtual float Slope { get; set; }

        public STKeyFrame() { }

        public STKeyFrame(int frame, float value)
        {
            Frame = frame;
            Value = value;
        }

        public STKeyFrame(float frame, float value)
        {
            Frame = frame;
            Value = value;
        }
    }

    /// <summary>
    /// Represents a bezier key frame used for beizer interpolation
    /// </summary>
    public class STBezierKeyFrame : STKeyFrame
    {
        public float SlopeIn;
        public float SlopeOut;
    }

    /// <summary>
    /// Represents a hermite cubic key frame used for hermite cubic interpolation
    /// </summary>
    public class STHermiteCubicKeyFrame : STHermiteKeyFrame
    {
        public float Coef0 { get; set; }
        public float Coef1 { get; set; }
        public float Coef2 { get; set; }
        public float Coef3 { get; set; }
    }

    /// <summary>
    /// Represents a hermite key frame used for hermite interpolation
    /// </summary>
    public class STHermiteKeyFrame : STKeyFrame
    {
        public virtual float TangentIn { get; set; }
        public virtual float TangentOut { get; set; }
    }
}
