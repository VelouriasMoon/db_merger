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
                Console.WriteLine("Usage: db_merger.exe [-e/-b] [pv_db file]");
                Console.WriteLine("  -e:\tExtract file");
                Console.WriteLine("  -b:\tBuild file");
            }
            else if (args[0] == "-e")
            {
                dump_pv_db(args[1]);
            }
            else if (args[0] == "-b")
            {
                build_pv_db(args[1]);
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
                        Console.WriteLine($"Writing data to {data[0]}.txt...");
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

                Console.WriteLine($"Adding {Path.GetFileName(file)}...");
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
    }
}
