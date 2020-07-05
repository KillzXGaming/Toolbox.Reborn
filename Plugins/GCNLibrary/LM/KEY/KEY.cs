using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.OpenGL;
using Toolbox.Core.Animations;
using Toolbox.Core.ModelView;
using OpenTK;

namespace GCNLibrary.LM
{
    public class KEY : STAnimation, IFileFormat, IConvertableTextFormat
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "LM Skeletal Animation" };
        public string[] Extension { get; set; } = new string[] { "*.key" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".key";
        }

        public TextFileType TextFileType => TextFileType.Yaml;
        public bool CanConvertBack => true;

        public string ConvertToString() {
            return ToText(Header);
        }

        public void ConvertFromString(string text) {
            Header = FromText(text);
        }

        public KEY_Parser Header;

        public void Load(Stream stream)
        {
            Header = new KEY_Parser(stream);
            this.Name = FileInfo.FileName;
            this.Loop = Header.Flags == 2;

            FrameCount = Header.FrameCount;
            Name = FileInfo.FileName;
            foreach (var joinAnim in Header.AnimJoints) {
                STAnimGroup group = new AnimGroup(joinAnim);
                AnimGroups.Add(group);
            }
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public string ToText(KEY_Parser header)
        {
            StringBuilder sb = new StringBuilder();
            using (var writer = new StringWriter(sb)) {
                writer.WriteLine($"FrameCount: {header.FrameCount}");
                writer.WriteLine($"FrameDelay: {header.AnimationDelay}");
                writer.WriteLine($"Flags: {header.Flags}");
                for (int i = 0; i < Header.AnimJoints.Length; i++)
                {
                    var joint = Header.AnimJoints[i];
                    writer.WriteLine($"- Joint: {i}");
                    WriteGroupText(writer, joint.ScaleX, "Scale X");
                    WriteGroupText(writer, joint.ScaleY, "Scale Y");
                    WriteGroupText(writer, joint.ScaleZ, "Scale Z");
                    WriteGroupText(writer, joint.RotateX, "Rotate X");
                    WriteGroupText(writer, joint.RotateY, "Rotate Y");
                    WriteGroupText(writer, joint.RotateZ, "Rotate Z");
                    WriteGroupText(writer, joint.PositionX, "Position X");
                    WriteGroupText(writer, joint.PositionY, "Position Y");
                    WriteGroupText(writer, joint.PositionZ, "Position Z");
                }
            }
            return sb.ToString();
        }

        private void WriteGroupText(StringWriter writer, KEY_Parser.Group group, string Name)
        {
            writer.WriteLine($"    {Name}:");
            for (int i = 0; i < group.KeyFrames.Count; i++)
            {
                string slopeInfo = "";
                if (group.KeyFrames.Count > 1)
                    slopeInfo = $", InSlope: {group.KeyFrames[i].InSlope}";
                if (group.SlopeFlag > 0)
                    slopeInfo = $", {slopeInfo}, OutSlope: {group.KeyFrames[i].OutSlope}";

                writer.WriteLine($"      - Frame: [{group.KeyFrames[i].Frame}, Value : {group.KeyFrames[i].Value}" + slopeInfo + "]");
            }
        }

        public KEY_Parser FromText(string text)
        {
            KEY_Parser parser = new KEY_Parser();
            List<KEY_Parser.AnimJoint> joints = new List<KEY_Parser.AnimJoint>();
            KEY_Parser.AnimJoint activeJoint = null;
            KEY_Parser.Group activeGroup = null;

            foreach (var line in text.Split('\n'))
            {
                if (line.Contains("FrameCount:"))
                    parser.FrameCount = ushort.Parse(line.Split(':')[1]);
                if (line.Contains("FrameDelay:"))
                    parser.AnimationDelay = ushort.Parse(line.Split(':')[1]);
                if (line.Contains("Flags:"))
                    parser.Flags = uint.Parse(line.Split(':')[1]);
                if (line.Contains("Joint:"))
                {
                    activeJoint = new KEY_Parser.AnimJoint();
                    joints.Add(activeJoint);
                }

                if (line.Contains("Scale X:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.ScaleX = activeGroup;
                }
                if (line.Contains("Scale Y:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.ScaleY = activeGroup;
                }
                if (line.Contains("Scale Z:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.ScaleZ = activeGroup;
                }

                if (line.Contains("Position X:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.PositionX = activeGroup;
                }
                if (line.Contains("Position Y:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.PositionY = activeGroup;
                }
                if (line.Contains("Position Z:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.PositionZ = activeGroup;
                }

                if (line.Contains("Rotate X:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.RotateX = activeGroup;
                }
                if (line.Contains("Rotate Y:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.RotateY = activeGroup;
                }
                if (line.Contains("Rotate Z:"))
                {
                    activeGroup = new KEY_Parser.Group();
                    activeJoint.RotateZ = activeGroup;
                }

                if (line.Contains("Frame:"))
                {
                    //Get values from array
                    string value = line.Split('[')[1];
                    value = value.Replace("[", string.Empty);
                    value = value.Replace("]", string.Empty);

                   // Console.WriteLine($"value {value}");

                    //Get multiple values in the array
                    string[] values = value.Split(',');

                    KEY_Parser.KeyFrame keyFrame = new KEY_Parser.KeyFrame();
                    for (int i = 0; i < values.Length; i++) {
                        if (i == 0) {
                            keyFrame.Frame = float.Parse(values[i]);
                            continue;
                        }

                        string val = values[i].Split(':')[1].Replace(" ", string.Empty);
                        if (i == 1) keyFrame.Value = float.Parse(val);
                        if (i == 2) keyFrame.InSlope = float.Parse(val);
                        if (i == 3) keyFrame.OutSlope = float.Parse(val);
                    }

                    activeGroup.KeyFrames.Add(keyFrame);
                    activeGroup.FrameCount = (ushort)activeGroup.KeyFrames.Count;
                }
            }

            parser.AnimJoints = joints.ToArray();

            return parser;
        }

        public override void NextFrame()
        {
            STSkeleton skeleton = null;
            foreach (var container in Runtime.ModelContainers)
            {
                var skel = container.SearchActiveSkeleton();
                if (skel != null)
                    skeleton = skel;
            }

            if (skeleton == null) return;

            bool Updated = false;
            for (int i = 0; i < AnimGroups.Count; i++)
            {
                if (i >= skeleton.Bones.Count)
                    break;

                var joint = skeleton.Bones[i];
                AnimGroup group = (AnimGroup)AnimGroups[i];

                Updated = true;

                var position = joint.Position;
                var scale = joint.Scale;
                var rotate = joint.EulerRotation;

                if (group.PositionX.HasKeys)
                    position.X = group.PositionX.GetFrameValue(Frame);
                if (group.PositionY.HasKeys)
                    position.Y = group.PositionY.GetFrameValue(Frame);
                if (group.PositionZ.HasKeys)
                    position.Z = group.PositionZ.GetFrameValue(Frame);

                if (group.RotateX.HasKeys)
                    rotate.X = group.RotateX.GetFrameValue(Frame);
                if (group.RotateY.HasKeys)
                    rotate.Y = group.RotateY.GetFrameValue(Frame);
                if (group.RotateZ.HasKeys)
                    rotate.Z = group.RotateZ.GetFrameValue(Frame);

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

            if (Updated) {
                skeleton.Update();
            }
        }

        public class AnimGroup : STAnimGroup
        {
            public STAnimationTrack PositionX = new STAnimationTrack();
            public STAnimationTrack PositionY = new STAnimationTrack();
            public STAnimationTrack PositionZ = new STAnimationTrack();

            public STAnimationTrack RotateX = new STAnimationTrack();
            public STAnimationTrack RotateY = new STAnimationTrack();
            public STAnimationTrack RotateZ = new STAnimationTrack();

            public STAnimationTrack ScaleX = new STAnimationTrack();
            public STAnimationTrack ScaleY = new STAnimationTrack();
            public STAnimationTrack ScaleZ = new STAnimationTrack();

            public AnimGroup(KEY_Parser.AnimJoint animJoint)
            {
                PositionX = new AnimationTrack(animJoint.PositionX);
                PositionY = new AnimationTrack(animJoint.PositionY);
                PositionZ = new AnimationTrack(animJoint.PositionZ);
                RotateX = new AnimationTrack(animJoint.RotateX);
                RotateY = new AnimationTrack(animJoint.RotateY);
                RotateZ = new AnimationTrack(animJoint.RotateZ);
                ScaleX = new AnimationTrack(animJoint.ScaleX);
                ScaleY = new AnimationTrack(animJoint.ScaleY);
                ScaleZ = new AnimationTrack(animJoint.ScaleZ);
            }
        }

        public class AnimationTrack : STAnimationTrack
        {
            public AnimationTrack(KEY_Parser.Group track)
            {
                InterpolationType = STInterpoaltionType.Hermite;

                foreach (var keyFrame in track.KeyFrames)
                {
                    KeyFrames.Add(new STHermiteKeyFrame()
                    {
                       Frame = keyFrame.Frame,
                       Value = keyFrame.Value,
                        TangentIn = keyFrame.InSlope,
                       TangentOut = keyFrame.OutSlope,
                    });
                }
            }
        }
    }
}
