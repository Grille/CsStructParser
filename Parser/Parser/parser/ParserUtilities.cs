using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGL.IO
{
    enum Typ { Byte, Int, Float,  Double, Bool, String, Var};
    enum TypKind { Other, Number, Bool, Text,Command};
    public partial class Parser
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
        private int compareNames(string name, string[] nameList)
        {
            return compareNames(name, nameList, nameList.Length);
        }
        private int compareNames(string name,string[] nameList,int lenght)
        {
            for (int i = 0; i < lenght; i++)
            {
                if (name == nameList[i])
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
                retValue = convertTyp(typ,ref pos);
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
                
                if (size == 0)
                {
                    //pos = nextItem(nextItem(pos));
                    return retValue;
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
                                case 0: ((byte[])(retValue))[index++] = (byte)convertTyp(0, ref pos) ; break;
                                case 1: ((int[])(retValue))[index++] = (int)convertTyp(1, ref pos); break;
                                case 2: ((float[])(retValue))[index++] = (float)convertTyp(2, ref pos); break;
                                case 3: ((double[])(retValue))[index++] = (double)convertTyp(3, ref pos); break;
                                case 4: ((bool[])(retValue))[index++] = (bool)convertTyp(4, ref pos); break;
                                case 5: ((string[])(retValue))[index++] = (string)convertTyp(5, ref pos); break;
                            }
                        break;
                    }
                }
                
            }
            return retValue;
        }
        private TypKind testTypKind(string value)
        {
            if (value == "true" || value == "false")
                return TypKind.Bool;
            switch (value[0])
            {
                case '0':case '1':case '2':case '3':case '4':
                case '5':case '6':case '7':case '8':case '9':
                    return TypKind.Number;
                case '"':
                    return TypKind.Text;
                case '=':case '+':case '-':case '*':case '/':
                    return TypKind.Command;
            }
            return TypKind.Other;
        }
        private object convertTyp(int typ, ref int pos)
        {
            if (testTypKind(getItem(pos))== TypKind.Command)
            {
                return convertTyp(typ, getItem(pos)+ getItem(pos = nextItem(pos)));
            }
            return convertTyp(typ, getItem(pos));
        }
        private object convertTyp(int typ,string value)
        {
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            TypKind kind = testTypKind(value);

            switch (typ)
            {
                case 0:case 1:case 2:case 3:
                    if (kind == TypKind.Other)
                    {
                        int indx = compareNames(value, enumName, enumLenght);
                        switch (typ)
                        {
                            case 0: return (byte)enumValue[indx];
                            case 1: return enumValue[indx];
                            case 2: return (float)enumValue[indx];
                            case 3: return (double)enumValue[indx];
                        }
                    }
                    break;
            }
            switch (typ)
            {
                case 0: return Convert.ToByte(value);
                case 1: return Convert.ToInt32(value);
                case 2: return Convert.ToSingle(value);
                case 3: return Convert.ToDouble(value);
                case 4: return Convert.ToBoolean(value);
                case 5: return value.Trim(new char[] { '"' });
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
                    case '[':scope++;
                        if (getItem(nextItem(pos)) == "]") return 0;
                        break;
                    case ']':scope--;break;
                    case ',':size++;break;
                }
                pos = nextItem(pos);
            } while (scope > 0);
            return size+1;

        }
        private object combineArray(int typ,object array1,object array2)
        {
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            switch (typ)
            {
                case 0: return combineArray((byte[])array1, (byte[])array2);
                case 1: return combineArray((int[])array1, (int[])array2);
                case 2: return combineArray((float[])array1, (float[])array2);
                case 3: return combineArray((double[])array1, (double[])array2);
                case 4: return combineArray((bool[])array1, (bool[])array2);
                case 5: return combineArray((string[])array1, (string[])array2);
            }
            return null;
        }
        private T[] combineArray<T>(T[] array1, T[] array2)
        {
            T[] array = new T[array1.Length + array2.Length];
            array1.CopyTo(array, 0);
            array2.CopyTo(array, array1.Length);
            return array;
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
