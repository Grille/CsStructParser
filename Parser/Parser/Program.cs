#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using GGL.IO;
using Tests;

namespace Programm
{
    class Program
    {
        unsafe private static void Main(string[] args)
        {
            //Test test = new Test();
            //test.Run();
            
            string h = ""+67;
            Parser parser = new Parser();
            //parser.AddAttribute("int[]", "test2", "0");
            parser.AddEnum("res", "waste", 3);
            //parser.ParseCode("Attributes{string name;int test;}Init{name = \"null\";test = 0;}");
            parser.ParseFile("../../testCodeAttr.txt");
            parser.Exists(6);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("------");
            foreach (string v in parser.GetAttributeNames())
            {
                Console.WriteLine("\"" + v + "\"");
            }
            Console.WriteLine("------");

            foreach (string v in parser.GetObjectNames())
            {
                Console.WriteLine("\n\n<" + v + ">");
                
                Console.WriteLine(parser.GetAttribute<string>(v, "name"));
                byte[] test = parser.GetAttribute<byte[]>(v, "array");
                for (int i = 0; i < test.Length; i++) Console.Write(test[i]+" ");
                
            }

            int hallo = 99;
            IntPtr ptr = new IntPtr(&hallo);
            byte[] data = new byte[4];
            
            //Console.WriteLine("\n\n: "+Marshal.SizeOf(hallo));
            //Console.WriteLine(ptr);
            //Marshal.Copy(ptr, data, 0, 4);
            /*
            Console.WriteLine(data[0]);
            Console.WriteLine(data[1]);
            Console.WriteLine(data[2]);
            Console.WriteLine(data[3]);
            */

            //Console.WriteLine(ptr);
            
            Console.ReadKey();
        }
    }
}
#endif
