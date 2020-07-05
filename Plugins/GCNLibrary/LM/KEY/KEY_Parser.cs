using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.LM
{
    public class KEY_Parser
    {
        public AnimJoint[] AnimJoints { get;set; }

        public ushort FrameCount { get; set; }
        public ushort AnimationDelay { get; set; }
        public uint Flags { get; set; }

        private uint ScaleKeyDataOffset;
        private uint RotationKeyDataOffset;
        private uint TranslationKeyDataOffset;

        public KEY_Parser() { }

        public KEY_Parser(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            uint jointCount = reader.ReadUInt32();
            FrameCount = reader.ReadUInt16();
            AnimationDelay = reader.ReadUInt16();
            Flags = reader.ReadUInt32(); //2 == loop
            ScaleKeyDataOffset = reader.ReadUInt32();
            RotationKeyDataOffset = reader.ReadUInt32();
            TranslationKeyDataOffset = reader.ReadUInt32();
            uint keyBeginIndicesOffset = reader.ReadUInt32();
            uint keyCountsOffset = reader.ReadUInt32();

            AnimJoints = new AnimJoint[(int)jointCount];
            for (int i = 0; i < jointCount; i++)
                AnimJoints[i] = new AnimJoint();

            reader.SeekBegin(keyBeginIndicesOffset);
            for (int i = 0; i < jointCount; i++)
            {
                AnimJoints[i].ScaleX.BeginIndex = reader.ReadUInt32();
                AnimJoints[i].ScaleY.BeginIndex = reader.ReadUInt32();
                AnimJoints[i].ScaleZ.BeginIndex = reader.ReadUInt32();
                AnimJoints[i].RotateX.BeginIndex = reader.ReadUInt32();
                AnimJoints[i].RotateY.BeginIndex = reader.ReadUInt32();
                AnimJoints[i].RotateZ.BeginIndex = reader.ReadUInt32();
                AnimJoints[i].PositionX.BeginIndex = reader.ReadUInt32();
                AnimJoints[i].PositionY.BeginIndex = reader.ReadUInt32();
                AnimJoints[i].PositionZ.BeginIndex = reader.ReadUInt32();
            }

            reader.SeekBegin(keyCountsOffset);
            for (int i = 0; i < jointCount; i++) {
                ReadGroupCount(AnimJoints[i].ScaleX, reader);
                ReadGroupCount(AnimJoints[i].ScaleY, reader);
                ReadGroupCount(AnimJoints[i].ScaleZ, reader);
                ReadGroupCount(AnimJoints[i].RotateX, reader);
                ReadGroupCount(AnimJoints[i].RotateY, reader);
                ReadGroupCount(AnimJoints[i].RotateZ, reader);
                ReadGroupCount(AnimJoints[i].PositionX, reader);
                ReadGroupCount(AnimJoints[i].PositionY, reader);
                ReadGroupCount(AnimJoints[i].PositionZ, reader);
            }

            for (int i = 0; i < jointCount; i++) {
                ReadKeyframe(reader, AnimJoints[i].ScaleX, 0);
                ReadKeyframe(reader, AnimJoints[i].ScaleY, 0);
                ReadKeyframe(reader, AnimJoints[i].ScaleZ, 0);
                ReadKeyframe(reader, AnimJoints[i].RotateX, 1);
                ReadKeyframe(reader, AnimJoints[i].RotateY, 1);
                ReadKeyframe(reader, AnimJoints[i].RotateZ, 1);
                ReadKeyframe(reader, AnimJoints[i].PositionX, 2);
                ReadKeyframe(reader, AnimJoints[i].PositionY, 2);
                ReadKeyframe(reader, AnimJoints[i].PositionZ, 2);
            }
        }

        private void Write(FileWriter writer)
        {
            writer.SetByteOrder(true);
            writer.Write(AnimJoints.Length);
            writer.Write(FrameCount);
            writer.Write(AnimationDelay);
            writer.Write(Flags);
            writer.Write(uint.MaxValue); //Scale offset
            writer.Write(uint.MaxValue); //Rotation offset
            writer.Write(uint.MaxValue); //Translation offset
            writer.Write(uint.MaxValue); //key indices offset
            writer.Write(uint.MaxValue); //key counts offset

            writer.WriteUint32Offset(12);
            WriteScaleGroup(writer, AnimJoints);

            writer.WriteUint32Offset(16);
            WriteRotationGroup(writer, AnimJoints);

            writer.WriteUint32Offset(20);
            WriteTranslationGroup(writer, AnimJoints);

            writer.WriteUint32Offset(24);
            foreach (var joint in AnimJoints)
            {
                writer.Write(joint.ScaleX.BeginIndex);
                writer.Write(joint.ScaleY.BeginIndex);
                writer.Write(joint.ScaleZ.BeginIndex);
                writer.Write(joint.RotateX.BeginIndex);
                writer.Write(joint.RotateY.BeginIndex);
                writer.Write(joint.RotateZ.BeginIndex);
                writer.Write(joint.PositionX.BeginIndex);
                writer.Write(joint.PositionY.BeginIndex);
                writer.Write(joint.PositionZ.BeginIndex);
            }

            writer.WriteUint32Offset(28);
            foreach (var joint in AnimJoints)
            {
                WriteKeyGroup(writer, joint.ScaleX);
                WriteKeyGroup(writer, joint.ScaleY);
                WriteKeyGroup(writer, joint.ScaleZ);
                WriteKeyGroup(writer, joint.RotateX);
                WriteKeyGroup(writer, joint.RotateY);
                WriteKeyGroup(writer, joint.RotateZ);
                WriteKeyGroup(writer, joint.PositionX);
                WriteKeyGroup(writer, joint.PositionY);
                WriteKeyGroup(writer, joint.PositionZ);
            }
        }

        private void WriteKeyGroup(FileWriter writer, Group group)
        {
            writer.Write((byte)group.SlopeFlag);
            writer.Write((byte)group.FrameCount);
        }

        private void WriteTranslationGroup(FileWriter writer, AnimJoint[] joints)
        {
            List<float> buffer = new List<float>();
            for (int i = 0; i < joints.Length; i++)
            {
                SetKeyGroupDataF32(buffer, joints[i].PositionX);
                SetKeyGroupDataF32(buffer, joints[i].PositionY);
                SetKeyGroupDataF32(buffer, joints[i].PositionZ);
            }
            writer.Write(buffer.ToArray());
            buffer.Clear();
        }

        private void WriteRotationGroup(FileWriter writer, AnimJoint[] joints)
        {
            List<short> buffer = new List<short>();
            for (int i = 0; i < joints.Length; i++)
            {
                SetKeyGroupDataU16(buffer, joints[i].RotateX);
                SetKeyGroupDataU16(buffer, joints[i].RotateY);
                SetKeyGroupDataU16(buffer, joints[i].RotateZ);
            }
            writer.Write(buffer.ToArray());
            buffer.Clear();
        }

        private void WriteScaleGroup(FileWriter writer, AnimJoint[] joints)
        {
            List<float> buffer = new List<float>();
            for (int i = 0; i < joints.Length; i++)
            {
                SetKeyGroupDataF32(buffer, joints[i].ScaleX);
                SetKeyGroupDataF32(buffer, joints[i].ScaleY);
                SetKeyGroupDataF32(buffer, joints[i].ScaleZ);
            }
            writer.Write(buffer.ToArray());
            buffer.Clear();
        }

        private void SetKeyGroupDataF32(List<float> buffer, Group group)
        {
            var values = GetKeyGroupDataF32(group);
            if (values.Length == 1 && buffer.Contains(values[0])) {
                group.BeginIndex = (uint)buffer.IndexOf(values[0]);
            }
            else
            {
                group.BeginIndex = (uint)buffer.Count;
                buffer.AddRange(values);
            }
        }

        private void SetKeyGroupDataU16(List<short> buffer, Group group)
        {
            var values = GetKeyGroupDataU16(group);
            if (values.Length == 1 && buffer.Contains(values[0]))
            {
                group.BeginIndex = (uint)buffer.IndexOf(values[0]);
            }
            else
            {
                int index = CompareUtility.SearchArray<short>(buffer.ToArray(), values);
                if (index != -1)
                    group.BeginIndex = (uint)index;
                else
                {
                    group.BeginIndex = (uint)buffer.Count;
                    buffer.AddRange(values);
                }
            }
        }

        private short[] GetKeyGroupDataU16(Group group)
        {
            int numElements = GetElementCount(group);
            short[] values = new short[numElements];
            int index = 0;
            for (int i = 0; i < group.FrameCount; i++)
            {
                if (group.FrameCount == 1)
                    values[index++] = (short)(group.KeyFrames[0].Value / 0.001533981f);
                else
                {
                    values[index++] = (short)group.KeyFrames[i].Frame;
                    values[index++] = (short)(group.KeyFrames[i].Value / 0.001533981f);
                    values[index++] = (short)(group.KeyFrames[i].InSlope / 0.001533981f);
                    if (group.SlopeFlag != 0)
                        values[index++] = (short)group.KeyFrames[i].OutSlope;
                }
            }
            return values;
        }

        private float[] GetKeyGroupDataF32(Group group)
        {
            int numElements = GetElementCount(group);
            float[] values = new float[numElements];

            int index = 0;
            for (int i = 0; i < group.FrameCount; i++)
            {
                if (group.FrameCount == 1)
                    values[index++] = group.KeyFrames[0].Value;
                else
                {
                    values[index++] = group.KeyFrames[i].Frame;
                    values[index++] = group.KeyFrames[i].Value;
                    values[index++] = group.KeyFrames[i].InSlope;
                    if (group.SlopeFlag != 0)
                        values[index++] = group.KeyFrames[i].OutSlope;
                }
            }
            return values;
        }

        private int GetElementCount(Group group)
        {
            if (group.FrameCount == 1) return 1;
            else return group.FrameCount * (group.SlopeFlag > 0 ? 4 : 3);
        }

        private void ReadGroupCount(Group group, FileReader reader)
        {
            group.SlopeFlag = reader.ReadByte();
            group.FrameCount = reader.ReadByte();
        }

        private void ReadKeyframe(FileReader reader, Group group, int type)
        {
            uint offset = 0;
            if (type == 0) offset = ScaleKeyDataOffset;
            if (type == 1) offset = RotationKeyDataOffset;
            if (type == 2) offset = TranslationKeyDataOffset;

            reader.SeekBegin(offset + (group.BeginIndex * (type == 1 ? 2 : 4)));

            KeyFrame[] keyFrames = new KeyFrame[group.FrameCount];
            //Use constants for single frames
            if (group.FrameCount == 1)
            {
                keyFrames[0] = new KeyFrame()
                {
                    Value = ReadKeyData(reader, type),
                    Frame = 0,
                };
            }
            else
            {
                for (int i = 0; i < group.FrameCount; i++)   
                {
                    keyFrames[i] = new KeyFrame();

                    //Keys have frame, value, slope
                    keyFrames[i].Frame = ReadKeyData(reader, type);
                    keyFrames[i].Value = ReadKeyData(reader, type);
                    keyFrames[i].InSlope = ReadKeyData(reader, type);
                    if (group.SlopeFlag != 0)
                        keyFrames[i].OutSlope = ReadKeyData(reader, type);
                }
            }

            if (type == 1)
            {
                for (int i = 0; i < keyFrames.Length; i++)
                {
                    keyFrames[i].Value *= 0.001533981f;
                    keyFrames[i].InSlope *= 0.001533981f;
                    keyFrames[i].OutSlope *= 0.001533981f;
                }
            }

            group.KeyFrames = keyFrames.ToList();
        }

        private float ReadKeyData(FileReader reader, int type)
        {
            if (type == 1)
                return reader.ReadInt16();
            else
                return reader.ReadSingle();
        }

        public class AnimJoint
        {
            public Group ScaleX = new Group();
            public Group ScaleY = new Group();
            public Group ScaleZ = new Group();

            public Group RotateX = new Group();
            public Group RotateY = new Group();
            public Group RotateZ = new Group();

            public Group PositionX = new Group();
            public Group PositionY = new Group();
            public Group PositionZ = new Group();
        }

        public class Group
        {
            public uint BeginIndex;
            public ushort FrameCount;

            public byte SlopeFlag; //0x80 for second slope used

            public List<KeyFrame> KeyFrames = new List<KeyFrame>();
        }

        public class KeyFrame
        {
            public float Frame { get; set; }
            public float Value { get; set; }
            public float InSlope { get; set; }
            public float OutSlope { get; set; }
        }
    }
}
