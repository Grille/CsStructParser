using System.IO;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Grille.Parsing.Tcf
{
    public partial class TcfParser
    {
        public int ObjectCount { get { return objects.Count; } }
        /// <summary>Test if an object exists with this id.</summary>
        public bool Exists(int number)
        {
            return Exists(number.ToString());
        }
        /// <summary>Test if an object exists with this name.</summary>
        public bool Exists(string name)
        {
            return findIndexByName(name, objects) != -1;
        }
        /// <summary>Removes all previously added objects.</summary>
        public void ClearResults()
        {
            objectIndex = 0;
            objects.Clear();
            Result.Clear();
        }

        public void AddConst(string name, int value)
        {
            Constants.Add(name, value);
        }
        public void AddEnum(string group, string name, int value)
        {
            AddConst(group + '.' + name, value);
        }
        public void AddEnum(string group, string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                AddEnum(group, names[i], i);
            }
        }

        /// <summary>Add a new attribute.</summary>
        /// <param name="type">Type of the attribute. z.B byte,int...</param>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="value">Default value of the attribute.</param>
        public void AddAttribute(string type, string name, string value)
        {
            parse("Attributes{" + type + " " + name + (value.Length > 0 ? "=" + value : "") + "}");
        }
        /// <summary>Fills all public fields and properties of the referenced object with values from identically named fields of the selectet object.</summary>
        /// <param name="dstStruct">Reference to C# object/struct.</param>
        /// <param name="srcName">Name of the parsed objects.</param>
        public void FillAttributes<T>(ref T dstStruct, string srcName)
        {
            int obj = findIndexByName(srcName, objects);
            if (obj == -1) throw new Exception("Struct <" + srcName + "> is not defined");

            FieldInfo[] info = dstStruct.GetType().GetFields();
            TypedReference reference = __makeref(dstStruct);
            for (int i = 0; i < info.Length; i++)
            {

                int attri = findIndexByName(info[i].Name, DefaultType.Properties);
                if (attri != -1)
                {
                    info[i].SetValueDirect(reference, objects[obj].Values[attri]);
                }
            }
        }

        /// <summary>Returns the specified attribute.</summary>
        /// <param name="objectNumber">name/id of the object </param>
        /// <param name="name">name of the attribute </param>
        public T GetAttribute<T>(int objectNumber, string name)
        {
            return GetAttribute<T>(objectNumber.ToString(), name);
        }

        /// <summary>Returns the specified attribute.</summary>
        /// <param name="objectName">name of the object </param>
        /// <param name="name">name of the attribute </param>
        public T GetAttribute<T>(string objectName, string name)
        {
            return Result[objectName].Get<T>(name);
        }

        /// <summary>Load code from a file and parse it.</summary>
        public void ParseFile(string path)
        {
            var code = File.ReadAllText(path);
            parse(code);
        }

        /// <summary>Load code and parse it.</summary>
        public void ParseCode(string code)
        {
            parse(code);
        }
    }
}
