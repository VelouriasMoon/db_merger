using db_merger.Formats;
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
                    txt_db.dump_pv_db(args[2]);
                else if (args[1] == "-itm")
                    txt_db.dump_itm_tbl(args[2]);
                else if (args[1] == "-gm")
                    txt_db.dump_gm_module(args[2]);
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
                    txt_db.build_pv_db(args[1]);
                else if (args[1] == "-itm")
                    txt_db.build_itm_tbl(args[2]);
                else if (args[1] == "-gm")
                    txt_db.build_gm_module(args[2]);
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
    }
}
