using System.IO;
using System;
namespace GGL.IO
{
    public partial class Parser
    {
        public bool IDUsed(int id)
        {
            return results[id].Used;
        }
        public void AddAttribute(string type,string name,string value)
        {
            int typ;
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            if (type[0] == 'b' && type[0 + 1] == 'y') typ = 0;
            else if (type[0] == 'i') typ = 1;
            else if (type[0] == 'f') typ = 2;
            else if (type[0] == 'f') typ = 3;
            else if (type[0] == 'b' && type[0 + 1] == 'o') typ = 4;
            else if (type[0] == 's') typ = 5;
            else if (type[0] == 'v') typ = 6;
        }
        public T GetAttribute<T>(int id,string name)
        {
            if (!results[id].Used) return default(T);
            int attri = compareNames(name, attributesName);
            return (T)results[id].AttributesValue[attri];
        }
        public void LoadData(string path)
        {
            data = File.ReadAllText(path).ToCharArray();
            length = data.Length;
        }
        public void Parse()
        {
            deleteComments();
            prepare();

            pharseAttributes();
            pharseInit();
            pharseEnum();
            pharseObjects();
        }
        public string GetData()
        {
            return new string(data);
        }
    }
}
