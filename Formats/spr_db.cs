using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Amicitia.IO;

namespace db_merger.Formats
{
    class spr_db
    {
        public class SpriteBlock
        {
            public uint ID { get; set; }
            public uint NamePos { get; set; }
            public ushort Index { get; set; }
            public ushort SetID { get; set; }
            public void Read(BinaryObjectReader sprite)
            {
                ID = sprite.ReadUInt32();
                NamePos = sprite.ReadUInt32();
                Index = sprite.ReadUInt16();
                SetID = sprite.ReadUInt16();
            }
        }
        public class SpriteSetBlock
        {
            public uint ID { get; set; }
            public uint NamePos { get; set; }
            public uint FilenamePost { get; set; }
            public uint Index { get; set; }
            public void Read(BinaryObjectReader spriteset)
            {
                ID = spriteset.ReadUInt32();
                NamePos = spriteset.ReadUInt32();
                FilenamePost = spriteset.ReadUInt32();
                Index = spriteset.ReadUInt32();
            }
        }
        public class sprdbFile
        {
            public uint SpriteSetCount { get; set; }
            public uint SpriteSetOffset { get; set; }
            public uint SpriteCount { get; set; }
            public uint SpriteOffset { get; set; }
            public SpriteSetBlock SpriteSets { get; set; }
            public List<SpriteSetBlock> SpriteSetList { get; set; }
            public SpriteBlock Sprites { get; set; }
            public List<SpriteBlock> SpriteList { get; set; }
            
            public void Read(string inpath)
            {
                using (BinaryObjectReader sprdb = new BinaryObjectReader(inpath, Endianness.Little, Encoding.UTF8))
                {
                    SpriteSetCount = sprdb.ReadUInt32();
                    SpriteSetOffset = sprdb.ReadUInt32();
                    SpriteCount = sprdb.ReadUInt32();
                    SpriteOffset = sprdb.ReadUInt32();

                    sprdb.Seek(SpriteSetOffset, SeekOrigin.Begin);
                    SpriteSetList = new List<SpriteSetBlock>();
                    for (int i = 0; i < SpriteSetCount; i++)
                    {
                        SpriteSets = new SpriteSetBlock();
                        SpriteSets.Read(sprdb);
                        SpriteSetList.Add(SpriteSets);
                    }

                    sprdb.Seek(SpriteOffset, SeekOrigin.Begin);
                    SpriteList = new List<SpriteBlock>();
                    for (int i = 0; i < SpriteCount; i++)
                    {
                        Sprites = new SpriteBlock();
                        Sprites.Read(sprdb);
                        SpriteList.Add(Sprites);
                    }
                }
            }
        }

        public static sprdbFile sprdb;
        public static List<string> SpriteSetNames;
        public static List<string> SpriteNames;
        public static List<string> SpriteSets;
        public static List<string> Sprites;
        public static List<string> Textures;
        public static List<uint> SetIDs;
        public static List<uint> IDs;

        public static void ReadStrings(string infile)
        {
            using (BinaryObjectReader database = new BinaryObjectReader(infile, Endianness.Little, Encoding.UTF8))
            {
                SpriteSetNames = new List<string>();
                SpriteNames = new List<string>();

                uint count = database.ReadUInt32();
                uint Pointer = database.ReadUInt32();
                database.Seek(Pointer + 0x4, SeekOrigin.Begin);
                Pointer = database.ReadUInt32();
                database.Seek(Pointer, SeekOrigin.Begin);

                for (int i = 0; i < count * 2; i++)
                {
                    string stringcheck = database.ReadString(StringBinaryFormat.NullTerminated);
                    if (stringcheck == "")
                        i--;
                    else
                        SpriteSetNames.Add(stringcheck);
                }

                database.Seek(0x8, SeekOrigin.Begin);
                count = database.ReadUInt32();
                Pointer = database.ReadUInt32();
                database.Seek(Pointer + 0x4, SeekOrigin.Begin);
                Pointer = database.ReadUInt32();
                database.Seek(Pointer, SeekOrigin.Begin);

                for (int i = 0; i < count; i++)
                {
                    string stringcheck = database.ReadString(StringBinaryFormat.NullTerminated);
                    if (stringcheck == "")
                        i--;
                    else
                        SpriteNames.Add(stringcheck);
                }
            }
        }

        public static void DumpFile(string infile)
        {
            // Phrase file and Setup lists
            sprdb = new sprdbFile();
            sprdb.Read(infile);
            //Sprites = new List<string>();
            //Textures = new List<string>();
            IDs = new List<uint>();
            SpriteSets = new List<string>();
            SetIDs = new List<uint>();

            // Phrase each Set Entry into yaml and store IDs
            for (int i = 0; i < sprdb.SpriteSetCount; i++)
            {
                SetIDs.Add(Convert.ToUInt32(sprdb.SpriteSetList[i].ID));
                SpriteSets.Add(Convert.ToString($"Sprite_Set:\n  ID: {sprdb.SpriteSetList[i].ID}\n  Name: {SpriteSetNames[i * 2]}\n  File_Name: {SpriteSetNames[(i * 2) + 1]}\n  Index: {sprdb.SpriteSetList[i].Index}\n"));
            }

            // Phrase each Sprite into yaml and store IDs
            for (int i = 0; i < sprdb.SpriteCount; i++)
            {
                IDs.Add(Convert.ToUInt32(sprdb.SpriteList[i].ID));
                // Sort between sprites and textures
                if (sprdb.SpriteList[i].SetID >= 4096)
                {
                    if (!SpriteSets[sprdb.SpriteList[i].SetID - 4096].Contains("Textures:"))
                    {
                        SpriteSets[sprdb.SpriteList[i].SetID - 4096] = SpriteSets[sprdb.SpriteList[i].SetID - 4096] + "  Textures:\n";
                    }
                    SpriteSets[sprdb.SpriteList[i].SetID - 4096] = SpriteSets[sprdb.SpriteList[i].SetID - 4096] + $"    Texture:\n      ID: {sprdb.SpriteList[i].ID}\n      Name: {SpriteNames[i]}\n      Index: {sprdb.SpriteList[i].Index}\n      Set_Index: {sprdb.SpriteList[i].SetID - 4096}\n";
                }
                else
                {
                    if (!SpriteSets[sprdb.SpriteList[i].SetID].Contains("Sprites:"))
                    {
                        SpriteSets[sprdb.SpriteList[i].SetID] = SpriteSets[sprdb.SpriteList[i].SetID] + "  Sprites:\n";
                    }
                    SpriteSets[sprdb.SpriteList[i].SetID] = SpriteSets[sprdb.SpriteList[i].SetID] + $"    Sprite:\n      ID: {sprdb.SpriteList[i].ID}\n      Name: {SpriteNames[i]}\n      Index: {sprdb.SpriteList[i].Index}\n      Set_Index: {sprdb.SpriteList[i].SetID}\n";
                }
            }
        }

        public static void Read_YAML(string infile)
        {
            // Split each Sprite Set into it's own array entry
            string yamlfile = File.ReadAllText(infile);
            yamlfile = yamlfile.Replace("\r", "").Replace("Sprite_Set:\n", "*");
            string[] Sets = yamlfile.Split('*');


        }

        public static void Extract_sprdb(string infile)
        {
            //Read File Content
            ReadStrings(infile);
            DumpFile(infile);

            string outfile = Path.GetFileNameWithoutExtension(infile) + ".yml";
            if (File.Exists(outfile))
                File.Delete(outfile);

            for (int i = 0; i < sprdb.SpriteSetCount; i++)
            {
                if (!File.Exists(outfile))
                {
                    using (StreamWriter sw = File.CreateText(outfile))
                    {
                        sw.Write(SpriteSets[i]);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(outfile))
                    {
                        sw.Write(SpriteSets[i]);
                    }
                }
            }
        }
    }
}
