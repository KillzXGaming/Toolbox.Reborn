using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Core.Animations
{
    /// <summary>
    /// Represents a animation that uses bones to animate given a skeleton.
    /// </summary>
    public class STSkeletonAnimation : STAnimation
    {
        /// <summary>
        /// Gets the active skeleton visbile in the scene that may be used for animation.
        /// </summary>
        /// <returns></returns>
        public virtual STSkeleton GetActiveSkeleton()
        {
            foreach (var container in Runtime.ModelContainers)
            {
                var skel = container.SearchActiveSkeleton();
                if (skel != null)
                    return skel;
            }
            return null;
        }

        public override void NextFrame()
        {
            base.NextFrame();

            bool update = false;
            var skeleton = GetActiveSkeleton();

            if (skeleton == null) return;

            foreach (var group in AnimGroups) {
                if (group is STBoneAnimGroup)
                {
                    var boneAnim = (STBoneAnimGroup)group;
                    var bone = skeleton.SearchBone(boneAnim.Name);

                    if (bone == null)
                        continue;

                    update = true;

                    Vector3 position = bone.Position;
                    Vector3 scale = bone.Scale;

                    if (boneAnim.TranslateX.HasKeys)
                        position.X = boneAnim.TranslateX.GetFrameValue(Frame);
                    if (boneAnim.TranslateY.HasKeys)
                        position.Y = boneAnim.TranslateY.GetFrameValue(Frame);
                    if (boneAnim.TranslateZ.HasKeys)
                        position.Z = boneAnim.TranslateZ.GetFrameValue(Frame);

                    if (boneAnim.ScaleX.HasKeys)
                        scale.X = boneAnim.ScaleX.GetFrameValue(Frame);
                    if (boneAnim.ScaleY.HasKeys)
                        scale.Y = boneAnim.ScaleY.GetFrameValue(Frame);
                    if (boneAnim.ScaleZ.HasKeys)
                        scale.Z = boneAnim.ScaleZ.GetFrameValue(Frame);

                    bone.AnimationController.Position = position;
                    bone.AnimationController.Scale = scale;

                    if (boneAnim.UseQuaternion)
                    {
                        Quaternion rotation = bone.Rotation;

                        if (boneAnim.RotateX.HasKeys)
                            rotation.X = boneAnim.RotateX.GetFrameValue(Frame);
                        if (boneAnim.RotateY.HasKeys)
                            rotation.Y = boneAnim.RotateY.GetFrameValue(Frame);
                        if (boneAnim.RotateZ.HasKeys)
                            rotation.Z = boneAnim.RotateZ.GetFrameValue(Frame);
                        if (boneAnim.RotateW.HasKeys)
                            rotation.W = boneAnim.RotateW.GetFrameValue(Frame);

                        bone.AnimationController.Rotation = rotation;
                    }
                    else
                    {
                        Vector3 rotationEuluer = bone.EulerRotation;

                        if (boneAnim.RotateX.HasKeys)
                            rotationEuluer.X = boneAnim.RotateX.GetFrameValue(Frame);
                        if (boneAnim.RotateY.HasKeys)
                            rotationEuluer.Y = boneAnim.RotateY.GetFrameValue(Frame);
                        if (boneAnim.RotateZ.HasKeys)
                            rotationEuluer.Z = boneAnim.RotateZ.GetFrameValue(Frame);

                        bone.AnimationController.EulerRotation = rotationEuluer;
                    }
                }
            }

            if (update) {
                skeleton.Update();
            }
        }
    }
}
