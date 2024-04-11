using Grille.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Grille.Parsing.Tcf
{
    public enum TypeName { Byte, Int, Float,  Double, Bool, String, Ref, Var};

    public partial class TcfParser
    {
        private void assertToken(int index, TokenKind kind, string value)
        {
            if (!tokens[index].Equals(kind, value))
                throw new TcfParserException(tokens[index]);
        }

        private void assertToken(int index, TokenKind kind)
        {
            if (!tokens[index].Equals(kind))
                throw new TcfParserException(tokens[index]);
        }

        private int referseToTokenIndex(int index,string value)
        {
            for (int i = index; i > 0; i--)
                if (tokens[i].Value == value)
                    return i;
            return -1;
        }
        private int referseToTokenIndex(int index, string[] value)
        {
            for (int i = index; i > 0; i--)
                for (int i2 = 0; i2 < value.Length; i2++)
                    if (tokens[i].Value == value[i2])
                        return i;
            return -1;
        }

        private int findIndexByName<T>(string key, IList<T> list) where T : IHasName
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == key) return i;
            }
            return -1;
        }

        private object getValue(ref int index, TypeName typ, bool array)
        {
            object retValue = null;

            if (!array)
                retValue = readNativeValue(typ, ref index);
            else
            {
                //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 ref, 7 var,8 cond
                int size = testArraySize(typ,index);
                switch (typ)
                {
                    case TypeName.Byte: retValue = new byte[size];break;
                    case TypeName.Ref:
                    case TypeName.Int: retValue = new int[size]; break;
                    case TypeName.Float: retValue = new float[size]; break;
                    case TypeName.Double: retValue = new double[size]; break;
                    case TypeName.Bool: retValue = new bool[size]; break;
                    case TypeName.String: retValue = new string[size]; break;
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
                    switch (tokens[index].Value)
                    {
                        case "[":scope++; break;
                        case "]":scope--; break;
                        case ",": break;
                        default:
                            object value = readNativeValue(typ, ref index);
                            if (tokens[index + 1].Value == "to")
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

        private void addValueToArray(ref object array, TypeName typ, object value, int index)
        {
            switch (typ)
            {
                case TypeName.Byte: ((byte[])array)[index] = Convert.ToByte(value); break;
                case TypeName.Ref:
                case TypeName.Int: ((int[])array)[index] = Convert.ToInt32(value); break;
                case TypeName.Float: ((float[])array)[index] = Convert.ToSingle(value); break;
                case TypeName.Double: ((double[])array)[index] = Convert.ToDouble(value); break;
                case TypeName.Bool: ((bool[])array)[index] = Convert.ToBoolean(value); break;
                case TypeName.String: ((string[])array)[index] = (string)value; break;
            }
        }

        private void readValueToArray(ref object array, TypeName typ, ref int pos,int index)
        {
            addValueToArray(ref array, typ, readNativeValue(typ, ref pos), index);
        }

        private object readNativeValue(TypeName typ, int index)
        {
            return readNativeValue(typ, ref index);
        }

        private object readNativeValue(TypeName typ, ref int index)
        {
            double neg = 1;
            if (typ == TypeName.Byte|| typ == TypeName.Int|| typ == TypeName.Float|| typ == TypeName.Double)
            {
                bool enableRet = false;
                int retValue = 0;

                var token = tokens[index];

                if (token.Value == "-")
                {
                    neg = -1;
                    index++;
                }
                else if (token.Value == "+")
                    index++;
                else if (token.Value == "&")
                {
                    token = tokens[++index];
                    int indx = findIndexByName(token.Value, objects);
                    if (indx == -1) throw new TcfParserException(token, "Object \"" + token.Value + "\" is not defined.");
                    retValue = indx; enableRet = true;
                }
                else if (token.Kind == TokenKind.Word)
                {
                    if (Constants.TryGetValue(token.Value, out retValue))
                    {
                        enableRet = true;
                    }
                    else
                    {
                        throw new TcfParserException(token, "Enum \"" + token.Value + "\" is not defined.");
                    }
                }
                if (enableRet)
                {
                    switch (typ)
                    {
                        case TypeName.Byte: return (byte)retValue;
                        case TypeName.Int: return (int)retValue;
                        case TypeName.Float: return (float)retValue;
                        case TypeName.Double: return (double)retValue;
                    }
                }
            }
            
            string value = tokens[index].Value;
            try
            {
                switch (typ)
                {
                    case TypeName.Byte: return (byte)(Convert.ToByte(value) * neg);
                    case TypeName.Int: return (int)(Convert.ToInt32(value) * neg);
                    case TypeName.Float: return (float)(Convert.ToSingle(value.Replace('.', ',').TrimEnd('f')) * neg);
                    case TypeName.Double: return (double)(Convert.ToDouble(value.Replace('.', ',').TrimEnd('d')) * neg);
                    case TypeName.Bool:
                        if (value == "0") return false;
                        else if (value == "1") return true;
                        return Convert.ToBoolean(value);
                    case TypeName.Ref: 
                        int id = findIndexByName(value,objects);
                        if (id ==-1) throw new TcfParserException(tokens[index], "Object \"" + tokens[index].Value + "\" is not defined");
                        return (byte)id;
                    case TypeName.String: return value;
                    default: return null;
                }
            }
            catch (FormatException)
            {
                throw new TcfParserException(tokens[index],"Value \"" + tokens[index].Value + "\" is not a "+(TypeName)typ);
            }
        }

        private object defaultTypValue(TypeName typ,bool array)
        {
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            if (!array)
                switch (typ)
                {
                    case TypeName.Byte: return (byte)0;
                    case TypeName.Ref:
                    case TypeName.Int: return (int)0;
                    case TypeName.Float: return (float)0;
                    case TypeName.Double: return (double)0;
                    case TypeName.Bool: return false;
                }
            return null;
        }

        private int testArraySize(TypeName typ,int index)
        {
            int scope = 0;
            int size = 0;
            do
            {
                switch (tokens[index].Value)
                {
                    case "[":scope++;
                        if (tokens[index+1].Value == "]") return 0;
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

        private object combineArray(TypeName typ,object array1,object array2)
        {
            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            switch (typ)
            {
                case TypeName.Byte: return CombineArray((byte[])array1, (byte[])array2);
                case TypeName.Ref:
                case TypeName.Int: return CombineArray((int[])array1, (int[])array2);
                case TypeName.Float: return CombineArray((float[])array1, (float[])array2);
                case TypeName.Double: return CombineArray((double[])array1, (double[])array2);
                case TypeName.Bool: return CombineArray((bool[])array1, (bool[])array2);
                case TypeName.String: return CombineArray((string[])array1, (string[])array2);
            }
            return null;
        }

        private static T[] CombineArray<T>(T[] array1, T[] array2)
        {
            T[] array = new T[array1.Length + array2.Length];
            array1.CopyTo(array, 0);
            array2.CopyTo(array, array1.Length);
            return array;
        }
       
    }
}
