using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.WW
{
    public class RSC_Parser
    {
        public List<FileEntry> Files = new List<FileEntry>();

        public byte[] Unknowns { get; set; }

        public RSC_Parser(string fileName, System.IO.Stream stream)
        {
            using (var reader = new FileReader(stream))
            {
                reader.SetByteOrder(true);
                Unknowns = reader.ReadBytes(0x20); //ID list?
                ReadFiles(fileName, reader);
            }
        }

        private void ReadFiles(string fileName, FileReader reader)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(fileName);

            ResourceType type = (ResourceType)reader.ReadUInt32();
            uint size = reader.ReadUInt32();
            uint nextFileOffset = reader.ReadUInt32();
            reader.Seek(20); //padding

            if (size == 0)
                return;

            string extension = $"_{type}.bin";
            if (ExtensionList.ContainsKey(type))
                extension = ExtensionList[type];

            byte[] data = reader.ReadBytes((int)size);

            //File data right after
            var file = new FileEntry()
            {
                Type = type,
                FileName = $"Root/RawFiles/{name}_File{Files.Count}{extension}",
            };
            file.SetData(data);
            Files.Add(file);
            if (nextFileOffset != 0)
            {
                reader.SeekBegin(nextFileOffset);
                ReadFiles(fileName, reader);
            }
        }

        public void Save(System.IO.Stream stream)
        {
            using (var writer = new FileWriter(stream))
            {
                writer.SetByteOrder(true);
                writer.Write(Unknowns); //ID list?
                long currentOffset = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    if (currentOffset != 0)
                    {
                        long nextOffset = writer.Position;
                        using (writer.TemporarySeek(currentOffset + 8, System.IO.SeekOrigin.Begin)) {
                            writer.Write((uint)nextOffset);
                        }
                    }

                    currentOffset = writer.Position;
                    writer.Write((uint)Files[i].Type);
                    writer.Write((uint)Files[i].FileData.Length);
                    writer.Write((uint)0);
                    writer.Write(new byte[20]);
                    writer.Write(Files[i].AsBytes());
                    writer.AlignBytes(32);
                }
            }
        }

        public class FileEntry : ArchiveFileInfo {
            public ResourceType Type { get; set; }
        }

        public enum ResourceType
        {
            StaticModel = 0, //Static models with no animations/skeleton.
            TextureContainer = 1, //Stores multiple textures.
            SkeletonAnimation = 2, //Also includes skeleton data for rigged models
            MapModel = 3, //Map models and textures
            CollisionData = 4, //Collision tree data
            RiggedModel = 5, //Used in an animation. Always has a animation resource next to it.
            MapObjectParams = 6, //Placements for map objects done from hardcoded grouped assets.
            LightingData = 8, //Controls colors and parameters for lighting
            SpawnLocations = 9, //Controls player spawns and mini sub area spawns
            Animation = 10, //Animation without the skeleton data
            SpecialCollisionData = 11, //Controls collision on objects like bridges
            MessageFile = 14, //Message data

            //For the current unknown types
            //7 is used in AB22.RSC. It's fairly uncommon and unsure what type of data it has.
            //13 is used in some bosses. Unsure what it does.
        }

        private Dictionary<ResourceType, string> ExtensionList = new Dictionary<ResourceType, string>()
        {
            { ResourceType.TextureContainer, ".tpl" },
            { ResourceType.MessageFile, ".ww_message" },
            { ResourceType.RiggedModel, ".ww_rigged_model" },
            { ResourceType.StaticModel, ".ww_static_model" },
            { ResourceType.SkeletonAnimation, ".ww_skel_animation" },
            { ResourceType.Animation, ".ww_animation" },
            { ResourceType.MapModel, ".ww_map_model" },
            { ResourceType.CollisionData, ".ww_collison" },
            { ResourceType.SpecialCollisionData, ".ww_phys_collison" },
            { ResourceType.LightingData, ".ww_lights" },
            { ResourceType.MapObjectParams, ".ww_map_placements" },
            { ResourceType.SpawnLocations, ".ww_spawn_placements" },
        };
    }
}
