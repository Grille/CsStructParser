using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGL.IO
{
    public enum TypName { Byte, Int, Float,  Double, Bool, String, Ref,Var};
    public enum TokenKind { Other, Number, Bool, String,Command};

    public partial class Parser
    {
        char[] commandChars = new char[] { ',', '{', '}', '[', ']', '=', '+', '-', '*', '/', ':', '<', '>', '>' };
        char[] endChars = new char[] { '\n', ' ', '\r', ';', ';' };

        private void setCode(string code)
        {
            codeLoaded = true;
            objectDeclaretionsParsed = attributesParsed = objectInitializationParsed = false;
            this.code = code;
        }
        private void initNativeTypes()
        {
            //types[typesIndex] = new Typ("int")
        }
        private int referseToTokenIndex(int index,string value)
        {
            for (int i = index; i > 0; i--)
                if (tokenList[i].value == value)
                    return i;
            return -1;
        }
        private int referseToTokenIndex(int index, string[] value)
        {
            for (int i = index; i > 0; i--)
                for (int i2 = 0; i2 < value.Length; i2++)
                    if (tokenList[i].value == value[i2])
                        return i;
            return -1;
        }
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

            TypName typ = attributesTyp[attrIndex];
            var array = attributesArray[attrIndex];

            object retValue = null;

            if (!array)
                retValue = readNativeValue(typ, ref index);
            else
            {
                //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 ref, 7 var,8 cond
                int size = testArraySize(typ,index);
                switch (typ)
                {
                    case TypName.Ref:
                    case TypName.Byte: retValue = new byte[size];break;
                    case TypName.Int: retValue = new int[size]; break;
                    case TypName.Float: retValue = new float[size]; break;
                    case TypName.Double: retValue = new double[size]; break;
                    case TypName.Bool: retValue = new bool[size]; break;
                    case TypName.String: retValue = new string[size]; break;
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
        private TokenKind testTypKind(string value)
        {
            if (value == "true" || value == "false")
                return TokenKind.Bool;
            switch (value[0])
            {
                case '0':case '1':case '2':case '3':case '4':
                case '5':case '6':case '7':case '8':case '9':
                    return TokenKind.Number;
                case '"':
                    return TokenKind.String;
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
                    return TokenKind.Command;
            }
            return TokenKind.Other;
        }

        private void addValueToArray(ref object array, TypName typ, object value, int index)
        {
            switch (typ)
            {
                case TypName.Ref:
                case TypName.Byte: ((byte[])array)[index] = Convert.ToByte(value); break;
                case TypName.Int: ((int[])array)[index] = Convert.ToInt32(value); break;
                case TypName.Float: ((float[])array)[index] = Convert.ToSingle(value); break;
                case TypName.Double: ((double[])array)[index] = Convert.ToDouble(value); break;
                case TypName.Bool: ((bool[])array)[index] = Convert.ToBoolean(value); break;
                case TypName.String: ((string[])array)[index] = (string)value; break;
            }
        }
        private void readValueToArray(ref object array, TypName typ, ref int pos,int index)
        {
            addValueToArray(ref array, typ, readNativeValue(typ, ref pos), index);
        }
        private object readNativeValue(TypName typ, int index)
        {
            return readNativeValue(typ, ref index);
        }
        private object readNativeValue(TypName typ,ref int index)
        {
            double neg = 1;
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            if (typ == TypName.Byte|| typ == TypName.Int|| typ == TypName.Float|| typ == TypName.Double)
            {
                bool enableRet = false;
                int retValue = 0;
                if (tokenList[index].value == "-")
                {
                    neg = -1;
                    index++;
                }
                else if (tokenList[index].value == "+")
                    index++;
                else if (tokenList[index].value == "&")
                {
                    index++;
                    int indx = compareNames(tokenList[index].value, objectNames, objectsIndex);
                    if (indx == -1) throw new ParserException(tokenList[index],"Object \"" + tokenList[index].value + "\" is not defined");
                    retValue = indx; enableRet = true;
                }
                else if (tokenList[index].kind == TokenKind.Other)
                {
                    int indx = compareNames(tokenList[index].value, enumNames, enumIndex);
                    if (indx == -1) throw new ParserException(tokenList[index],"Enum \"" + tokenList[index].value + "\" is not defined");
                    retValue = enumValue[indx]; enableRet = true;
                }
                if (enableRet)
                {
                    switch (typ)
                    {
                        case TypName.Byte: return (byte)retValue;
                        case TypName.Int: return (int)retValue;
                        case TypName.Float: return (float)retValue;
                        case TypName.Double: return (double)retValue;
                    }
                }
            }
            
            string value = tokenList[index].value;
            try
            {
                switch (typ)
                {
                    case TypName.Byte: return (byte)(Convert.ToByte(value) * neg);
                    case TypName.Int: return (int)(Convert.ToInt32(value) * neg);
                    case TypName.Float: return (float)(Convert.ToSingle(value.Replace('.', ',').TrimEnd('f')) * neg);
                    case TypName.Double: return (double)(Convert.ToDouble(value.Replace('.', ',').TrimEnd('d')) * neg);
                    case TypName.Bool:
                        if (value == "0") return false;
                        else if (value == "1") return true;
                        return Convert.ToBoolean(value);
                    case TypName.Ref: int id = compareNames(value,objectNames);
                        if (id ==-1) throw new ParserException(tokenList[index], "Object \"" + tokenList[index].value + "\" is not defined");
                        return (byte)id;
                    case TypName.String: return value;
                    default: return null;
                }
            }
            catch (FormatException e)
            {
                throw new ParserException(tokenList[index],"Value \"" + tokenList[index].value + "\" is not a "+(TypName)typ);
            }
        }
        private object defaultTypValue(TypName typ,bool array)
        {
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            if (!array)
                switch (typ)
                {
                    case TypName.Ref:
                    case TypName.Byte: return (byte)0;
                    case TypName.Int: return (int)0;
                    case TypName.Float: return (float)0;
                    case TypName.Double: return (double)0;
                    case TypName.Bool: return false;
                }
            return null;
        }
        private int testArraySize(TypName typ,int index)
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
                        size += Math.Abs(Convert.ToInt32(readNativeValue(typ, referseToTokenIndex(index,new string[] { "[", "]", "," })+1)) - Convert.ToInt32(readNativeValue(typ, index+1)));
                        break;
                }
                index++;
            } while (scope > 0);
            return size+1;

        }
        private object combineArray(TypName typ,object array1,object array2)
        {
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            switch (typ)
            {
                case TypName.Ref:
                case TypName.Byte: return combineArray((byte[])array1, (byte[])array2);
                case TypName.Int: return combineArray((int[])array1, (int[])array2);
                case TypName.Float: return combineArray((float[])array1, (float[])array2);
                case TypName.Double: return combineArray((double[])array1, (double[])array2);
                case TypName.Bool: return combineArray((bool[])array1, (bool[])array2);
                case TypName.String: return combineArray((string[])array1, (string[])array2);
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
