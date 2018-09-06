using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GGL.IO;

namespace Programm
{
    class Program
    {
        static void Main(string[] args)
        {
            string h = ""+67;
            Parser parser = new Parser();
            //parser.AddAttribute("int[]", "test2", "0");
            parser.AddEnum("res", "waste", 3);
            //parser.ParseCode("Attributes{string name;int test;}Init{name = \"null\";test = 0;}");
            parser.ParseFile("../../testCodeAttr.txt");
            parser.Exists(6);
            foreach (string v in parser.GetObjectNames())
            {
                //if (!parser.IDUsed(id)) continue;
                Console.WriteLine("\n<" + v + ">");
                Console.WriteLine(parser.GetAttribute<string>(v, "name"));
                byte[] test = parser.GetAttribute<byte[]>(v, "array1");
                for (int i = 0; i < test.Length; i++) Console.WriteLine(test[i]);
            }
            Console.ReadKey();
        }
    }
}
