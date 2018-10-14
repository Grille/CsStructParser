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
    //[Serializable]
    unsafe struct Point
    {
        fixed int h[2];

        public float x, y;
        public Point(float[] x, float y)
        {
            
            float* ptr = (float*)Marshal.AllocCoTaskMem(8);

            Marshal.FreeCoTaskMem((IntPtr)ptr);
            this.x = x[0];this.y = y;

            int[] lol = new int[20];
            
        }

        public int[] lol()
        {
            return new int[10];
        }
    }
    class Program
    {
        unsafe private static void Main(string[] args)
        {
            Test tests = new Test();
            tests.Run();
            
            /*
            string h = ""+67;
            Parser parser = new Parser();
            //parser.AddAttribute("int[]", "test2", "0");
            parser.AddEnum("res", "waste", 3);
            //parser.ParseCode("Attributes{string name;int test;}Init{name = \"null\";test = 0;}");
            parser.AddEnum("holly", new string[] { "fuck", "luck", "tree" });
            parser.ParseFile("../testCodeAttr.txt");
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
                
                Console.WriteLine("name: "+parser.GetAttribute<string>(v, "name"));
                Console.WriteLine("number: "+parser.GetAttribute<int>(v, "number"));
                Console.Write("array: ");
                byte[] test = parser.GetAttribute<byte[]>(v, "array");
                for (int i = 0; i < test.Length; i++) Console.Write(test[i]+" ");
                
            }


            Console.WriteLine("---------------<<<"); 

            Point point = new Point(new float[] { 4 },42);
            IntPtr ptr = new IntPtr(&point);
            byte[] data = new byte[8];
            Marshal.Copy(ptr, data, 0, data.Length);

            for (int i=0;i<data.Length;i++)
            data[i] = 200;

            Marshal.Copy(data, 0, ptr, data.Length);

            Console.WriteLine("x:"+point.x+",y:"+point.y);
            //Marshal.StructureToPtr(new int[9], ptr2, false);


            /*
            Console.WriteLine(Marshal.SizeOf(new int[99]));
            //Console.WriteLine(ptr);
            */
            Console.ReadKey();
        }
    }
}
#endif
