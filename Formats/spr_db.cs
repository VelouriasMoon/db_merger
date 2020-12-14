using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
    }
}
