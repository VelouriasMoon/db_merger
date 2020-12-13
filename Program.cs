using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace db_merger
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.WriteLine("Usage: db_merger.exe [-e/-b] [-pv/-itm/-gm/-obj] [db file]");
                Console.WriteLine("  -e:\tExtract file");
                Console.WriteLine("  -b:\tBuild file");
                Console.WriteLine("\n  -pv:\tInput is a pv_db");
                Console.WriteLine("  -itm:\tInput is a itm_tbl");
                Console.WriteLine("  -gm:\tInput is a gm_module");
                Console.WriteLine("  -obj:\tInput file is obj_db");
            }
            else if (args[0] == "-e")
            {
                if (args[1] == "-pv")
                    dump_pv_db(args[2]);
                else if (args[1] == "-itm")
                    dump_itm_tbl(args[2]);
                else if (args[1] == "-gm")
                    dump_gm_module(args[2]);
                else if (args[1] == "-obj")
                {
                    string outfile = Path.GetFileNameWithoutExtension(args[2]) + ".yml";
                    obj_db.Obj_db("-e", args[2], outfile);
                }
                else
                    Console.WriteLine("Invaild Arugument: 2");
            }
            else if (args[0] == "-b")
            {
                if (args[1] == "-pv")
                    build_pv_db(args[1]);
                else if (args[1] == "-itm")
                    build_itm_tbl(args[2]);
                else if (args[1] == "-gm")
                    build_gm_module(args[2]);
                else if (args[1] == "-obj")
                {
                    string outfile = Path.GetFileNameWithoutExtension(args[2]) + ".bin";
                    obj_db.Obj_db("-b", args[2], outfile);
                }
                else
                    Console.WriteLine("Invaild Arugument: 2");
            }
            else if (args[0] == "-p")
            {
                if (args[1] == "-obj")
                {
                    string outfile = Path.GetFileNameWithoutExtension(args[2]) + "_Patched.bin";
                    obj_db.Patch_objdb(args[2], outfile, args[3]);
                }
                else
                    Console.WriteLine("Invaild Arugument: 2");
            }
            else
            {
                Console.WriteLine("Invaild Arugument: 1");
            }
            
        }

        private static void dump_pv_db(string infile)
        {
            //String Setup
            string[] lines = File.ReadAllLines(infile);
            string decpath = Path.GetDirectoryName(infile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(infile);
            
            //File and Path Clean up
            if (Path.GetDirectoryName(infile) == string.Empty)
                decpath = Path.GetFileNameWithoutExtension(infile);
            if (!Directory.Exists(decpath))
                Directory.CreateDirectory(decpath);
            else if (Directory.Exists(decpath))
            {
                Directory.Delete(decpath, true);
                Directory.CreateDirectory(decpath);
            }

            //Line by line dumping
            Console.WriteLine($"Dumping {Path.GetFileName(infile)} to pv files...");
            foreach (string line in lines)
            {
                string[] data = line.Split('.');

                if (line == string.Empty || data[0][0] == '#')
                    continue;
                if (!File.Exists(decpath + Path.DirectorySeparatorChar + data[0] + ".txt"))
                {
                    using (StreamWriter sw = File.CreateText(decpath + Path.DirectorySeparatorChar + data[0] + ".txt"))
                    {
                        sw.WriteLine(line);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(decpath + Path.DirectorySeparatorChar + data[0] + ".txt"))
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            Console.WriteLine("Done");
        }

        private static void build_pv_db(string inpath)
        {
            string[] files = Directory.GetFiles(inpath);
            string decpath = Path.GetDirectoryName(inpath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inpath);

            if (Path.GetDirectoryName(inpath) == string.Empty)
                decpath = Path.GetFileNameWithoutExtension(inpath);
            if (File.Exists(decpath + ".txt"))
                File.Delete(decpath + ".txt");

            Console.WriteLine($"Building {Path.GetFileName(decpath)}.txt from {Path.GetFileName(decpath)}...");
            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);

                //Console.WriteLine($"Adding {Path.GetFileName(file)}...");
                if (!File.Exists(decpath + ".txt"))
                {
                    using (StreamWriter sw = File.CreateText(decpath + ".txt"))
                    {
                        foreach (string line in lines)
                        {
                            sw.WriteLine(line);
                        }
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(decpath + ".txt"))
                    {
                        foreach (string line in lines)
                        {
                            sw.WriteLine(line);
                        }
                    }
                }
            }
            Console.WriteLine("Done");
        }

        private static void dump_itm_tbl(string infile)
        {
            string[] lines = File.ReadAllLines(infile);
            string decpath = Path.GetDirectoryName(infile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(infile);

            if (Path.GetDirectoryName(infile) == string.Empty)
                decpath = Path.GetFileNameWithoutExtension(infile);
            if (!Directory.Exists(decpath))
                Directory.CreateDirectory(decpath);
            else if (Directory.Exists(decpath))
            {
                Directory.Delete(decpath, true);
                Directory.CreateDirectory(decpath);
            }

            Console.WriteLine($"Dumping {Path.GetFileName(infile)} to itm files...");
            foreach (string line in lines)
            {
                string[] data = line.Split('.');
                if (line == string.Empty || data[0][0] == '#')
                    continue;
                if (!Directory.Exists(decpath + Path.DirectorySeparatorChar + data[0]))
                {
                    Directory.CreateDirectory(decpath + Path.DirectorySeparatorChar + data[0]);
                }

                if (data[1].Length >= 6 && data[1].Substring(0,6) == "length")
                {
                    continue;
                }

                if (!File.Exists(decpath + Path.DirectorySeparatorChar + data[0] + Path.DirectorySeparatorChar + $"{data[0]}.{data[1]}.txt"))
                {
                    using (StreamWriter sw = File.CreateText(decpath + Path.DirectorySeparatorChar + data[0] + Path.DirectorySeparatorChar + $"{data[0]}.{data[1]}.txt"))
                    {
                        //Console.WriteLine($"Writing data to {data[0]}\\{data[0]}.{data[1]}.txt...");
                        sw.WriteLine(line);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(decpath + Path.DirectorySeparatorChar + data[0] + Path.DirectorySeparatorChar + $"{data[0]}.{data[1]}.txt"))
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            Console.WriteLine("Done");
        }

        private static void build_itm_tbl(string inpath)
        {
            string[] folders = Directory.GetDirectories(inpath);
            string decpath = Path.GetDirectoryName(inpath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inpath);

            if (Path.GetDirectoryName(inpath) == string.Empty)
                decpath = Path.GetFileNameWithoutExtension(inpath);
            if (File.Exists(decpath + ".txt"))
                File.Delete(decpath + ".txt");

            Console.WriteLine($"Build itm_tbl from {Path.GetFileName(inpath)}...");
            foreach (string folder in folders)
            {
                string[] files = Directory.GetFiles(folder);

                foreach (string file in files)
                {
                    string[] lines = File.ReadAllLines(file);

                    //Console.WriteLine($"Adding {Path.GetFileName(file)}...");
                    if (!File.Exists(decpath + ".txt"))
                    {
                        using (StreamWriter sw = File.CreateText(decpath + ".txt"))
                        {
                            foreach (string line in lines)
                            {
                                sw.WriteLine(line);
                            }
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = File.AppendText(decpath + ".txt"))
                        {
                            foreach (string line in lines)
                            {
                                sw.WriteLine(line);
                            }
                        }
                    }
                }

                using (StreamWriter sw = File.AppendText(decpath + ".txt"))
                {
                    sw.WriteLine($"{Path.GetFileNameWithoutExtension(folder)}.length={files.Length}");
                }
                
            }
            Console.WriteLine("Done");
        }

        private static void dump_gm_module(string infile)
        {
            string[] lines = File.ReadAllLines(infile);
            string decpath = Path.GetDirectoryName(infile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(infile);

            if (Path.GetDirectoryName(infile) == string.Empty)
                decpath = Path.GetFileNameWithoutExtension(infile);
            if (!Directory.Exists(decpath))
                Directory.CreateDirectory(decpath);
            else if (Directory.Exists(decpath))
            {
                Directory.Delete(decpath, true);
                Directory.CreateDirectory(decpath);
            }

            Console.WriteLine($"Dumping {Path.GetFileName(infile)} to module files...");
            foreach (string line in lines)
            {
                string[] data = line.Split('.');
                
                
                if (data[0] == "patch=0" || data[0] == "version=0")
                    continue;
                if (line == string.Empty || data[0][0] == '#')
                    continue;
                if (data[1] == "data_list")
                    continue;

                string filepath = decpath + Path.DirectorySeparatorChar + data[0] + "." + data[1] + ".txt";
                if (!File.Exists(filepath))
                {
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        //Console.WriteLine($"Writing data to { data[0]}.{ data[1]}.txt...");
                        sw.WriteLine(line);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            Console.WriteLine("Done");
        }

        private static void build_gm_module(string inpath)
        {
            string[] files = Directory.GetFiles(inpath);
            string decpath = Path.GetDirectoryName(inpath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inpath);
            string[] type = files[0].Split('.');
            type = type[0].Split('\\');

            if (Path.GetDirectoryName(inpath) == string.Empty)
                decpath = Path.GetFileNameWithoutExtension(inpath);
            if (File.Exists(decpath + ".bin"))
                File.Delete(decpath + ".bin");

            // Write Padding
            Console.WriteLine("Writing Header Padding...");
            using (StreamWriter sw = File.CreateText(decpath + ".bin"))
            {
                for (int i = 0; i < files.Length; i++)
                {
                    sw.WriteLine("#---------------------------------------------");
                }
            }

            Console.WriteLine($"Building {Path.GetFileName(decpath)}.bin from {Path.GetFileName(decpath)}...");
            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);

                //Console.WriteLine($"Adding {Path.GetFileName(file)}...");
                using (StreamWriter sw = File.AppendText(decpath + ".bin"))
                {
                    foreach (string line in lines)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            Console.WriteLine("Adding Ending Data...");
            using (StreamWriter sw = File.AppendText(decpath + ".bin"))
            {
                sw.WriteLine($"{type[1]}.data_list.length={files.Length}");
                if (type[1] == "cstm_item")
                {
                    sw.WriteLine("patch=0");
                    sw.WriteLine("version=0");
                }
            }

            Console.WriteLine("Done");
        }
    }
}
