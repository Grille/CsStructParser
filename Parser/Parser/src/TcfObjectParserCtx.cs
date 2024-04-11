using Grille.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Grille.Parsing.Tcf
{
    enum ParseStatus
    {
        None, 
        Parsing,
        Done,
    }

    public class TcfType : IHasName
    {
        public string Name { get; }

        public List<PropertyDefinition> Properties { get; }

        public TcfType(string name) {
            Name = name;
            Properties = new List<PropertyDefinition>();
        }

        public class PropertyDefinition : IHasName
        {
            public TypeName Type { get; }
            public bool IsArray { get; }
            public string Name { get; }
            public object DefaultValue { get; set; }

            public PropertyDefinition(TypeName type, bool isArray, string name, object defaultValue)
            {
                Type = type;
                IsArray = isArray;
                Name = name;
                DefaultValue = defaultValue;
            }
        }

        internal void CreateProperty(TypeName type, bool isArray, string name, object value)
        {
            Properties.Add(new PropertyDefinition(type, isArray, name, value));
        }
    }

    class TcfObjectParserCtx : IHasName
    {
        public TcfObjectParserCtx(TcfType type, string name, string parent, int index)
        {
            Object = new TcfObject(type, name);
            ParentName = parent;
            TokenIndex = index;
        }

        public string Name => Object.Name;

        public int TokenIndex { get; }

        public ParseStatus State { get; set; }

        public string ParentName { get; }

        public object[] Values => Object.Values;

        public TcfObject Object { get; }
    }

    public class TcfObject : ICollection
    {
        public TcfType Type { get; }

        public string Name { get; }

        public object[] Values { get; private set; }

        public int Count => Values.Length;

        public object SyncRoot => Values.SyncRoot;

        public bool IsSynchronized => Values.IsSynchronized;

        public TcfObject(TcfType type, string name)
        {
            Type = type;
            Name = name;
        }

        internal void Init()
        {
            Values = new object[Type.Properties.Count];
        }



        /// <summary>Returns the specified property.</summary>
        /// <param name="key">name of the property </param>
        public T Get<T>(string key)
        {
            for (int i = 0; i < Type.Properties.Count; i++)
            {
                if (Type.Properties[i].Name == key)
                {
                    return (T)Values[i];
                }
            }
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        public void CopyTo(Array array, int index)
        {
            Values.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public object this[string key]
        {
            get => Get<object>(key);
        }
    }
}
