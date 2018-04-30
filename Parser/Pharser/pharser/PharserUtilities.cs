using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programm
{
    public partial class Pharser
    {

        private int globalPos = 0;

        private int nextItem()
        {
            return nextItem(globalPos);
        }
            private int nextItem(int index)
        {
            while (data[index++] != '\n') { }
            globalPos = index;
            return index;
        }
        private string getItem()
        {
            return getItem(globalPos);
        }
        private string getItem(int index)
        {
            string item = "";
            while (data[index] != '\n') item += data[index++];
            return item;
        }
        private int getAttributeByName()
        {
            return getAttributeByName(globalPos);
        }
        private int getAttributeByName(int pos)
        {
            for (int i = 0; i < attributesLenght; i++)
            {
                if (testString(pos, attributesName[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        private object getValue(int attrIndex)
        {
            return getValue(globalPos);
        }
        private object getValue(ref int pos, int attrIndex)
        {

            int typ = attributesTyp[attrIndex * 2];
            int array = attributesTyp[attrIndex * 2+1];

            object retValue = null;

            if (array == 0)
                retValue = convertTyp(typ,getItem(pos));
            else
            {
                //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
                int size = testArraySize(pos);
                switch (typ)
                {
                    case 0: retValue = new byte[size];break;
                    case 1: retValue = new int[size]; break;
                    case 2: retValue = new float[size]; break;
                    case 3: retValue = new double[size]; break;
                    case 4: retValue = new bool[size]; break;
                    case 5: retValue = new string[size]; break;
                }
                int index = 0;
                while (index < size)
                {
                    pos = nextItem(pos);
                    switch (data[pos])
                    {
                        case '[':case ']':case ',': break;
                        default:
                            switch (typ)
                            {
                                case 0: ((byte[])(retValue))[index++] = (byte)convertTyp(0, getItem(pos)) ; break;
                                case 1: ((int[])(retValue))[index++] = (int)convertTyp(1, getItem(pos)); break;
                                case 2: ((float[])(retValue))[index++] = (float)convertTyp(2, getItem(pos)); break;
                                case 3: ((double[])(retValue))[index++] = (double)convertTyp(3, getItem(pos)); break;
                                case 4: ((bool[])(retValue))[index++] = (bool)convertTyp(4, getItem(pos)); break;
                                case 5: ((string[])(retValue))[index++] = (string)convertTyp(5, getItem(pos)); break;
                            }
                        break;
                    }
                }
                
            }
            return retValue;
        }
        private object convertTyp(int typ,string value)
        {
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            switch (typ)
            {
                case 0: return Convert.ToByte(value);
                case 1: return Convert.ToInt32(value);
                case 2: return Convert.ToSingle(value);
                case 3: return Convert.ToDouble(value);
                case 4: return Convert.ToBoolean(value);
                case 5: return value;
                default:return null;
            }
        }
        private int testArraySize(int pos)
        {
            int scope = 0;
            int size = 0;
            do
            {
                switch (data[pos])
                {
                    case '[':scope++;break;
                    case ']':scope--;break;
                    case ',':size++;break;
                }
                pos = nextItem(pos);
            } while (scope > 0);
            return size+1;

        }

        private bool testString(int pos, string input)
        {
            return testString(pos, input.ToCharArray());
        }
        private bool testString(int pos, char[] input)
        {
            if (pos + input.Length > length) return false;
            for (int i = 0; i < input.Length; i++)
            {
                if (data[pos + i] != input[i]) return false;
            }
            return true;
        }
        private int searchString(string input)
        {
            return searchString(input,0, length);
        }
        private int searchString(string input,int min)
        {
            return searchString(input, min, length);
        }
        private int searchString(string input,int min, int max)
        {
            for (int i = min; i < max; i++)
            {
                if (testString(i, input))
                {
                    globalPos = i;
                    return i;
                }
            }
            globalPos = -1;
            return -1;
        }
    }
}
