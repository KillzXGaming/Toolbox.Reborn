using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a transform used for mesh movement in the picking engine.
    /// </summary>
    public class ModelTransform
    {
        /// <summary>
        /// Gets or sets the position of the model in world space.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the scale of the model in world space.
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the model in world space.
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

        private Matrix4 matrix = Matrix4.Identity;
        public Matrix4 Matrix
        {
            get
            {
                Update();
                return matrix;
            }
        }

        public ModelTransform() { Reset(); }

        public void Update() {
            matrix = GetTransform();
        }

        public void Reset()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        private Matrix4 GetTransform()
        {
            return Matrix4.CreateScale(Scale) *
                   Matrix4.CreateFromQuaternion(Rotation) *
                   Matrix4.CreateTranslation(Position);
        }
    }
}
