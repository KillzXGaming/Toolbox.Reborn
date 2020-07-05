using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents an animation controller for a <see cref="STBone"/>.
    /// </summary>
    public class STBoneAnimController
    {
        /// <summary>
        /// Gets or sets the position of the bone in world space.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the scale of the bone in world space.
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the bone in world space.
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rotation"/> using euler method. 
        /// </summary>
        public Vector3 EulerRotation
        {
            get { return STMath.ToEulerAngles(Rotation); }
            set { Rotation = STMath.FromEulerAngles(value); }
        }

        public bool WorldTransform { get; set; } = false;
    }
}
