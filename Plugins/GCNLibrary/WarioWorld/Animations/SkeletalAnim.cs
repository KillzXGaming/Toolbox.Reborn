using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.Animations;
using System.Linq;

namespace GCNLibrary.WW
{
    //Note:
    //Rather than having bone sets for models, the game will bake skeleton data into animations. 

    /// <summary>
    /// Reperesents an animation that animates a skeleton.
    /// </summary>
    public class SkeletalAnim
    {
        public string FileName;

        public List<ModelSkeleton> Skeletons = new List<ModelSkeleton>();
        public List<Animation> Animations = new List<Animation>();

        public SkeletalAnim(Stream stream, string fileName)
        {
            FileName = fileName;
            Read(new FileReader(stream));
        }

        /// <summary>
        /// Generates a list of index groups seperated per mesh.
        /// Used to determine which groups to use for indexing bone indices.
        /// </summary>
        public List<IndexGroup[]> GetMeshIndexGroups(int skeletonIndex)
        {
            var skeleton = Skeletons.FirstOrDefault();
            if (skeletonIndex < Skeletons.Count)
                skeleton = Skeletons[skeletonIndex];

            List<IndexGroup[]> meshIndexGroups = new List<IndexGroup[]>();
            List<IndexGroup> currentGroup = new List<IndexGroup>();
            for (int i = 0; i < skeleton.GlobalIndexList.Count; i++)
            {
                var indexGroup = skeleton.GlobalIndexList[i];
                currentGroup.Add(indexGroup);
                if (indexGroup.MeshIndexShift != 0) {
                    for (int j = 0; j < indexGroup.MeshIndexShift; j++)
                        meshIndexGroups.Add(currentGroup.ToArray());
                    currentGroup = new List<IndexGroup>();
                }
            }

            return meshIndexGroups;
        }

        /// <summary>
        /// Generates a baked skeleton based on the first frame of the animation.
        /// </summary>
        /// <returns></returns>
        public STSkeleton CreateBakedSkeleton(int skeletonIndex)
        {
            var skel = Skeletons.FirstOrDefault();
            if (skeletonIndex < Skeletons.Count)
                skel = Skeletons[skeletonIndex];
            if (skel == null)
                return new STSkeleton();

            var target = Animations[0];

            STSkeleton skeleton = new STSkeleton();
            for (int i = 0; i < skel.GlobalIndexList.Count; i++)
            {
                var boneAnim = target.BoneGroups[i];
                float posX = boneAnim.TranslateX.GetBaseValue();
                float posY = boneAnim.TranslateY.GetBaseValue();
                float posZ = boneAnim.TranslateZ.GetBaseValue();

                float rotX = boneAnim.RotateX.GetBaseValue();
                float rotY = boneAnim.RotateY.GetBaseValue();
                float rotZ = boneAnim.RotateZ.GetBaseValue();

                float scaleX = boneAnim.ScaleX.GetBaseValue(true);
                float scaleY = boneAnim.ScaleY.GetBaseValue(true);
                float scaleZ = boneAnim.ScaleZ.GetBaseValue(true);

                STBone bone = new STBone(skeleton);
                bone.ParentIndex = skel.GlobalIndexList[i].ParentIndex;
                bone.Name = $"Bone{i}";
                //Multiply by 1024 (hardcoded scale value)
                bone.Position = new Vector3(posX, posY, posZ) * 1024.0f;

                bone.EulerRotation = new Vector3(rotX, rotY, rotZ);
                bone.Scale = new Vector3(scaleX, scaleY, scaleZ);
                skeleton.Bones.Add(bone);
            }

            skeleton.Reset();
            skeleton.Update();
            return skeleton;
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            uint boneCount = reader.ReadUInt32();
            uint indexGroupOffset = reader.ReadUInt32() * 4;
            uint animCount = reader.ReadUInt32();
            uint animPtrOffset = reader.ReadUInt32() * 4;

            reader.SeekBegin(indexGroupOffset);
            while (reader.Position < reader.BaseStream.Length - 8)
            {
                ModelSkeleton skeleton = new ModelSkeleton();
                skeleton.RootGroup = new IndexGroup(reader, skeleton.GlobalIndexList, indexGroupOffset);
                Skeletons.Add(skeleton);

                //Reorder based on placement in the file.
                skeleton.GlobalIndexList = skeleton.GlobalIndexList.OrderBy(x => x.Position).ToList();
                for (int i = 0; i < skeleton.GlobalIndexList.Count; i++)
                {
                    skeleton.GlobalIndexList[i].Index = i;
                    if (skeleton.GlobalIndexList[i].Parent != null)
                        skeleton.GlobalIndexList[i].ParentIndex = skeleton.GlobalIndexList.IndexOf(skeleton.GlobalIndexList[i].Parent);
                }
            }

            reader.SeekBegin(animPtrOffset);
            uint[] animOffsets = reader.ReadUInt32s((int)animCount);

            for (int i = 0; i < animCount; i++) {
                reader.SeekBegin(animOffsets[i] * 4);
                Animations.Add(new Animation(reader, boneCount));
            }
        }

        public STSkeletonAnimation[] ToGeneric()
        {
            STSkeletonAnimation[] genericAnims = new STSkeletonAnimation[Animations.Count];
            for (int i = 0; i < Animations.Count; i++)
            {
                genericAnims[i] = new STSkeletonAnimation();
                genericAnims[i].Name = $"Animation{i}";
                genericAnims[i].FrameCount = Animations[i].FrameCount;
                foreach (var boneAnim in Animations[i].BoneGroups) {
                    STBoneAnimGroup group = new STBoneAnimGroup();
                    group.Name = $"Bone{genericAnims[i].AnimGroups.Count}";
                    group.TranslateX = ToGeneric(boneAnim.TranslateX, 0);
                    group.TranslateY = ToGeneric(boneAnim.TranslateY, 0);
                    group.TranslateZ = ToGeneric(boneAnim.TranslateZ, 0);
                    group.RotateX = ToGeneric(boneAnim.RotateX, 1);
                    group.RotateY = ToGeneric(boneAnim.RotateY, 1);
                    group.RotateZ = ToGeneric(boneAnim.RotateZ, 1);
                    group.ScaleX = ToGeneric(boneAnim.ScaleX, 2);
                    group.ScaleY = ToGeneric(boneAnim.ScaleY, 2);
                    group.ScaleZ = ToGeneric(boneAnim.ScaleZ, 2);
                    genericAnims[i].AnimGroups.Add(group);
                }
            }
            return genericAnims;
        }

        private STAnimationTrack ToGeneric(Track track, int trackType)
        {
            STAnimationTrack genericTrack = new STAnimationTrack();
            genericTrack.InterpolationType = STInterpoaltionType.Linear;

            float scale = 1.0f;
            if (trackType == 0) //Set hardcoded scale for translation types
                scale = 1024.0f;

            float currentFrame = 0;
            foreach (var key in track.Keys)
            {
                genericTrack.KeyFrames.Add(new STKeyFrame()
                {
                    Frame = currentFrame,
                    Value = key.Value * scale,
                    Slope = key.Delta,
                });
                currentFrame += key.FrameCount;
            }
            return genericTrack;
        }

        public class ModelSkeleton
        {
            public List<IndexGroup> GlobalIndexList = new List<IndexGroup>();
            public IndexGroup RootGroup { get; set; }
        }

        public class IndexGroup
        {
            /// <summary>
            /// The bone indices used for rigging.
            /// </summary>
            public List<AnimIndex> BoneIndices = new List<AnimIndex>();

            /// <summary>
            /// Children of this index group.
            /// </summary>
            public List<IndexGroup> Children = new List<IndexGroup>();

            public IndexGroup Parent { get; set; }

            /// <summary>
            /// The parent index group.
            /// </summary>
            public int ParentIndex { get; set; } = -1;

            /// <summary>
            /// The index of this group.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Shifts the current mesh to the next one.
            /// When the value is not 0, the next mesh will be used
            /// for the next the index group.
            /// </summary>
            public ushort MeshIndexShift { get; set; }

            internal long Position;

            public IndexGroup(FileReader reader, List<IndexGroup> indexGroups, uint startPos)
            {
                Position = reader.Position;
                indexGroups.Add(this);

                ushort boneIndexCount = reader.ReadUInt16();
                for (int i = 0; i < boneIndexCount; i++)
                    BoneIndices.Add(new AnimIndex(reader));

                MeshIndexShift = reader.ReadUInt16();

                ushort childCount = reader.ReadUInt16();
                ushort[] childIndices = reader.ReadUInt16s(childCount);
                for (int i = 0; i < childCount; i++) {
                    reader.SeekBegin(startPos + (childIndices[i] * 2));
                    Children.Add(new IndexGroup(reader, indexGroups, startPos));
                    Children[i].Parent = this;
                }
            }
        }

        public class AnimIndex
        {
            /// <summary>
            /// A bone index shift value used to determine what to rig to.
            /// To get the proper index, get the index of this current bone then subtract by the shift value.
            /// If the value is 0, the bone is rigged to this current bone.
            /// </summary>
            public byte BoneIndexShift { get; set; }

            /// <summary>
            /// The index used for rigging meshes.
            /// Mesh vertex data will share the same bone index. (both divided by 3 for proper index))
            /// Compares the values from vertices then use the bone index shift to determine what bone to assign to.
            /// </summary>
            public byte RiggingIndex { get; set; }

            public AnimIndex(FileReader reader)
            {
                BoneIndexShift = reader.ReadByte();
                RiggingIndex = reader.ReadByte();
            }
        }

        public class Animation
        {
            public float FrameCount { get; set; }

            public List<BoneGroup> BoneGroups = new List<BoneGroup>();

            public Animation(FileReader reader, uint boneCount)
            {
                FrameCount = reader.ReadSingle();
                uint boneAnimsOffset = reader.ReadUInt32() * 4;
                reader.SeekBegin(boneAnimsOffset);
                for (int i = 0; i < boneCount; i++) {
                    BoneGroups.Add(new BoneGroup(reader));
                }
            }
        }

        public class BoneGroup
        {
            public Track TranslateX { get; set; }
            public Track TranslateY { get; set; }
            public Track TranslateZ { get; set; }

            public Track RotateX { get; set; }
            public Track RotateY { get; set; }
            public Track RotateZ { get; set; }

            public Track ScaleX { get; set; }
            public Track ScaleY { get; set; }
            public Track ScaleZ { get; set; }

            public BoneGroup(FileReader reader) {
                TranslateX = new Track(reader, 0);
                TranslateY = new Track(reader, 0);
                TranslateZ = new Track(reader, 0);
                RotateX = new Track(reader, 1);
                RotateY = new Track(reader, 1);
                RotateZ = new Track(reader, 1);
                ScaleX = new Track(reader, 2);
                ScaleY = new Track(reader, 2);
                ScaleZ = new Track(reader, 2);
            }
        }

        public class Track
        {
            const uint KeySize = 12;

            public List<Key> Keys = new List<Key>();

            public float GetBaseValue(bool isScale = false)
            {
                if (Keys.Count > 0) {
                    return Keys[0].Value;
                }
                else
                    return isScale ? 1 : 0;
            }

            public Track(FileReader reader, int track) {
                uint firstFrameOffset = reader.ReadUInt32() * 4;
                uint lastFrameOffset = reader.ReadUInt32() * 4;

                if (lastFrameOffset != 0)
                {
                    //Determine the size between the first and last frames
                    uint dataSize = lastFrameOffset - firstFrameOffset + KeySize;
                    uint frameCount = dataSize / KeySize;

                    using (reader.TemporarySeek(firstFrameOffset, SeekOrigin.Begin))
                    {
                        for (int i = 0; i < frameCount; i++) {
                            Keys.Add(new Key()
                            {
                                FrameCount = reader.ReadSingle(),
                                Value = reader.ReadSingle(),
                                Delta = reader.ReadSingle(),
                            });
                        }
                    }
                }
            }
        }

        public class Key
        {
            /// <summary>
            /// The amount of frames to show this key on
            /// </summary>
            public float FrameCount { get; set; } 

            /// <summary>
            /// The value of the key.
            /// </summary>
            public float Value { get; set; }

            /// <summary>
            /// The difference between the current and the next key frame.
            /// </summary>
            public float Delta { get; set; }
        }
    }
}
