using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGL.IO
{
    struct Result
    {
        public int Pos;
        public static int AttributesNumber;
        public int State;
        public string ParentName;
        public object[] AttributesValue;

        public void Init(int pos)
        {
            Init(pos,null);
        }
        public void Init(int pos,string parent)
        {
            Pos = pos;
            ParentName = parent;
            State = 0;
            AttributesValue = new object[AttributesNumber];
        }
    }
}
