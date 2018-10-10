using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGL.IO;

#if DEBUG
namespace Tests
{

    class Test
    {
        public void Run()
        {
            Console.WriteLine("run tests\n");

            Parser parser = new Parser();

            parser.ParseCode("Attributes{string name;int test;}Init{name = \"null\";test = 0;}");
        }
    }
}
#endif
