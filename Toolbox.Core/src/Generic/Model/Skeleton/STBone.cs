using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a bone storing information to allow rendering, editing, and exporting from a skeleton
    /// </summary>
    public class STBone
    {
        /// <summary>
        /// Gets or sets the name of the bone.
        /// </summary>
        public string Name { get; set; }

        private STSkeleton Skeleton;

        private Matrix4 transform;

        /// <summary>
        /// Gets or sets the transformation of the bone.
        /// Setting this will adjust the 
        /// <see cref="Scale"/>, 
        /// <see cref="Rotation"/>, and 
        /// <see cref="Position"/> properties.
        /// </summary>
        public Matrix4 Transform
        {
            set
            {
                transform = value;
            }
            get
            {
                return transform;
            }
        }

        public Matrix4 Inverse { get; set; }

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
            get { return  STMath.ToEulerAngles(Rotation); }
            set { Rotation = STMath.FromEulerAngles(value); }
        }

        /// <summary>
        /// Gets or sets the parent bone. Returns null if unused.
        /// </summary>
        public STBone Parent;

        /// <summary>
        /// The list of children this bone is parenting to.
        /// </summary>
        public List<STBone> Children = new List<STBone>();

        /// <summary>
        /// Toggles the visibily of the bone when being rendered.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the parent bone index.
        /// </summary>
        public int ParentIndex
        {
            set
            {
                if (Parent != null) Parent.Children.Remove(this);
                if (value > -1 && value < Skeleton.Bones.Count) {
                    Skeleton.Bones[value].Children.Add(this);
                    Parent = Skeleton.Bones[value];
                }
            }
            get
            {
                if (Parent == null)
                    return -1;

                return Skeleton.Bones.IndexOf(Parent);
            }
        }

        public int Index
        {
            get { return Skeleton.Bones.IndexOf(this); }
        }

        /// <summary>
        /// The animation controller storing transformation data for 
        /// displayed animations.
        /// </summary>
        public STBoneAnimController AnimationController = new STBoneAnimController();

        public STBone(STSkeleton parentSkeleton) {
            Skeleton = parentSkeleton;
            Scale = Vector3.One;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        public STBone(STSkeleton parentSkeleton, string name) {
            Skeleton = parentSkeleton;
            Name = name;
            Scale = Vector3.One;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        /// <summary>
        /// Gets the transformation of the bone without it's parent transform applied.
        /// </summary>
        /// <returns></returns>
        public Matrix4 GetTransform()
        {
            return Matrix4.CreateScale(Scale) *
                   Matrix4.CreateFromQuaternion(Rotation) *
                   Matrix4.CreateTranslation(Position);
        }
    }
}
