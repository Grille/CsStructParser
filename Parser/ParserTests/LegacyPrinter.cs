using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grille.ConsoleTestLib;
using static System.Net.Mime.MediaTypeNames;

namespace ParserTests
{
    internal class LegacyPrinter : ITestPrinter
    {
        public void PrintSectionBegin(Section section)
        {
            if (section.TestCases.Count > 0)
            {
                Console.WriteLine(section.Name);
            }
        }

        public void PrintSectionEnd(Section section)
        {
            if (section.TestCases.Count > 0)
            {
                Console.WriteLine();
            }
        }

        public void PrintSummary(TestCounter counter)
        {
            Console.WriteLine("Executed tests: " + counter.Total);
            Console.WriteLine("ok: " + counter.Success + " | " + 100 * Math.Round((double)(counter.Success / counter.Total), 2) + "%");
            Console.WriteLine("fail: " + counter.Failure + " | " + 100 * Math.Round((double)(counter.Failure / counter.Total), 2) + "%");
            Console.WriteLine("error: " + counter.Error + " | " + 100 * Math.Round((double)(counter.Error / counter.Total), 2) + "%");

        }

        public void PrintTest(TestCase test)
        {
            switch (test.Status)
            {
                case TestStatus.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(test.Name + " OK");
                    break;
                case TestStatus.Failure:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(test.Name + " FAIL");
                    break;
                case TestStatus.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(test.Name + " ERROR");
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!string.IsNullOrEmpty(test.Message)) Console.Write(" -> " + test.Message);
            Console.WriteLine();
        }
    }
}
