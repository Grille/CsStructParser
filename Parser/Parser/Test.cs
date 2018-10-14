#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGL.IO;

namespace Tests
{

    class Test
    {
        Parser parser;
        private string text;
        public void Run()
        {
            Console.WriteLine("Run tests...\n");

            parser = new Parser();
            
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nBasic tests");
            setTest("Add attribute");//----------------------------------
            try
            {
                parser.AddAttribute("byte", "name", "0");
                if (parser.GetAttributeNames()[0] == "name") printTest(0);
                else printTest(1);
            }
            
            catch (Exception e){printTest(2, e.Message);}
            parser.Clear();
            setTest("Declare attribute: int v");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte name}");
                if (parser.GetAttributeNames()[0] == "name") printTest(0);
                else printTest(1);
            }
            catch (Exception e){printTest(2,e.Message);}
            parser.Clear();
            setTest("Declare attribute list: int x,y");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte foo,baa}");
                if (parser.GetAttributeNames()[1] == "baa") printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Declare objects by id: <0>{}");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{}<0>{}");
                if (parser.Exists(0)) printTest(0);
                else printTest(1);
            }
            catch (Exception e){printTest(2, e.Message);}
            parser.Clear();
            setTest("Declare objects by name: <name>{}");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{}<foo>{}");
                if (parser.Exists("foo")) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Define attribute: v=2");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{int v v=2}<0>{}");
                if (parser.GetAttribute<int>(0, "v") == 2) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Declare & Define attribute: int v=2");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte b=8}<0>{}");
                if (parser.GetAttribute<byte>(0, "b") == 8) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Implizit attribute definition: int v");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte v}<0>{}");
                if (parser.GetAttribute<byte>(0, "v") == 0) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nType tests");
            testType<byte>("byte", "16", 16);
            testType<int>("int", "42", 42);
            testType<int>("int", "-42", -42);
            testType<float>("float", "1.23", 1.23f);
            testType<float>("float", "-1.23", -1.23f);
            testType<float>("float", "1.23f", 1.23f);
            testType<double>("double", "2.46", 2.46d);
            testType<double>("double", "-2.46", -2.46d);
            testType<double>("double","2.46d", 2.46d);
            testType<bool>("bool", "false", false);
            testType<bool>("bool", "true", true);
            testType<string>("string", "\"test\"", "test");
            testType<string>("string", "\"\"", "");

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nArray tests");
            testArray<byte>("byte[]", "[2,4]", 2, 4,2);
            testArray<int>("int[]", "[-4,8]", -4, 8, 2);
            testArray<int>("int[]", "[2 to 10]", 2, 3, 9);
            testArray<int>("int[]", "[0 to -4]", 0, -1, 5);
            testArray<float>("float[]", "[1.3,8.9]", 1.3f, 8.9f,2);
            testArray<float>("float[]", "[4 to 7]", 4, 5, 4);
            testArray<double>("double[]", "[2.6,17.8]", 2.6, 17.8, 2);
            testArray<bool>("bool[]", "[false,true]", false, true, 2);
            testArray<string>("string[]", "[foo,baa]", "foo", "baa", 2);
            setTest("empty array");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte[] a = []}<0>{}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v!= null&&v.Length==0) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("wtf array");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte[] a = [[2],4]}<0>{}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1]==4) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("multiple arrays");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte[] a = [1] int[] x = [],y=[2 to 6]}<0>{}");
                int[] v = parser.GetAttribute<int[]>(0, "y");
                if (v != null && v.Length == 5) printTest(0);
                else printTest(1,""+v.Length);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nEnum tests");
            setTest("Add/use single enum");//----------------------------------
            try
            {
                parser.AddEnum("e", "x",5);
                parser.ParseCode("Attributes{int v=e.x}<0>{}");
                if (parser.GetAttribute<int>(0, "v") == 5) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Add/use enum list");//----------------------------------
            try
            {
                parser.AddEnum("e", new string[] { "x", "y" });

                parser.ParseCode("Attributes{int x=e.x,y=e.y}<0>{}");
                if (parser.GetAttribute<int>(0, "x") == 0 && parser.GetAttribute<int>(0, "y") == 1) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Define/use enum: enum e{x,y}");//----------------------------------
            try
            {
                parser.ParseCode("enum e{x,y} Attributes{int x=e.x,y=e.y}<0>{}");
                if (parser.GetAttribute<int>(0, "x") == 0 && parser.GetAttribute<int>(0, "y") == 1) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Define/use enum: enum e{x=8,y=-4}");//----------------------------------
            try
            {
                parser.ParseCode("enum e{x=8,y=-4} Attributes{int x=e.x,y=e.y}<0>{}");
                if (parser.GetAttribute<int>(0, "x") == 8 && parser.GetAttribute<int>(0, "y") == -4) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nObject tests");
            setTest("Define object");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{int x,y}<0>{x=4 y=8}");
                if (parser.GetAttribute<int>(0, "x") == 4 && parser.GetAttribute<int>(0, "y") == 8) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Define multiple objects");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{int v}<0>{v=1}<1>{v=2}");
                if (parser.GetAttribute<int>(0, "v") == 1 && parser.GetAttribute<int>(1, "v") == 2) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Object inheritance: foo.x=42 baa.x:foo.x");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte x=0}<1>{x=42}<0>:1{}");
                int v = parser.GetAttribute<byte>(0, "x");
                if (v == 42) printTest(0);
                else printTest(1, "" + v);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Object inheritance: baa.x:foo.x foo.x=42");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte x=0}<0>:1{}<1>{x=42}");
                int v = parser.GetAttribute<byte>(0, "x");
                if (v == 42) printTest(0);
                else printTest(1, "" + v);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Object array");//----------------------------------
            try
            {

                parser.ParseCode("Attributes{byte[]a}<0>{a=[2,4]}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4) printTest(0);
                else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Object empty array");//----------------------------------
            try
            {
                
                parser.ParseCode("Attributes{byte[]a}<0>{a=[]}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v != null && v.Length == 0) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Object wtf array");//----------------------------------
            try
            {

                parser.ParseCode("Attributes{byte[]a}<0>{a=[[2]],4}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4 && v.Length == 2) printTest(0);
                else printTest(1);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
            setTest("Object array inheritance: foo:array+baa.array");//----------------------------------
            try
            {
                parser.ParseCode("Attributes{byte[]a=[2]}<0>{a+[4]}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4) printTest(0);
                else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
        }


        private void testType<T>(string typ,string num,T expect)
        {
            setTest(typ+" "+num);
            try
            {
                parser.ParseCode("Attributes{"+ typ + " v=" + num + "}<0>{}");
                T v = (T)parser.GetAttribute<T>(0, "v");
                
                if (Convert.ToString(v) == Convert.ToString(expect))
                    printTest(0);
                else
                    printTest(1, "" + v);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
        }
        private void testArray<T>(string typ, string num, T expect0,T expect1,int length)
        {
            setTest(typ + " " + num);
            try
            {
                parser.ParseCode("Attributes{" + typ + " v=" + num + "}<0>{}");
                T[] v = parser.GetAttribute<T[]>(0, "v");
                if (Convert.ToString(v[0]) == Convert.ToString(expect0) && Convert.ToString(v[1]) == Convert.ToString(expect1) && v.Length == length)
                    printTest(0);
                else
                    printTest(1, "[" + v[0]+","+v[1]+"]."+ v.Length);
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
        }
        private void setTest(string text)
        {
            this.text = text;
        }
        private void printTest(int state)
        {
            printTest(state, null);
        }
        private void printTest(int state,string message)
        {
            switch (state)
            {
                case 0:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(text + " OK");
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(text + " FAIL");
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(text + " ERROR");
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            if (message != null) Console.Write(" -> "+message);
            Console.WriteLine();
        }
    }
}
#endif
