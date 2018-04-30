using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programm
{
    class Program
    {
        static void Main(string[] args)
        {
            Pharser phaser = new Pharser();
            phaser.LoadData("../../testcode.txt");
            phaser.Pharse();
            phaser.GetData();
           // Console.WriteLine(phaser.getData());
            Console.ReadKey();
        }
    }
}
