using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGL.IO
{
    enum Typ { Byte, Int, Float,  Double, Bool, String, Var};
    enum TypKind { Other, Number, Bool, String,Command};

    public partial class Parser
    {
        char[] commandChars = new char[] { ',', '{', '}', '[', ']', '=', '+', '-', '*', '/', ':', '<', '>', '>' };
        char[] endChars = new char[] { '\n', ' ', '\r', ';', ';' };

        private int searchTokenIndex(string name)
        {
            for (int i = 0;i< tokenList.Length; i++)
            {
                if (tokenList[i].value == name) return i;
            }
            return -1;
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
        private object getValue(ref int index, int attrIndex)
        {

            int typ = attributesTyp[attrIndex * 2];
            int array = attributesTyp[attrIndex * 2+1];

            object retValue = null;

            if (array == 0)
                retValue = readNativeValue(typ, ref index);
            else
            {
                //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
                int size = testArraySize(index);
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
                    index += 1;
                    return retValue;
                }
                
                
                int arrayIndex = 0;
                int scope = 1;
                while (scope > 0)
                {
                    index+=1;
                    switch (tokenList[index].value)
                    {
                        case "[":scope++; break;
                        case "]":scope--; break;
                        case ",": break;
                        default:
                            object value = readNativeValue(typ, ref index);
                            if (tokenList[index + 1].value == "to")
                            {
                                index+=2;
                                int v1 = Convert.ToInt32(value);
                                int v2 = Convert.ToInt32(readNativeValue(typ, ref index));
                                if (v1 < v2)
                                    for (int i = v1; i <= v2; i++)
                                        addValueToArray(ref retValue, typ, i, arrayIndex++);
                                else
                                    for (int i = v1; i >= v2; i--)
                                        addValueToArray(ref retValue, typ, i, arrayIndex++);
                            }
                            else addValueToArray(ref retValue, typ, value, arrayIndex++);
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
                    return TypKind.String;
                case '=':case '+':case '-':case '*':case '/':
                case ',':
                case '{':
                case '}':
                case '(':
                case ')':
                case '[':
                case ']':
                case ':':
                case '<':
                case '>':
                case ';':
                    return TypKind.Command;
            }
            return TypKind.Other;
        }

        private void addValueToArray(ref object array, int typ, object value, int index)
        {
            switch (typ)
            {
                case 0: ((byte[])array)[index] = Convert.ToByte(value); break;
                case 1: ((int[])array)[index] = Convert.ToInt32(value); break;
                case 2: ((float[])array)[index] = Convert.ToSingle(value); break;
                case 3: ((double[])array)[index] = Convert.ToDouble(value); break;
                case 4: ((bool[])array)[index] = Convert.ToBoolean(value); break;
                case 5: ((string[])array)[index] = (string)value; break;
            }
        }
        private void readValueToArray(ref object array, int typ, ref int pos,int index)
        {
            addValueToArray(ref array, typ, readNativeValue(typ, ref pos), index);
        }
        private object readNativeValue(int typ, int index)
        {
            return readNativeValue(typ, ref index);
        }
        private object readNativeValue(int typ,ref int index)
        {
            double neg = 1;
            if (tokenList[index].value == "-")
            {
                neg = -1;
                index++;
            }
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            if (typ < 4 && tokenList[index].kind == TypKind.Other)
            {
                int indx = compareNames(tokenList[index].value, enumNames, enumIndex);
                if (indx == -1) throw new Exception("line "+tokenList[index].line+": enum \"" + tokenList[index].value + "\" is not defined");
                switch (typ)
                {
                    case 0: return (byte)enumValue[indx];
                    case 1: return (int)(enumValue[indx]);
                    case 2: return (float)enumValue[indx];
                    case 3: return (double)enumValue[indx];
                }
            }
            
            string value = tokenList[index].value;
            switch (typ)
            {
                case 0: return (byte)(Convert.ToByte(value) * neg);
                case 1: return (int)(Convert.ToInt32(value) * neg);
                case 2: return (float)(Convert.ToSingle(value.Replace('.',',').TrimEnd('f')) * neg);
                case 3: return (double)(Convert.ToDouble(value.Replace('.', ',').TrimEnd('d')) * neg);
                case 4: return Convert.ToBoolean(value);
                case 5: return value;
                default:return null;
            }
        }
        private int testArraySize(int index)
        {
            int scope = 0;
            int size = 0;
            do
            {
                switch (tokenList[index].value)
                {
                    case "[":scope++;
                        if (tokenList[index+1].value == "]") return 0;
                        break;
                    case "]":scope--;break;
                    case ",":size++;break;
                    case "to":
                        size += Math.Abs((int)readNativeValue(1,index - 1) - (int)readNativeValue(1,index+1));
                        break;
                }
                index++;
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
       
    }
}
