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
