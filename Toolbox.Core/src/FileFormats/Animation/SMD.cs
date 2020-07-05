using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Toolbox.Core.Animations
{
    public class SMD : IExportableSkeletalAnimation
    {
        public void Export(STAnimation animation, STSkeleton skeleton, string filePath)
        {
            if (skeleton == null) return;

            using (StreamWriter writer = new StreamWriter(new FileStream(filePath, FileMode.Create)))
            {
                writer.WriteLine("version 1");
                writer.WriteLine("nodes");
                foreach (STBone bone in skeleton.Bones)
                    writer.WriteLine($" {skeleton.Bones.IndexOf(bone)} \"{bone.Name}\" {bone.ParentIndex}");

                writer.WriteLine("end");
                writer.WriteLine("skeleton");

                animation.Frame = 0;
                for (int i = 0; i < animation.FrameCount; i++) {
                    animation.UpdateFrame(i);
                    writer.WriteLine($"time {animation.StartFrame + i}");
                    foreach (STBone bone in skeleton.Bones)
                    {
                        var controller = bone.AnimationController;
                        writer.WriteLine($" {skeleton.Bones.IndexOf(bone)}" +
                            $"{controller.Position.X} {controller.Position.Y} {controller.Position.Z} " +
                            $"{controller.EulerRotation.X} {controller.EulerRotation.Y} {controller.EulerRotation.Z}");
                    }
                }
                writer.WriteLine("end");
            }
        }
    }
}
