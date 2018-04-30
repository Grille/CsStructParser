using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programm
{
    public struct Result
    {
        public bool Used;
        public int Pos;
        public static int AttributesNumber;
        public int State;
        public int ParentID;
        public object[] AttributesValue;

        public void Init(int pos)
        {
            Init(pos,-1);
        }
        public void Init(int pos,int parent)
        {
            Pos = pos;
            ParentID = parent;
            State = 0;
            Used = true;
            AttributesValue = new object[AttributesNumber];
        }
    }
}
