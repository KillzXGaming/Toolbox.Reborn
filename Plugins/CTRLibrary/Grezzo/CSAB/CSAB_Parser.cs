using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.Animations;
using Toolbox.Core.IO;
using System.IO;

namespace CTRLibrary.Grezzo
{
    public class CSAB_Parser : STSkeletonAnimation
    {
        public GameVersion Version { get; set; }

        public enum GameVersion
        {
            OOT3D,
            MM3D,
            LM3DS,
        }

        public enum AnimationTrackType
        {
            LINEAR = 0x01,
            HERMITE = 0x02,
        };

        public CSAB_Parser(Stream stream) {
            Read(new FileReader(stream));
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(false);

            reader.ReadSignature(4, "csab");
            uint FileSize = reader.ReadUInt32();
            uint versionNum = reader.ReadUInt32();
            if (versionNum == 5)
                Version = GameVersion.MM3D;
            else if (versionNum == 3)
                Version = GameVersion.OOT3D;
            else
                Version = GameVersion.LM3DS;

            uint padding = reader.ReadUInt32(); //Unsure
            if (Version >= GameVersion.MM3D)
            {
                uint unknown = reader.ReadUInt32();//0x42200000
                uint unknown2 = reader.ReadUInt32();//0x42200000
                uint unknown3 = reader.ReadUInt32();//0x42200000
            }
            uint numAnimations = reader.ReadUInt32(); //Unsure
            uint location = reader.ReadUInt32(); //Unsure
            uint unknown4 = reader.ReadUInt32();//0x00
            uint unknown5 = reader.ReadUInt32();//0x00
            uint unknown6 = reader.ReadUInt32();//0x00
            uint unknown7 = reader.ReadUInt32();//0x00

            uint duration = reader.ReadUInt32();
            uint unknown9 = reader.ReadUInt32();//1
            uint nodeCount = reader.ReadUInt32();
            uint boneCount = reader.ReadUInt32();
            ushort[] BoneIndexTable = reader.ReadUInt16s((int)boneCount);
            reader.Align(4);
            uint[] nodeOffsets = reader.ReadUInt32s((int)nodeCount);

            FrameCount = duration;

            Console.WriteLine($"duration {duration}");
            Console.WriteLine($"boneCount {boneCount}");

            uint nodeSize = 0x18;
            if (Version >= GameVersion.MM3D)
                nodeSize = 0x24;

            for (int i = 0; i < nodeCount; i++)
            {
                reader.SeekBegin(nodeOffsets[i] + nodeSize);
                AnimationNode node = new AnimationNode();
                node.Read(reader, Version);
                AnimGroups.Add(node);
            }
        }


        public class AnimationNode : STAnimGroup
        {
            public ushort BoneIndex { get; set; }
            public ushort RotationFlags { get; set; }

            public AnimTrack TranslateX { get; set; }
            public AnimTrack TranslateY { get; set; }
            public AnimTrack TranslateZ { get; set; }
            public AnimTrack RotationX { get; set; }
            public AnimTrack RotationY { get; set; }
            public AnimTrack RotationZ { get; set; }
            public AnimTrack ScaleX { get; set; }
            public AnimTrack ScaleY { get; set; }
            public AnimTrack ScaleZ { get; set; }

            public override List<STAnimationTrack> GetTracks()
            {
                List<STAnimationTrack> tracks = new List<STAnimationTrack>();
                tracks.Add(TranslateX);
                tracks.Add(TranslateY);
                tracks.Add(TranslateZ);
                tracks.Add(RotationX);
                tracks.Add(RotationY);
                tracks.Add(RotationZ);
                tracks.Add(ScaleX);
                tracks.Add(ScaleY);
                tracks.Add(ScaleZ);
                return tracks;
            }

            public void Read(FileReader reader, GameVersion version)
            {
                long pos = reader.Position;
                reader.ReadSignature(4, "anod");
                BoneIndex = reader.ReadUInt16();
                RotationFlags = reader.ReadUInt16();
                TranslateX = ParseTrack(reader, version, pos);
                TranslateY = ParseTrack(reader, version, pos);
                TranslateZ = ParseTrack(reader, version, pos);
                RotationX = ParseTrack(reader, version, pos, RotationFlags == 1);
                RotationY = ParseTrack(reader, version, pos, RotationFlags == 1);
                RotationZ = ParseTrack(reader, version, pos, RotationFlags == 1);
                ScaleX = ParseTrack(reader, version, pos);
                ScaleY = ParseTrack(reader, version, pos);
                ScaleZ = ParseTrack(reader, version, pos);
                reader.ReadUInt16();//0x00
            }

            private static AnimTrack ParseTrack(FileReader reader, GameVersion version, long startPos, bool rotation = false)
            {
                long pos = reader.Position;

                uint Offset = reader.ReadUInt16();
                if (Offset == 0) return new AnimTrack();

                reader.SeekBegin(startPos + Offset);
                var track = new AnimTrack(reader, version, rotation);

                reader.SeekBegin(pos + sizeof(ushort)); //Seek back to next offset
                return track;
            }
        }

        public class AnimTrack : STAnimationTrack
        {
            public List<LinearKeyFrame> KeyFramesLinear = new List<LinearKeyFrame>();
            public List<HermiteKeyFrame> KeyFramesHermite = new List<HermiteKeyFrame>();

            public uint TrackInterpolationType;

            public AnimTrack() { }

            public AnimTrack(FileReader reader, GameVersion version, bool isShortRotation)
            {
                uint numKeyFrames = 0;

                if (version >= GameVersion.MM3D)
                {
                    reader.ReadByte(); //unk
                    TrackInterpolationType = reader.ReadByte();
                    numKeyFrames = reader.ReadUInt16();
                }
                else
                {
                    TrackInterpolationType = reader.ReadUInt32();
                    numKeyFrames = reader.ReadUInt32();
                    uint startFrame = reader.ReadUInt32();
                    uint endFrame = reader.ReadUInt32();
                }

                if (TrackInterpolationType == (uint)AnimationTrackType.LINEAR)
                {
                    InterpolationType = STInterpoaltionType.Linear;

                    if (version >= GameVersion.MM3D)
                    {
                        float scale = reader.ReadSingle();
                        float bias = reader.ReadSingle();

                        for (uint i = 0; i < numKeyFrames; i++)
                        {
                            float Value = reader.ReadUInt16() * scale - bias;
                            KeyFrames.Add(new STKeyFrame()
                            {
                                Frame = i,
                                Value = Value
                            });
                        }
                    }
                    else
                    {
                        for (int i = 0; i < numKeyFrames; i++)
                        {
                            if (isShortRotation)
                            {
                                KeyFrames.Add(new STKeyFrame()
                                {
                                    Frame = reader.ReadUInt16(),
                                    Value = (float)reader.ReadUInt16() * 0.001533981f,
                                });
                            }
                            else
                            {
                                KeyFrames.Add(new STKeyFrame()
                                {
                                    Frame = reader.ReadSingle(),
                                    Value = reader.ReadSingle()
                                });
                            }
                        }
                    }
                }
                else if (TrackInterpolationType == (uint)AnimationTrackType.HERMITE)
                {
                    InterpolationType = STInterpoaltionType.Hermite;

                    for (int i = 0; i < numKeyFrames; i++)
                    {
                        if (isShortRotation)
                        {
                            KeyFrames.Add(new STHermiteKeyFrame()
                            {
                                Frame = reader.ReadUInt16(),
                                Value = (float)reader.ReadInt16() * 0.001533981f,
                                TangentIn = (float)reader.ReadInt16() * 0.001533981f,
                                TangentOut = (float)reader.ReadInt16() * 0.001533981f,
                            });
                        }

                        else
                        {
                            KeyFrames.Add(new STHermiteKeyFrame()
                            {
                                Frame = reader.ReadUInt32(),
                                Value = reader.ReadSingle(),
                                TangentIn = reader.ReadSingle(),
                                TangentOut = reader.ReadSingle(),
                            });
                        }
                    }
                }
                else
                    throw new Exception("Unknown interpolation type! " + InterpolationType);
            }
        }

        public class HermiteKeyFrame
        {
            public uint Time { get; set; }
            public float Value { get; set; }
            public float TangentIn { get; set; }
            public float TangentOut { get; set; }
        }

        public class LinearKeyFrame
        {
            public uint Time { get; set; }
            public float Value { get; set; }
        }
    }
}
