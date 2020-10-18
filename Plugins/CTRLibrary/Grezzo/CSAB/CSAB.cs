using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.OpenGL;
using Toolbox.Core.Animations;
using Toolbox.Core.ModelView;
using OpenTK;

namespace CTRLibrary.Grezzo
{
    public class CSAB : STAnimation, IFileFormat
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "CTR Skeletal Animation Binary" };
        public string[] Extension { get; set; } = new string[] { "*.csab" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "csab");
            }
        }

        public CSAB_Parser Header;

        public void Load(Stream stream)
        {
            this.Name = FileInfo.FileName;
            Header = new CSAB_Parser(stream);

            FrameCount = Header.FrameCount;
            foreach (var joinAnim in Header.AnimGroups)
                AnimGroups.Add(joinAnim);
        }

        public void Save(Stream stream)
        {

        }


        public override void NextFrame()
        {
            STSkeleton skeleton = null;
            Console.WriteLine($"Models {Runtime.ModelContainers.Count}");
            foreach (var container in Runtime.ModelContainers)
            {
                var skel = container.SearchActiveSkeleton();
                if (skel != null)
                    skeleton = skel;
            }

            Console.WriteLine($"skel {skeleton != null} AnimGroups {AnimGroups.Count}");

            if (skeleton == null) return;

            bool Updated = false;
            for (int i = 0; i < AnimGroups.Count; i++)
            {
                CSAB_Parser.AnimationNode group = (CSAB_Parser.AnimationNode)AnimGroups[i];
                var joint = skeleton.Bones[group.BoneIndex];

                Updated = true;

                var position = joint.Position;
                var scale = joint.Scale;
                var rotate = joint.EulerRotation;

                if (group.TranslateX.HasKeys)
                    position.X = group.TranslateX.GetFrameValue(Frame);
                if (group.TranslateY.HasKeys)
                    position.Y = group.TranslateY.GetFrameValue(Frame);
                if (group.TranslateZ.HasKeys)
                    position.Z = group.TranslateZ.GetFrameValue(Frame);

                if (group.RotationX.HasKeys)
                    rotate.X = group.RotationX.GetFrameValue(Frame);
                if (group.RotationY.HasKeys)
                    rotate.Y = group.RotationY.GetFrameValue(Frame);
                if (group.RotationZ.HasKeys)
                    rotate.Z = group.RotationZ.GetFrameValue(Frame);

                if (group.ScaleX.HasKeys)
                    scale.X = group.ScaleX.GetFrameValue(Frame);
                if (group.ScaleY.HasKeys)
                    scale.Y = group.ScaleY.GetFrameValue(Frame);
                if (group.ScaleZ.HasKeys)
                    scale.Z = group.ScaleZ.GetFrameValue(Frame);

                joint.AnimationController.Position = position;
                joint.AnimationController.Scale = scale;
                joint.AnimationController.EulerRotation = rotate;
            }

            if (Updated)
            {
                skeleton.Update();
            }
        }
    }
}
