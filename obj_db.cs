using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia;

namespace db_merger
{
    class obj_db
    {
        
        public class ModelBlock
        {
            public short id { get; set; }
            public short objset_id { get; set; }
            public uint NamePos { get; set; }

            public void Read(BinaryObjectReader models)
            {
                id = models.ReadInt16();
                objset_id = models.ReadInt16();
                NamePos = models.ReadUInt32();
            }
            public void Write(BinaryObjectWriter models)
            {
                models.WriteInt16(id);
                models.WriteInt16(objset_id);
                models.WriteUInt32(NamePos);
            }
            
        }
        public class ObjectBlock
        {
            public uint NamePos { get; set; }
            public uint id { get; set; }
            public uint FileNamePos { get; set; }
            public uint TextureNamePos { get; set; }
            public uint FarcNamePos { get; set; }
            public long padding_1 { get; set; }
            public long padding_2 { get; set; }

            public void Read(BinaryObjectReader objects)
            {
                NamePos = objects.ReadUInt32();
                id = objects.ReadUInt32();
                FileNamePos = objects.ReadUInt32();
                TextureNamePos = objects.ReadUInt32();
                FarcNamePos = objects.ReadUInt32();
                padding_1 = objects.ReadInt64();
                padding_2 = objects.ReadInt64();
            }

            public void Write(BinaryObjectWriter objects)
            {
                objects.WriteUInt32(NamePos);
                objects.WriteUInt32(id);
                objects.WriteUInt32(FileNamePos);
                objects.WriteUInt32(TextureNamePos);
                objects.WriteUInt32(FarcNamePos);
                objects.WriteUInt64(0);
                objects.WriteUInt64(0);
            }

        }
        public class objdbFile
        {
            public uint numofObj { get; set; }
            public uint maxid { get; set; }
            public uint objoffset { get; set; }
            public uint numofMesh { get; set; }
            public uint meshoffset { get; set; }
            public long Padding { get; set; }
            public uint Unk { get; set; }
            public ObjectBlock Objects { get; set; }
            public List<ObjectBlock> ObjectList { get; set; }
            public ModelBlock Models { get; set; }
            public List<ModelBlock> ModelList { get; set; }

            public void ReadData(string inpath)
            {
                using (BinaryObjectReader obj_db = new BinaryObjectReader(inpath, Endianness.Little, Encoding.UTF8))
                {
                    numofObj = obj_db.ReadUInt32();
                    maxid = obj_db.ReadUInt32();
                    objoffset = obj_db.ReadUInt32();
                    numofMesh = obj_db.ReadUInt32();
                    meshoffset = obj_db.ReadUInt32();

                    obj_db.Seek(objoffset, SeekOrigin.Begin);
                    ObjectList = new List<ObjectBlock>();
                    for (int i = 0; i < numofObj; i++)
                    {
                        Objects = new ObjectBlock();
                        Objects.Read(obj_db);
                        ObjectList.Add(Objects);
                    }

                    obj_db.Seek(meshoffset, SeekOrigin.Begin);
                    ModelList = new List<ModelBlock>();
                    for (int i = 0; i < numofMesh; i++)
                    {
                        Models = new ModelBlock();
                        Models.Read(obj_db);
                        ModelList.Add(Models);
                    }
                }
            }
        }

        public static objdbFile objdb;
        public static List<string> ObjectSetNames { get; private set; }
        public static List<string> ObjectNames { get; private set; }
        public static List<string> ModelSets { get; private set; }
        public static List<string> ObjectSets { get; private set; }
        public static List<int> IDs { get; private set; }

        public static void ReadStrings(string infile)
        {
            using (BinaryObjectReader database = new BinaryObjectReader(infile, Endianness.Little, Encoding.UTF8))
            {
                ObjectSetNames = new List<string>();
                ObjectNames = new List<string>();

                var count = database.ReadUInt32();
                database.Seek(0x8, SeekOrigin.Begin);
                var pointer = database.ReadUInt32();
                database.Seek(pointer, SeekOrigin.Begin);
                pointer = database.ReadUInt32();
                database.Seek(pointer, SeekOrigin.Begin);

                for (int i = 0; i < count * 4; i++)
                {
                    ObjectSetNames.Add(database.ReadString(StringBinaryFormat.NullTerminated));
                }

                database.Seek(0xC, SeekOrigin.Begin);
                count = database.ReadUInt32();
                pointer = database.ReadUInt32();
                database.Seek(pointer + 0x4, SeekOrigin.Begin);
                pointer = database.ReadUInt32();
                database.Seek(pointer, SeekOrigin.Begin);

                for (int i = 0; i < count; i++)
                {
                    long currentpos = database.Position;
                    string stringcheck = database.ReadString(StringBinaryFormat.NullTerminated);
                    if (stringcheck == "")
                        i--;
                    else
                        ObjectNames.Add(stringcheck);
                }
            }
        }

        public static void Dump_models(string infile)
        {
            objdb = new objdbFile();
            objdb.ReadData(infile);
            ModelSets = new List<string>();

            for (int i = 0; i < objdb.numofMesh; i++)
            {
                ModelSets.Add(Convert.ToString($"  Object:\n    ID: {objdb.ModelList[i].id}\n    Set_ID: {objdb.ModelList[i].objset_id}\n    Name: {ObjectNames[i]}\n"));
            }
        }

        public static void Dump_objdb(string infile)
        {
            objdb = new objdbFile();
            objdb.ReadData(infile);
            ObjectSets = new List<string>();
            IDs = new List<int>();

            for (int i = 0; i < objdb.numofObj; i++)
            {
                IDs.Add(Convert.ToInt32(objdb.ObjectList[i].id));
                ObjectSets.Add(Convert.ToString($"Object_Set:\n  Name: {ObjectSetNames[i * 4]}\n  ID: {objdb.ObjectList[i].id}\n  Model_Name: {ObjectSetNames[(i * 4) + 1]}\n  Texture_Name: {ObjectSetNames[(i * 4) + 2]}\n  Farc_Name: {ObjectSetNames[(i * 4) + 3]}\n"));
            }
        }

        public static void Read_Yaml(string infile)
        {
            string yamlfile = File.ReadAllText(infile);
            yamlfile = yamlfile.Replace("Object_Set:\n", "*");
            string[] Sets = yamlfile.Split('*');

            ObjectSets = new List<string>();
            ModelSets = new List<string>();

            foreach (string Set in Sets)
            {
                ObjectSets.Add(Set.Replace(" ", ""));
            }

            foreach (string Set in Sets)
            {
                string subset = Set.Replace("Object:\n", "*");
                string[] objects = subset.Split('*');
                for (int i = 0; i < objects.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            break;
                        default:
                            ModelSets.Add(objects[i].Replace(" ", ""));
                            break;
                    }
                }
            }
        }

        public static void Write_objdb(string infile, string outfile)
        {
            Console.WriteLine("Converting yaml file to obj_db");
            Read_Yaml(infile);
            ObjectSets.Remove("");
            using (BinaryObjectWriter obj_db = new BinaryObjectWriter(outfile, Endianness.Little, Encoding.UTF8))
            {
                byte nullbyte = 0;
                long currentPos = 0;
                long Eof = 0;
                uint MaxID = 3816;

                // Write Dummy Header
                for (int i = 0; i < 0x20; i++)
                {
                    obj_db.WriteByte(nullbyte);
                }

                // Rewrite object set Data
                currentPos = obj_db.Position;
                obj_db.Seek(0, SeekOrigin.Begin);       //return to start
                obj_db.WriteInt32(ObjectSets.Count);    //Write number of object sets
                obj_db.WriteUInt32(MaxID);       //Write Max number
                obj_db.WriteInt32(Convert.ToInt32(currentPos));          //Write Offset to object set data
                obj_db.Seek(currentPos, SeekOrigin.Begin);  //Return to end

                // Write Dummy Object Sets
                long ObjectSetPos = obj_db.Position;
                for (int i = 0; i < ObjectSets.Count; i++)
                {
                    for (int j = 0; j < 0x24; j++)
                    {
                        obj_db.WriteByte(nullbyte);
                    }
                }

                //Write Strings and rewriting Object Set Data
                for (int i = 0; i < ObjectSets.Count; i++)
                {
                    string subset = ObjectSets[i].Replace("\n","*");
                    string[] data = subset.Split('*');
                    currentPos = obj_db.Position;
                    obj_db.WriteStringNullTerminated(Encoding.UTF8, data[0].Split(':')[1]);     //Set Name
                    Eof = obj_db.Position;
                    obj_db.Seek((ObjectSetPos + (i * 36)), SeekOrigin.Begin);
                    obj_db.WriteInt32(Convert.ToInt32(currentPos));     //Write Pointer to String
                    if (Convert.ToInt32(data[1].Split(':')[1]) > MaxID)
                    {
                        currentPos = obj_db.Position;
                        obj_db.Seek(0x4, SeekOrigin.Begin);
                        obj_db.WriteInt32(Convert.ToInt32(data[1].Split(':')[1]));
                        obj_db.Seek(currentPos, SeekOrigin.Begin);
                    }
                    obj_db.WriteInt32(Convert.ToInt32(data[1].Split(':')[1]));      //Write Set ID
                    obj_db.Seek(Eof, SeekOrigin.Begin);
                    currentPos = obj_db.Position;
                    obj_db.WriteStringNullTerminated(Encoding.UTF8, data[2].Split(':')[1]);     //Model Name
                    Eof = obj_db.Position;
                    obj_db.Seek((ObjectSetPos + (i * 36) + 8), SeekOrigin.Begin);
                    obj_db.WriteInt32(Convert.ToInt32(currentPos));
                    obj_db.Seek(Eof, SeekOrigin.Begin);
                    currentPos = obj_db.Position;
                    obj_db.WriteStringNullTerminated(Encoding.UTF8, data[3].Split(':')[1]);     //Texture Name
                    Eof = obj_db.Position;
                    obj_db.Seek((ObjectSetPos + (i * 36) + 12), SeekOrigin.Begin);
                    obj_db.WriteInt32(Convert.ToInt32(currentPos));
                    obj_db.Seek(Eof, SeekOrigin.Begin);
                    currentPos = obj_db.Position;
                    obj_db.WriteStringNullTerminated(Encoding.UTF8, data[4].Split(':')[1]);     //Farc Name
                    Eof = obj_db.Position;
                    obj_db.Seek((ObjectSetPos + (i * 36) + 16), SeekOrigin.Begin);
                    obj_db.WriteInt32(Convert.ToInt32(currentPos));
                    obj_db.Seek(Eof, SeekOrigin.Begin);
                }

                obj_db.Align(4);

                //Write Dummy Model Sets
                long ModelSetPos = obj_db.Position;
                for (int i = 0; i < ModelSets.Count; i++)
                {
                    for (int j = 0; j < 0x8; j++)
                    {
                        obj_db.WriteByte(nullbyte);
                    }
                }

                //Rewrite Model Data
                currentPos = obj_db.Position;
                obj_db.Seek(0xC, SeekOrigin.Begin);
                obj_db.WriteInt32(ModelSets.Count);
                obj_db.WriteInt32(Convert.ToInt32(ModelSetPos));
                obj_db.Seek(currentPos, SeekOrigin.Begin);

                obj_db.Align(1);

                for (int i = 0; i < ModelSets.Count; i++)
                {
                    string subset = ModelSets[i].Replace("\n", "*");
                    string[] data = subset.Split('*');
                    currentPos = obj_db.Position;
                    obj_db.WriteStringNullTerminated(Encoding.UTF8, data[2].Split(':')[1]);
                    Eof = obj_db.Position;
                    obj_db.Seek((ModelSetPos + (i * 8)), SeekOrigin.Begin);
                    obj_db.WriteInt16(Convert.ToInt16(data[0].Split(':')[1]));
                    obj_db.WriteInt16(Convert.ToInt16(data[1].Split(':')[1]));
                    obj_db.WriteInt32(Convert.ToInt32(currentPos));
                    obj_db.Seek(Eof, SeekOrigin.Begin);
                }
            }
        }

        public static void extract_objdb(string infile, string outfile)
        {
            ReadStrings(infile);
            Dump_models(infile);
            Dump_objdb(infile);

            objdb = new objdbFile();
            objdb.ReadData(infile);

            if (File.Exists(outfile))
                File.Delete(outfile);
            Console.WriteLine("Converting obj_db to yaml");
            int j = 0;
            for (int i = 0; i < objdb.numofObj; i++)
            {
                if (!File.Exists(outfile))
                {
                    using (StreamWriter sw = File.CreateText(outfile))
                    {
                        sw.Write(ObjectSets[i]);
                        for (; j < objdb.numofMesh;)
                        {
                            if (ModelSets[j].Contains($"Set_ID: {objdb.ObjectList[i].id}\n"))
                            {
                                sw.Write(ModelSets[j]);
                                j++;
                            }
                            else
                                break;
                        }
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(outfile))
                    {
                        sw.Write(ObjectSets[i]);
                        for (; j < objdb.numofMesh;)
                        {
                            if (ModelSets[j].Contains($"Set_ID: {objdb.ObjectList[i].id}\n"))
                            {
                                sw.Write(ModelSets[j]);
                                j++;
                            }
                            else
                                break;
                        }
                    }
                }
            }
        }

        public static void Patch_objdb(string infile, string outfile, string inpatch)
        {
            if (File.Exists("Temp.yml"))
                File.Delete("Temp.yml");
            extract_objdb(infile, "Temp.yml");
            string[] patch = File.ReadAllLines(inpatch);
            int oldID = Convert.ToInt32(patch[2].Split(' ')[3]);
            int newID = oldID;

            Console.WriteLine("Patching and fixing IDs");
            while (IDs.Contains(newID))
            {
                newID++;
            }
            patch[2] = patch[2].Replace(Convert.ToString(oldID), Convert.ToString(newID));
            foreach (string line in patch)
            {
                if (line.Contains("Set_ID"))
                {
                    line.Replace(Convert.ToString(oldID), Convert.ToString(newID));
                }
                using (StreamWriter sw = File.AppendText("Temp.yml"))
                {
                    sw.Write(line + "\n");
                }
            }
            
            Write_objdb("Temp.yml", outfile);
            File.Delete("Temp.yml");
        }

        public static void Obj_db(string type, string infile, string outfile)
        {
            if (type == "-e")
            {
                extract_objdb(infile, outfile);
            }
            else if (type == "-b")
            {
                Write_objdb(infile, outfile);
            }
        }
    }
}
