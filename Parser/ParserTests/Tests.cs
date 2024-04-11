
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grille.Parsing.Tcf;
using Grille.ConsoleTestLib;

using static ParserTests.TestSystem;
using static Grille.ConsoleTestLib.Asserts;

namespace ParserTests
{
    struct Ts
    {
        public int x;
    }
    class Tests
    {
        public void Run()
        {
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Section("Basic tests");
            Test("Add attribute", () =>
            {
                Parser.AddAttribute("byte", "v", "0");
                if (Parser.DefaultType.Properties[0].Name == "v") Result(0);
                else Result(1);
            });
            Test("Declare attribute: int v", () =>
            {
                Parser.ParseCode("Attributes{int v}");
                if (Parser.DefaultType.Properties[0].Name == "v") Result(0);
                else Result(1);
            });

            Test("Declare attribute list: int x,y", () =>
            {
                Parser.ParseCode("Attributes{int x,y}");
                if (Parser.DefaultType.Properties[1].Name == "y") Result(0);
                else Result(1);
            });
            Test("Declare objects by id: <0>{}", () =>
            {
                Parser.ParseCode("Attributes{}<0>{}");
                if (Parser.Exists(0)) Result(0);
                else Result(1);
            });
            Test("Declare objects by name: <name>{}", () =>
            {
                Parser.ParseCode("Attributes{}<foo>{}");
                if (Parser.Exists("foo")) Result(0);
                else Result(1);
            });
            TestSingleValue<int>("Define attribute: v=2", "Attributes{int v v=2}<0>{}", 0, 2);
            TestSingleValue<byte>("Declare & Define attribute: int v=2", "Attributes{byte v=8}<0>{}", 0, 8);
            Test("Fill attributes()", () =>
            {
                Ts ts; ts.x = 0;
                Parser.ParseCode("Attributes{int x}<foo>{x=4}");
                Parser.FillAttributes(ref ts, "foo");
                if (ts.x == 4) Result(0);
                else Result(1, "" + ts.x);
            });

            Section("Syntax tests");
            Test("Next token after string end not cut", () =>
            {
                Ts ts; ts.x = 0;
                Parser.ParseCode("Attributes{string x=\"xx\",y=\"yy\"}<0>{}");
                string x = Parser.GetAttribute<string>(0, "x"), y = Parser.GetAttribute<string>(0, "y");
                if (x == "xx" && y == "yy") Result(0);
                else Result(1, "x:" + y + " y:" + y);
            });

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Section("Type tests");
            TestType<byte>("byte", 0);
            TestType<byte>("byte", "16", 16);
            TestType<int>("int", 0);
            TestType<int>("int", "42", 42);
            TestType<int>("int", "+42", +42);
            TestType<int>("int", "-42", -42);
            TestType<float>("float", 0f);
            TestType<float>("float", "1.23", 1.23f);
            TestType<float>("float", "-1.23", -1.23f);
            TestType<float>("float", "1.23f", 1.23f);
            TestType<double>("double", 0d);
            TestType<double>("double", "2.46", 2.46d);
            TestType<double>("double", "-2.46", -2.46d);
            TestType<double>("double","2.46d", 2.46d);
            TestType<bool>("bool", false);
            TestType<bool>("bool", "false", false);
            TestType<bool>("bool", "true", true);
            TestType<bool>("bool", "0", false);
            TestType<string>("string", "");
            TestType<string>("string", "\"\"", "");
            TestType<string>("string", "\"test\"", "test");
            TestType<string>("string", "\"-\\\"\\n-\"", "-\"\n-");
            TestType<string>("script", "(){foo}", "foo");
            TestType<string>("script", "(){foo{baa}}", "foo{baa}");
            TestType<int>("ref", 0);
            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Section("Array tests");
            TestArray<byte>("byte[]", "[2,4]", 2, 4,2);
            TestArray<int>("int[]", "[-4,8]", -4, 8, 2);
            TestArray<int>("int[]", "[2 to 10]", 2, 3, 9);
            TestArray<int>("int[]", "[0 to -4]", 0, -1, 5);
            TestArray<int>("int[]", "[-8 to 2]", -8, -7, 11);
            TestArray<float>("float[]", "[1.3,8.9]", 1.3f, 8.9f,2);
            TestArray<float>("float[]", "[4 to 7]", 4, 5, 4);
            TestArray<double>("double[]", "[2.6,17.8]", 2.6, 17.8, 2);
            TestArray<bool>("bool[]", "[false,true]", false, true, 2);
            TestArray<bool>("bool[]", "[0,1]", false, true, 2);
            TestArray<string>("string[]", "[\"foo\",\"baa\"]", "foo", "baa", 2);

            Test("empty array []", () =>
            {
                Parser.ParseCode("Attributes{byte[] a = []}<0>{}");
                byte[] v = Parser.GetAttribute<byte[]>(0, "a");
                if (v != null && v.Length == 0) Result(0);
                else Result(1);
            });
            Test("array/enum byte[] [e.a,e.b]", () =>
            {
                Parser.AddEnum("e", new string[2] { "a", "b" });
                Parser.ParseCode("Attributes{byte[] a = [e.a,e.b]}<a>{}");
                byte[] v = Parser.GetAttribute<byte[]>("a", "a");
                if (v[0] == 0 && v[1] == 1) Result(0);
                else Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            Test("array/enum byte[] [e.a to e.b]", () =>
            {
                Parser.AddEnum("e", new string[2] { "a", "b" });
                Parser.ParseCode("Attributes{byte[] a = [e.a to e.b]}<a>{}");
                byte[] v = Parser.GetAttribute<byte[]>("a", "a");
                if (v[0] == 0 && v[1] == 1) Result(0);
                else Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            Test("array/ref byte[] [&a,&b]", () =>
            {
                Parser.ParseCode("Attributes{byte[] a}<a>{a = [&a,&b]}<b>{}");
                byte[] v = Parser.GetAttribute<byte[]>("a", "a");
                if (v[0] == 0&& v[1]==1) Result(0);
                else Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            Test("array/ref byte[] [&a to &b]", () =>
            {
                Parser.ParseCode("Attributes{byte[] a}<a>{a=[&a to &b]}<b>{}");
                byte[] v = Parser.GetAttribute<byte[]>("a", "a");
                if (v[0] == 0 && v[1] == 1) Result(0);
                else Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            Test("array ref[] [obj1,obj2]", () =>
            {
                Parser.ParseCode("Attributes{ref[] v}<0>{v=[obj1,obj2]}<obj1>{}<obj2>{}");
                if (Parser.GetAttribute<int[]>(0, "v")[1] == 2) Result(0);
                else Result(1);
            });
            Test("array ref[] [obj1 to obj3]", () =>
            {
                Parser.ParseCode("Attributes{ref[] v}<0>{v=[obj1 to obj3]}<obj1>{}<obj2>{}<obj3>{}");
                if (Parser.GetAttribute<int[]>(0, "v")[1] == 2) Result(0);
                else Result(1);
            });
            Test("interleaved array [[2],4]", () =>
            {
                Parser.ParseCode("Attributes{byte[] a = [[2],4]}<0>{}");
                byte[] v = Parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4) Result(0);
                else Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            Test("multiple arrays", () =>
            {
                Parser.ParseCode("Attributes{byte[] a = [1] int[] x = [],y=[2 to 6]}<0>{}");
                int[] v = Parser.GetAttribute<int[]>(0, "y");
                if (v != null && v.Length == 5) Result(0);
                else Result(1, "" + v.Length);
            });

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Section("Enum tests");

            Test("Add/use single enum", () =>
            {
                Parser.AddEnum("e", "x", 5);
                Parser.ParseCode("Attributes{int v=e.x}<0>{}");
                if (Parser.GetAttribute<int>(0, "v") == 5) Result(0);
                else Result(1);
            });
            Test("Add/use enum list", () =>
            {
                Parser.AddEnum("e", new string[] { "x", "y" });
                Parser.ParseCode("Attributes{int x=e.x,y=e.y}<0>{}");
                if (Parser.GetAttribute<int>(0, "x") == 0 && Parser.GetAttribute<int>(0, "y") == 1) Result(0);
                else Result(1);
            });

            Test("Define enum: enum e{x,y}", () =>
            {
                Parser.ParseCode("enum e{x,y} Attributes{int x=e.x,y=e.y}<0>{}");
                if (Parser.GetAttribute<int>(0, "x") == 0 && Parser.GetAttribute<int>(0, "y") == 1) Result(0);
                else Result(1);
            });
            Test("Define enum: enum e{x=8,y=-4}", () =>
            {
                Parser.ParseCode("enum e{x=8,y=-4} Attributes{int x=e.x,y=e.y}<0>{}");
                if (Parser.GetAttribute<int>(0, "x") == 8 && Parser.GetAttribute<int>(0, "y") == -4) Result(0);
                else Result(1);
            });
            Test("Define multiple enums: enum e{x} enum v{y}", () =>
            {
                Parser.ParseCode("enum e{x=2} enum v{y=4} Attributes{int x=e.x,y=v.y}<0>{}");
                if (Parser.GetAttribute<int>(0, "x") == 2 && Parser.GetAttribute<int>(0, "y") == 4) Result(0);
                else Result(1, Parser.GetAttribute<int>(0, "x") + ":" + Parser.GetAttribute<int>(0, "y"));
            });
            Test("Empty enum", () =>
            {
                Parser.ParseCode("enum e {}");
            });
            Test("const", () =>
            {
                Parser.ParseCode("const a = 4");
                AssertValueIsEqual(Parser.Constants["a"], 4);
            });

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Section("Types");
            Test("Define Class", () =>
            {
                Parser.ParseCode("typedef Class{}");

                if (Parser.Types.Count != 1) Fail();
                if (!Parser.Types.ContainsKey("Class")) Fail();
            });
            Test("Define C1 & C2", () =>
            {
                Parser.ParseCode("typedef C1{} typedef C2{}");

                if (Parser.Types.Count != 2) Fail();
                if (!Parser.Types.ContainsKey("C1")) Fail();
                if (!Parser.Types.ContainsKey("C2")) Fail();
            });
            Test("Define Class{int v}", () =>
            {
                Parser.ParseCode("typedef Class{int v}");

                if (!Parser.Types.ContainsKey("Class")) Fail();
                var prop = Parser.Types["Class"].Properties;
                if (prop.Count != 1) Fail();
                if (prop[0].Name != "v") Fail();
            });
            Test("Define and use C c", () =>
            {
                Parser.ParseCode("typedef C{int v = 42} C c{}");

                AssertValueIsEqual(Parser.Result["c"].Get<int>("v"), 42);
            });
            Test("Complex test", () =>
            {
                Parser.ParseCode("Attributes {int z} typedef C{int v} \n typedef Point{int x; int y} \n C c1{v=4} \n C c2{v=8} \n Point p {x=12;y=42;} <a>{z=66}");

                AssertValueIsEqual(Parser.Result["c1"].Get<int>("v"), 4);
                AssertValueIsEqual(Parser.Result["c2"].Get<int>("v"), 8);

                AssertValueIsEqual(Parser.Result["p"].Get<int>("x"), 12);
                AssertValueIsEqual(Parser.Result["p"].Get<int>("y"), 42);

                AssertValueIsEqual(Parser.Result["a"].Get<int>("z"), 66);
            });

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Section("Object tests");
            Test("Define object", () =>
            {
                Parser.ParseCode("Attributes{int x,y}<0>{x=4 y=8}");
                if (Parser.GetAttribute<int>(0, "x") == 4 && Parser.GetAttribute<int>(0, "y") == 8) Result(0);
                else Result(1);
            });
            Test("Define multiple objects", () =>
            {
                Parser.ParseCode("Attributes{int v}<0>{v=1}<1>{v=2}");
                if (Parser.GetAttribute<int>(0, "v") == 1 && Parser.GetAttribute<int>(1, "v") == 2) Result(0);
                else Result(1);
            });
            Test("Get object id", () =>
            {
                Parser.ParseCode("Attributes{int p}<a>{p=&b}<b>{}");
                if (Parser.GetAttribute<int>("a", "p") == 1) Result(0);
                else Result(1);
            });
            Test("Object array", () =>
            {
                Parser.ParseCode("Attributes{byte[]a}<0>{a=[2,4]}");
                byte[] v = Parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4) Result(0);
                else Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            Test("Object empty array", () =>
            {
                Parser.ParseCode("Attributes{byte[]a}<0>{a=[]}");
                byte[] v = Parser.GetAttribute<byte[]>(0, "a");
                if (v != null && v.Length == 0) Result(0);
                else Result(1);
            });
            Test("Object enclosed array", () =>
            {
                Parser.ParseCode("Attributes{int v byte[]a}<0>{v=2 a=[1,3,6]v=1}");
                byte[] v = Parser.GetAttribute<byte[]>(0, "a");
                if (v[1] == 3 && v.Length == 3) Result(0);
                else Result(1);
            });
            Test("Object interleaved array", () =>
            {
                Parser.ParseCode("Attributes{byte[]a}<0>{a=[[2],4]}");
                byte[] v = Parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4 && v.Length == 2) Result(0);
                else Result(1);
            });
            Test("Object enum as object name", () =>
            {
                Parser.ParseCode("<enum>{}");
                if (Parser.Result.Count == 1 && Parser.Result.ContainsKey("enum")) Result(0);
                else Result(1);
            });
            Test("Object enum as field name", () =>
            {
                Parser.ParseCode("Attributes{int enum}<0>{enum = 6}");
                int v = Parser.GetAttribute<int>(0, "enum");
                if (v == 6) Result(0);
                else Result(1);
            });

            Section("Inheritance tests");
            Test("Object inheritance: a.x=42 a.x:b.x", () =>
            {
                Parser.ParseCode("Attributes{byte x=0}<1>{x=42}<0>:1{}");
                int v = Parser.GetAttribute<byte>(0, "x");
                if (v == 42) Result(0);
                else Result(1, "" + v);
            });
            Test("Object inheritance: a.x:b.x b.x=42", () =>
            {
                Parser.ParseCode("Attributes{byte x=0}<0>:1{}<1>{x=42}");
                int v = Parser.GetAttribute<byte>(0, "x");
                if (v == 42) Result(0);
                else Result(1, "" + v);
            });
            Test("Object int array inheritance: a:array + b.array", () =>
            {
                Parser.ParseCode("Attributes{byte[]a=[2]}<0>{a+[4]}");
                byte[] v = Parser.GetAttribute<byte[]>(0, "a");
                if (v[0] == 2 && v[1] == 4) Result(0);
                else Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
            Test("Object string array inheritance: a:array + b.array", () =>
            {
                Parser.ParseCode("Attributes{string[]a=[\"x\"]}<0>{a+[\"y\"]}");
                string[] v = Parser.GetAttribute<string[]>(0, "a");
                if (v[0] == "x" && v[1] == "y") Result(0);
                else Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            Section("Exception tests");
            TestExeption("circular reference", "Attributes{}<0>:1{}<1>:0{}");
            TestExeption("heritage from undefined", "Attributes{}<0>:1{}");
            TestExeption("undeclared attribute access", "Attributes{v=0;}<0>{}");
            TestExeption("undeclared enum access", "Attributes{int v;}<0>{v=enum.value}");
            TestExeption("incompatible value", "Attributes{int v=\"x\";}<0>{}");
            TestExeption("empety array", "Attributes{int[] array;}<0>{array + [8,0]}");
            TestExeption("empety array", "enum v");
            TestExeption("empety array", "<0>{");
            TestExeption("inherent wrong type", "typedef C1{} typedef C2{} C1 c1:c2{} C2 c2{}");

            Section("File tests");
            Test("Load And Parse File", () =>
            {
                Parser.ParseFile("test.txt");
                if (Parser.GetAttribute<string>(0, "name") != "obj-0") Result(1);
                if (Parser.GetAttribute<int>(0, "num") != 2) Result(1);
                Result(0);
            });

            RunTests();
        }
    }
}

