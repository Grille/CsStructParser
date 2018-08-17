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
            Parser parser = new Parser();
            parser.AddEnum("res", "waste", 3);
            parser.ParseCode("Attributes{string name;int test;}Init{name = \"null\";test = 0;}");
            //parser.ParseFile("../../testCodeAttr.txt");
            parser.ParseFile("../../testCodeData.txt");
            for (int id = 0; id < 256; id++)
            {
                if (!parser.IDUsed(id)) continue;
                Console.WriteLine("\n<objectID " + id + ">");
                Console.WriteLine(parser.GetAttribute<string>(id, "name"));
                Console.WriteLine(parser.GetAttribute<int>(id, "test"));
            }
            Console.ReadKey();
        }
    }
}
