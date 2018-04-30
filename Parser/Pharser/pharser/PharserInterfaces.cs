using System.IO;
using System;
namespace Programm
{
    public partial class Pharser
    {
        public bool Enabeld(int index)
        {
            return false;
        }
        public object GetAttribute(int id,string name)
        {
            if (results[id].State == 0) return null;
            return 0;
        }
        public void LoadData(string path)
        {
            data = File.ReadAllText(path).ToCharArray();
            length = data.Length;
        }
        public string GetData()
        {
            return new string(data);
        }
    }
}
