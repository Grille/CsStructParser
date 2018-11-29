using System.IO;
using System;
using System.Reflection;

namespace GGL.IO
{
    public partial class Parser
    {

        public int ObjectCount { get { return objectsIndex; } }
        /// <summary>Test if an object exists with this id.</summary>
        public bool Exists(int number)
        {
            return Exists("" + number);
        }
        /// <summary>Test if an object exists with this name.</summary>
        public bool Exists(string name)
        {
            return compareNames(name,objectNames) != -1;
        }
        /// <summary>Removes all previously added attributes, enums and objects.</summary>
        public void Clear()
        {
            typesIndex = 0;
            types = new Typ[256];
            initNativeTypes();

            attributesIndex = 0;
            attributesTyp = new byte[256];
            attributesArray = new bool[256];
            attributesName = new string[256];
            attributesInitValue = new object[256];

            enumIndex = 0;
            enumValue = new int[512];
            enumNames = new string[512];

            objectsIndex = 0;
            objectNames = new string[256];

            results = new Struct[256];
        }
        public string[] AttributeNames
        {
            get
            {
                string[] result = new string[attributesIndex];
                Array.Copy(attributesName, result, attributesIndex);
                return result;
            }
        }
        public string[] ObjectNames
        {
            get
            {
                string[] result = new string[objectsIndex];
                Array.Copy(objectNames, result, objectsIndex);
                return result;
            }
        }
        public void AddConst(string name, int value)
        {
            enumNames[enumIndex] = name;
            enumValue[enumIndex++] = value;
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
        /*
        public void AddAttribute<T>(string name, T value)
        {
            for (int i = 0; i < attributesIndex; i++)
            {

            }
        }
        */
        /// <summary>Add a new attribute.</summary>
        /// <param name="type">Type of the attribute. z.B byte,int...</param>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="value">Default value of the attribute.</param>
        public void AddAttribute(string type,string name,string value)
        {
            parse("Attributes{"+ type + " "+name+ (value.Length>0?"=" +value:"")+"}");
        }

        /*
        public T GetStruct<T>(string name)
        {
            T t = default(T);
            GetStruct<T>(ref t, name);
            return t;
        }
        */
        /// <summary>Fills all public fields and properties of the referenced object with values from identically named fields of the selectet object.</summary>
        /// <param name="dstStruct">Reference to C# object/struct.</param>
        /// <param name="srcName">Name of the parsed objects.</param>
        public void FillAttributes<T>(ref T dstStruct,string srcName)
        {
            int obj = compareNames(srcName, objectNames);
            if (obj == -1) throw new Exception("Struct <" + srcName + "> is not defined");

            FieldInfo[] info = dstStruct.GetType().GetFields();
            TypedReference reference = __makeref(dstStruct);
            for (int i = 0; i < info.Length; i++)
            {

                int attri = compareNames(info[i].Name, attributesName);
                if (attri != -1)
                {
                    info[i].SetValueDirect(reference, results[obj].AttributesValue[attri]);
                }
            }
        }
        /// <summary>Returns the specified attribute.</summary>
        /// <param name="objectNumber">name/id of the object </param>
        /// <param name="name">name of the attribute </param>
        public T GetAttribute<T>(int objectNumber, string name)
        {
            return GetAttribute<T>("" + objectNumber, name);
        }
        /// <summary>Returns the specified attribute.</summary>
        /// <param name="objectName">name of the object </param>
        /// <param name="name">name of the attribute </param>
        public T GetAttribute<T>(string objectName,string name)
        {
            int obj = compareNames(objectName, objectNames);
            if (obj == -1) throw new Exception("Object <"+ objectName+"> is not defined");
            int attri = compareNames(name, attributesName);
            if (attri == -1) throw new Exception("Attribute <" + name + "> is not declared");
            return (T)results[obj].AttributesValue[attri];
        }
        /// <summary>Load code from a file without parsing it.</summary>
        public void LoadFile(string path)
        {
            code = File.ReadAllText(path);
        }
        /// <summary>Load code without parsing it.</summary>
        public void LoadCode(string code)
        {
            this.code = code;
        }
        /// <summary>Load code from a file and parse it.</summary>
        public void ParseFile(string path)
        {
            code = File.ReadAllText(path);
            parse(code);
            parserState = 0;
        }
        /// <summary>Load code and parse it.</summary>
        public void ParseCode(string code)
        {
            parse(this.code = code);
            parserState = 0;
        }
        /*
        public void ParseDeclerations()
        {
            parseToTokenList(code);
            pharseEnums();
            pharseObjectDeclaretions();
            parserState = 1;
        }
        public void ParseInitializations()
        {
            pharseAttributes("Attributes");
            pharseAttributes("Init");
            parseObjectInitialization();
            parserState = 0;
        }
        */
        /// <summary>Parse the currently loaded code.</summary>
        public void Parse()
        {
            parse(code);
            parserState = 0;
        }
    }
}
