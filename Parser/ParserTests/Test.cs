
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGL.IO;

namespace ParserTests
{
    struct Ts
    {
        public int x;
    }
    class Test
    {
        Parser parser;
        private string text;
        public void Run()
        {
            int[] a1 = new int []{ 2, 3};
            int[] a2 = new int[] { 2, 3};
            Console.WriteLine(a1.Equals(a2));
            Console.WriteLine("Run tests...\n");

            parser = new Parser();
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nBasic tests");
            test("Add attribute", () =>
            {
                parser.AddAttribute("byte", "v", "0");
                if (parser.AttributeNames[0] == "v") printTest(0);
                else printTest(1);
            });
            test("Declare attribute: int v", () =>
            {
                parser.ParseCode("Attributes{int v}");
                if (parser.AttributeNames[0] == "v") printTest(0);
                else printTest(1);
            });

            test("Declare attribute list: int x,y", () =>
            {
                parser.ParseCode("Attributes{int x,y}");
                if (parser.AttributeNames[1] == "y") printTest(0);
                else printTest(1);
            });
            test("Declare objects by id: <0>{}", () =>
            {
                parser.ParseCode("Attributes{}<0>{}");
                if (parser.Exists(0)) printTest(0);
                else printTest(1);
            });
            test("Declare objects by name: <name>{}", () =>
            {
                parser.ParseCode("Attributes{}<foo>{}");
                if (parser.Exists("foo")) printTest(0);
                else printTest(1);
            });
            testSingleValue<int>("Define attribute: v=2", "Attributes{int v v=2}<0>{}", 0, 2);
            testSingleValue<byte>("Declare & Define attribute: int v=2", "Attributes{byte v=8}<0>{}", 0, 8);
            testSingleValue<byte>("Implizit attribute initialization: int v", "Attributes{byte v}<0>{}", 0, 0);
            test("Get struct by name: <name>{}", () =>
            {
                Ts ts; ts.x = 0;
                parser.ParseCode("Attributes{int x}<foo>{x=4}");
                parser.FillAttributes(ref ts, "foo");
                if (ts.x == 4) printTest(0);
                else printTest(1, "" + ts.x);
            });

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nType tests");
            testType<byte>("byte", "16", 16);
            testType<int>("int", "42", 42);
            testType<int>("int", "+42", +42);
            testType<int>("int", "-42", -42);
            testType<float>("float", "1.23", 1.23f);
            testType<float>("float", "-1.23", -1.23f);
            testType<float>("float", "1.23f", 1.23f);
            testType<double>("double", "2.46", 2.46d);
            testType<double>("double", "-2.46", -2.46d);
            testType<double>("double","2.46d", 2.46d);
            testType<bool>("bool", "false", false);
            testType<bool>("bool", "true", true);
            testType<bool>("bool", "0", false);
            testType<string>("string", "\"test\"", "test");
            testType<string>("string", "\"\"", "");

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nArray tests");
            testArray<byte>("byte[]", "[2,4]", 2, 4,2);
            testArray<int>("int[]", "[-4,8]", -4, 8, 2);
            testArray<int>("int[]", "[2 to 10]", 2, 3, 9);
            testArray<int>("int[]", "[0 to -4]", 0, -1, 5);
            testArray<int>("int[]", "[-8 to 2]", -8, -7, 11);
            testArray<float>("float[]", "[1.3,8.9]", 1.3f, 8.9f,2);
            testArray<float>("float[]", "[4 to 7]", 4, 5, 4);
            testArray<double>("double[]", "[2.6,17.8]", 2.6, 17.8, 2);
            testArray<bool>("bool[]", "[false,true]", false, true, 2);
            testArray<string>("string[]", "[foo,baa]", "foo", "baa", 2);
            test("byte[] [e.a,e.b]", () =>
            {
                parser.AddEnum("e", new string[2] { "a", "b" });
                parser.ParseCode("Attributes{byte[] a = [e.a,e.b]}<a>{}");
                byte[] v = parser.GetAttribute<byte[]>("a", "a");
                if (v[0] == 0 && v[1] == 1) printTest(0);
                else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            test("byte[] [e.a to e.b]", () =>
            {
                parser.AddEnum("e", new string[2] { "a", "b" });
                parser.ParseCode("Attributes{byte[] a = [e.a to e.b]}<a>{}");
                byte[] v = parser.GetAttribute<byte[]>("a", "a");
                if (v[0] == 0 && v[1] == 1) printTest(0);
                else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            test("byte[] [&a,&b]", () =>
            {
                parser.ParseCode("Attributes{byte[] a = [&a,&b]}<a>{}<b>{}");
                byte[] v = parser.GetAttribute<byte[]>("a", "a");
                if (v[0] == 0&& v[1]==1) printTest(0);
                else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            test("byte[] [&a to &b]", () =>
            {
                parser.ParseCode("Attributes{byte[] a = [&a to &b]}<a>{}<b>{}");
                byte[] v = parser.GetAttribute<byte[]>("a", "a");
                if (v[0] == 0 && v[1] == 1) printTest(0);
                else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            test("empty array []", () =>
            {
                parser.ParseCode("Attributes{byte[] a = []}<0>{}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v != null && v.Length == 0) printTest(0);
                else printTest(1);
            });
            test("interleaved array [[2],4]", () =>
            {
                parser.ParseCode("Attributes{byte[] a = [[2],4]}<0>{}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4) printTest(0);
                else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            test("multiple arrays", () =>
            {
                parser.ParseCode("Attributes{byte[] a = [1] int[] x = [],y=[2 to 6]}<0>{}");
                int[] v = parser.GetAttribute<int[]>(0, "y");
                if (v != null && v.Length == 5) printTest(0);
                else printTest(1, "" + v.Length);
            });

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nEnum tests");

            test("Add/use single enum", () =>
            {
                parser.AddEnum("e", "x", 5);
                parser.ParseCode("Attributes{int v=e.x}<0>{}");
                if (parser.GetAttribute<int>(0, "v") == 5) printTest(0);
                else printTest(1);
            });
            test("Add/use enum list", () =>
            {
                parser.AddEnum("e", new string[] { "x", "y" });
                parser.ParseCode("Attributes{int x=e.x,y=e.y}<0>{}");
                if (parser.GetAttribute<int>(0, "x") == 0 && parser.GetAttribute<int>(0, "y") == 1) printTest(0);
                else printTest(1);
            });

            test("Define enum: enum e{x,y}", () =>
            {
                parser.ParseCode("enum e{x,y} Attributes{int x=e.x,y=e.y}<0>{}");
                if (parser.GetAttribute<int>(0, "x") == 0 && parser.GetAttribute<int>(0, "y") == 1) printTest(0);
                else printTest(1);
            });
            test("Define enum: enum e{x=8,y=-4}", () =>
            {
                parser.ParseCode("enum e{x=8,y=-4} Attributes{int x=e.x,y=e.y}<0>{}");
                if (parser.GetAttribute<int>(0, "x") == 8 && parser.GetAttribute<int>(0, "y") == -4) printTest(0);
                else printTest(1);
            });
            test("Define multiple enums: enum e{x} enum v{y}", () =>
            {
                parser.ParseCode("enum e{x=2} enum v{y=4} Attributes{int x=e.x,y=v.y}<0>{}");
                if (parser.GetAttribute<int>(0, "x") == 2 && parser.GetAttribute<int>(0, "y") == 4) printTest(0);
                else printTest(1, parser.GetAttribute<int>(0, "x") + ":" + parser.GetAttribute<int>(0, "y"));
            });
            
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nObject tests");
            test("Define object", () =>
            {
                parser.ParseCode("Attributes{int x,y}<0>{x=4 y=8}");
                if (parser.GetAttribute<int>(0, "x") == 4 && parser.GetAttribute<int>(0, "y") == 8) printTest(0);
                else printTest(1);
            });
            test("Define multiple objects", () =>
            {
                parser.ParseCode("Attributes{int v}<0>{v=1}<1>{v=2}");
                if (parser.GetAttribute<int>(0, "v") == 1 && parser.GetAttribute<int>(1, "v") == 2) printTest(0);
                else printTest(1);
            });
            test("Get object id", () =>
            {
                parser.ParseCode("Attributes{int p=&b}<a>{}<b>{}");
                if (parser.GetAttribute<int>("a", "p") == 1) printTest(0);
                else printTest(1);
            });
            test("Object inheritance: foo.x=42 baa.x:foo.x", () =>
            {
                parser.ParseCode("Attributes{byte x=0}<1>{x=42}<0>:1{}");
                int v = parser.GetAttribute<byte>(0, "x");
                if (v == 42) printTest(0);
                else printTest(1, "" + v);
            });
            test("Object inheritance: baa.x:foo.x foo.x=42", () =>
            {
                parser.ParseCode("Attributes{byte x=0}<0>:1{}<1>{x=42}");
                int v = parser.GetAttribute<byte>(0, "x");
                if (v == 42) printTest(0);
                else printTest(1, "" + v);
            });
            test("Object array", () =>
            {
                parser.ParseCode("Attributes{byte[]a}<0>{a=[2,4]}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4) printTest(0);
                else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            test("Object empty array", () =>
            {
                parser.ParseCode("Attributes{byte[]a}<0>{a=[]}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v != null && v.Length == 0) printTest(0);
                else printTest(1);
            });
            test("Object enclosed array", () =>
            {
                parser.ParseCode("Attributes{int v byte[]a}<0>{v=2 a=[1,3,6]v=1}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v[1] == 3 && v.Length == 3) printTest(0);
                else printTest(1);
            });
            test("Object interleaved array", () =>
            {
                parser.ParseCode("Attributes{byte[]a}<0>{a=[[2],4]}");
                byte[] v = parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4 && v.Length == 2) printTest(0);
                else printTest(1);
            });
            test("Object array inheritance: foo:array+baa.array", () =>
             {
                 parser.ParseCode("Attributes{byte[]a=[2]}<0>{a+[4]}");
                 byte[] v = parser.GetAttribute<byte[]>(0, "a");
                 if (v[0] == 2 && v[1] == 4) printTest(0);
                 else printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
             });
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Console.WriteLine("\nException tests");
            testExeption("circular reference", "Attributes{}<0>:1{}<1>:0{}", null);
            testExeption("heritage from undefined", "Attributes{}<0>:1{}", null);
            testExeption("undeclared attribute access", "Attributes{v=0;}<0>{}", null);
            testExeption("incompatible value", "Attributes{int v=\"x\";}<0>{}", null);
        }

        
        private void test(string name, Action method)
        {
            text = name;
            try
            {
                method();
            }
            catch (Exception e) { printTest(2, e.Message); }
            parser.Clear();
        }
        private void testSingleValue<T>(string name, string code, int id, T expect)
        {
            test(name, () =>
            {
                parser.ParseCode(code);
                if (Convert.ToString(parser.GetAttribute<T>(id, "v")) == Convert.ToString(expect)) printTest(0);
                else printTest(1);
            });
        }

        private void testType<T>(string typ,string num,T expect)
        {
            test(typ + " " + num, () =>
            {
                parser.ParseCode("Attributes{" + typ + " v=" + num + "}<0>{}");
                T v = (T)parser.GetAttribute<T>(0, "v");
                if (Convert.ToString(v) == Convert.ToString(expect))
                    printTest(0);
                else
                    printTest(1, "" + v);
            });
        }
        private void testArray<T>(string typ, string num, T expect0,T expect1,int length)
        {
            test(typ + " " + num, () =>
            {
                parser.ParseCode("Attributes{" + typ + " v=" + num + "}<0>{}");
                T[] v = parser.GetAttribute<T[]>(0, "v");
                if (Convert.ToString(v[0]) == Convert.ToString(expect0) && Convert.ToString(v[1]) == Convert.ToString(expect1) && v.Length == length)
                    printTest(0);
                else
                    printTest(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
        }

        private void testExeption(string name,string code,string expect)
        {
            text = name;
            try
            {
                parser.ParseCode(code);
                printTest(1);
            }
            catch (Exception e)
            {
                if (expect != null)
                    if (e.Message.ToLower() == ("line 1: " + expect).ToLower()) printTest(0);
                    else printTest(2, e.Message);
                else if (e.Message.ToLower().Contains("line 1:")) printTest(0, e.Message);
                else printTest(2, e.Message);

            }
            parser.Clear();
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

