using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGL.IO
{
    struct Struct
    {
        public int Pos;
        public int State;
        public string ParentName;
        public object[] AttributesValue;

        public void Declare(int pos, string parent)
        {
            Pos = pos;
            ParentName = parent;
            State = 0;
        }
        public void Define(int size)
        {
            AttributesValue = new object[size];
        }
    }
}
