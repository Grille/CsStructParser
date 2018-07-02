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
            Parser phaser = new Parser();
            phaser.LoadData("../../gameObject.gd");
            phaser.Parse();
            for (int id = 0; id < 256; id++)
            {
                if (!phaser.IDUsed(id)) continue;
                Console.WriteLine("\n<objectID " + id + ">");
                Console.WriteLine(phaser.GetAttribute<string>(id, "name"));
                
                /*
                byte[] array = phaser.GetAttribute<byte[]>(id, "canBuiltOn");
                for (int i = 0; i < array.Length; i++)
                {
                    Console.Write(array[i]);
                }
                */
                
            }
            Console.ReadKey();
        }
    }
}
