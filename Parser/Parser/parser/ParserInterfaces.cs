using System.IO;
using System;
namespace GGL.IO
{
    public partial class Parser
    {
        public bool Exists(int number)
        {
            return Exists("" + number);
        }
        public bool Exists(string name)
        {
            return compareNames(name,objectsName) != -1;
        }
        public void Clear()
        {
            attributesIndex = 0;
            attributesTyp = new byte[256];
            attributesName = new string[256];
            attributesInitValue = new object[256];

            enumIndex = 0;
            enumValue = new int[256];
            enumName = new string[256];

            objectsName = new string[256];

            results = new Result[256];
        }
        public string[] GetAttributeNames()
        {
            Array.Resize(ref attributesName, attributesIndex);
            return attributesName;
        }
        public string[] GetObjectNames()
        {
            Array.Resize(ref objectsName, objectsIndex);
            return objectsName;
        }
        public void AddEnum(string group,string name,int value)
        {
            enumName[enumIndex] = group + '.' + name;
            enumValue[enumIndex++] = value;
        }
        
        public void AddAttribute(string type,string name,string value)
        {
            byte typ = 0;
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            if (type[0] == 'b' && type[0 + 1] == 'y') typ = 0;
            else if (type[0] == 'i') typ = 1;
            else if (type[0] == 'f') typ = 2;
            else if (type[0] == 'd') typ = 3;
            else if (type[0] == 'b' && type[0 + 1] == 'o') typ = 4;
            else if (type[0] == 's') typ = 5;
            else if (type[0] == 'v') typ = 6;

            attributesTyp[attributesIndex * 2] = typ;
            attributesTyp[attributesIndex * 2 + 1] = (byte)(type.Contains("[") ? 1 : 0);
            attributesName[attributesIndex] = name;
            attributesInitValue[attributesIndex] = value;
            attributesIndex++;
        }

        public T GetAttribute<T>(int number, string name)
        {
            return GetAttribute<T>("" + number, name);
        }
        public T GetAttribute<T>(string objectname,string name)
        {
            int obj = compareNames(objectname, objectsName);
            if (obj == -1) return default(T);
            if (!results[obj].Used) return default(T);
            int attri = compareNames(name, attributesName);
            return (T)results[obj].AttributesValue[attri];
        }
        public void ParseFile(string path)
        {
            data = File.ReadAllText(path).ToCharArray();
            length = data.Length;

            parse();
        }
        public void ParseCode(string code)
        {
            data = code.ToCharArray();
            length = data.Length;

            parse();
        }
        public string GetData()
        {
            return new string(data);
        }
    }
}
