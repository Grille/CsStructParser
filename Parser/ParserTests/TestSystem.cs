using Grille.ConsoleTestLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grille.Parsing.Tcf;
using static Grille.ConsoleTestLib.Asserts;

namespace ParserTests
{
    internal static class TestSystem
    {
        public static TcfParser Parser { get; private set; }

        static TestSystem()
        {
            GlobalTestSystem.ExecuteImmediately = true;
            GlobalTestSystem.RethrowExceptions = false;
            GlobalTestSystem.RethrowFailed = false;
            GlobalTestSystem.Printer = new LegacyPrinter();

            Console.WriteLine("Run tests...\n");
        }

        public static void Test(string name, Action method)
        {
            Parser = new TcfParser();
            GlobalTestSystem.Test(name, method);
        }
        public static void TestSingleValue<T>(string name, string code, int id, T expect)
        {
            Test(name, () =>
            {
                Parser.ParseCode(code);
                if (Convert.ToString(Parser.GetAttribute<T>(id, "v")) == Convert.ToString(expect)) Result(0);
                else Result(1);
            });
        }

        public static void TestType<T>(string typ, T expect)
        {
            Test(typ, () =>
            {
                Parser.ParseCode("Attributes{" + typ + " v}<0>{}");
                T v = Parser.GetAttribute<T>(0, "v");
                if (Convert.ToString(v) == Convert.ToString(expect))
                    Result(0);
                else
                    Result(1, "'" + v + "'");
            });
        }
        public static void TestType<T>(string typ, string num, T expect)
        {
            Test(typ + " " + num, () =>
            {
                Parser.ParseCode("Attributes{" + typ + " v=" + num + "}<0>{}");
                T v = (T)Parser.GetAttribute<T>(0, "v");
                if (Convert.ToString(v) == Convert.ToString(expect))
                    Result(0);
                else
                    Result(1, "'" + v + "'");
            });
        }
        public static void TestArray<T>(string typ, string num, T expect0, T expect1, int length)
        {
            Test(typ + " " + num, () =>
            {
                Parser.ParseCode("Attributes{" + typ + " v=" + num + "}<0>{}");
                T[] v = Parser.GetAttribute<T[]>(0, "v");
                if (Convert.ToString(v[0]) == Convert.ToString(expect0) && Convert.ToString(v[1]) == Convert.ToString(expect1) && v.Length == length)
                    Result(0);
                else
                    Result(1, "[" + v[0] + "," + v[1] + "]." + v.Length);
            });
        }

        public static void TestExeption(string name, string code) => TestExeption(name, code, null);

        public static void TestExeption(string name, string code, string expect)
        {
            GlobalTestSystem.Test(name, () =>
            {
                var parser = new TcfParser();
                var e = AssertException<TcfParserException>(() =>
                {
                    parser.ParseCode(code);
                });
                if (expect != null)
                    if (e.Message.ToLower() == ("line 1: " + expect).ToLower()) Result(0);
                    else Result(2, e.Message);
                else if (e.Message.ToLower().Contains("line 1:")) Result(0, e.Message);
                else Result(2, e.Message);
            });
        }

        public static void Section(string name)
        {
            GlobalTestSystem.Section(name);
        }

        public static void Result(int state)
        {
            Result(state, string.Empty);
        }

        public static void Result(int state, string message)
        {
            switch (state)
            {
                case 0:
                    Succes(message);
                    break;
                case 1:
                    Fail(message);
                    break;
                case 2:
                    Fail(message);
                    break;
            }
        }

        public static void RunTests()
        {
            GlobalTestSystem.RunTestsSynchronously();
        }

    }
}
