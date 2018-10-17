using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGL.IO
{
    class Typ
    {
        int typ;//0 native,1 enum, 2 struct
        string name;
        object defaultValue;
        public Typ(string name,object value)
        {

            typ = 0;
            this.name = name;
            defaultValue = value;
        }
    }
}