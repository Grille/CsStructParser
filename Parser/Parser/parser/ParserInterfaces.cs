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
            return compareNames(name,objectNames) != -1;
        }
        public void Clear()
        {
            attributesIndex = 0;
            attributesTyp = new byte[256];
            attributesName = new string[256];
            attributesInitValue = new object[256];

            enumIndex = 0;
            enumValue = new int[256];
            enumNames = new string[256];

            objectsIndex = 0;
            objectNames = new string[256];

            results = new Result[256];
        }
        public string[] GetAttributeNames()
        {
            Array.Resize(ref attributesName, attributesIndex);
            return attributesName;
        }
        public string[] GetObjectNames()
        {
            Array.Resize(ref objectNames, objectsIndex);
            return objectNames;
        }
        public void AddEnum(string group,string name,int value)
        {
            enumNames[enumIndex] = group + '.' + name;
            enumValue[enumIndex++] = value;
        }
        public void AddEnum(string group, string[] names)
        {
            int value = 0;
            for (int i = 0; i < names.Length; i++)
            {
                enumNames[enumIndex] = group + '.' + names[i];
                enumValue[enumIndex++] = value++;
            }
        }

        public void AddAttribute<T>(string name, T value)
        {
        }
        public void AddAttribute(string type,string name,string value)
        {
            parse("Attributes{"+ type + " "+name+ (value.Length>0?"=" +value:"")+"}");
        }

        public T GetAttribute<T>(int number, string name)
        {
            return GetAttribute<T>("" + number, name);
        }
        public T GetAttribute<T>(string objectName,string name)
        {
            int obj = compareNames(objectName, objectNames);
            if (obj == -1) throw new Exception("Object <"+ objectName+"> is not defined");
            int attri = compareNames(name, attributesName);
            if (attri == -1) throw new Exception("Attribute <" + name + "> is not declared");
            return (T)results[obj].AttributesValue[attri];
        }
        public void ParseFile(string path)
        {
            parse(File.ReadAllText(path));
        }
        public void ParseCode(string code)
        {
            parse(code);
        }
    }
}
