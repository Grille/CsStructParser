using System.IO;
using System;
using System.Threading;
namespace GGL.IO
{
    public partial class Parser
    {
        const int con = 9,h=8;
        enum enu { gg,dd,rr};

        private int length = 0;
        private char[] data;
        Token[] tokenList;


        byte attributesIndex;
        byte objectsIndex;

        byte[] attributesTyp;
        string[] attributesName;
        string[] objectsName;
        Object[] attributesInitValue;

        int enumIndex;
        int[] enumValue;
        string[] enumName;

        Result[] results;

        public Parser()
        {
            Clear();
        }

        private void parse()
        {
            deleteComments();
            prepare();

            pharseAttributes();
            pharseInit();
            //pharseEnum();
            pharseObjects();
        }

        private void prepare()
        {
            int iSrc, iDst = 0, iEnd;
            bool stringMode = false;
            char[] dstData = new char[data.Length * 2];
            //Console.BackgroundColor = ConsoleColor.DarkGray;
            //normalize ends, isolate operators
            for (iSrc = 0; iSrc < length; iSrc++)
            {
                if (data[iSrc] == '"') stringMode = !stringMode;

                if (stringMode) dstData[iDst++] = data[iSrc];
                else if (data[iSrc] == ' ' || data[iSrc] == '\r' || data[iSrc] == ';' || data[iSrc] == ';') dstData[iDst++] = '\n'; 
                else if (data[iSrc] == ',' || data[iSrc] == '{' || data[iSrc] == '}' || data[iSrc] == '[' || data[iSrc] == ']' || data[iSrc] == '=' || data[iSrc] == '+' || data[iSrc] == '-' || data[iSrc] == '*' || data[iSrc] == '/' || data[iSrc] == ':' || data[iSrc] == '<' || data[iSrc] == '>' || data[iSrc] == ';')
                {
                    dstData[iDst++] = '\n';
                    dstData[iDst++] = data[iSrc];
                    dstData[iDst++] = '\n';
                }
                else dstData[iDst++] = data[iSrc];
            }
            data = dstData;

            //collapse ends
            iEnd = iDst; iDst = 0;
            for (iSrc = 0; iSrc <= iEnd; iSrc++)
            {
                if (data[iSrc] == '"') stringMode = !stringMode;
                if (stringMode || data[iSrc] != '\n' || data[iSrc + 1] != '\n') data[iDst++] = data[iSrc];
            }
            length = iDst;

            //Array.Resize(ref data, length);

            int tokenCount = 0;
            for (int i = 0;i< length; i++)
            {
                if (data[i] == '\n') tokenCount++;
            }
            tokenList = new Token[tokenCount];
            nextItem(0);
            for (int i = 0;i< tokenCount; i++)
            {
                tokenList[i].value = getItem();
                tokenList[i].kind = testKind(tokenList[i].value);
                tokenList[i].line = i;
                nextItem();
            }
            for (int i = 0; i < tokenList.Length; i++)
            {
                switch (tokenList[i].kind)
                {
                    case TypKind.Command:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case TypKind.Number:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case TypKind.Text:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                }
                Console.Write(tokenList[i].value+" ");
            }

            Console.WriteLine(tokenCount);
            //for (int i = 0)
            /*
            while (true)
            {
                Console.WriteLine(getItem());
                nextItem();
            }
            */
        }
        private void deleteComments()
        {
            int commentMode = 0;
            int iDst = 0;
            int newLenght = 0;
            for (int iSrc = 0; iSrc < length; iSrc++)
            {
                if (commentMode == 0)
                {
                    if (data[iSrc] == '/' && data[iSrc + 1] == '/') commentMode = 1;
                    else if (data[iSrc] == '/' && data[iSrc + 1] == '*') commentMode = 2;
                    else
                    {
                        data[iDst] = data[iSrc];
                        iDst++;
                        newLenght++;
                    }
                }
                else if (commentMode == 1 && data[iSrc] == '\n') commentMode = 0;
                else if (commentMode == 2 && data[iSrc] == '*' && data[iSrc + 1] == '/') { commentMode = 0; iSrc++; }
            }
            length = newLenght;
        }

        private void pharseAttributes()
        {
            int index = searchTokenIndex("Attributes");
            if (index == -1) return;
            index += 2;
            while (tokenList[index].value != "}")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                byte array = 0;
                string type;
                if (tokenList[index + 1].value != "=")
                {
                    type = tokenList[index++].value;
                    if (tokenList[index].value == "[")
                    {
                        array = 1; index += 2;
                    }

                    byte typ = 0; //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
                    if (type[0] == 'b' && type[0 + 1] == 'y') typ = 0;
                    else if (type[0] == 'i') typ = 1;
                    else if (type[0] == 'f') typ = 2;
                    else if (type[0] == 'd') typ = 3;
                    else if (type[0] == 'b' && type[0 + 1] == 'o') typ = 4;
                    else if (type[0] == 's') typ = 5;
                    else if (type[0] == 'v') typ = 6;

                    int i = 0;
                    do
                    {
                        if (i++ > 0) index += 2;
                        string name = tokenList[index].value;
                        object value = null;

                        attributesTyp[attributesIndex * 2] = typ;
                        attributesTyp[attributesIndex * 2 + 1] = array;
                        attributesName[attributesIndex] = tokenList[index].value;
                        if (tokenList[index + 1].value == "=")
                        {
                            index += 2;
                            value = getValue(ref index, attributesIndex);
                            attributesInitValue[attributesIndex] = value;
                        }
                        attributesIndex++;

                        Console.WriteLine(type + "[" + array + "]->" + name + (value != null ? ("=" + value) : "") + ";");
                    } while (tokenList[index + 1].value == ",");

                }
                else if (tokenList[index + 1].value == "=")
                {
                    int attri = compareNames(tokenList[index].value, attributesName);
                    if (attri == -1) throw new Exception("Attribute \"" + tokenList[index].value + "\" is not defined");
                    index += 2;
                    attributesInitValue[attri] = getValue(ref index, attri);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(attributesName[attri]+"="+attributesInitValue[attri]+";");
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(tokenList[index].value);
                index++;
            }
            Console.WriteLine("attributesIndex " + attributesIndex);
        }
        private void pharseInit()
        {
            int index = searchTokenIndex("Init");
            if (index == -1) return;
            index += 2;
            while (tokenList[index].value != "}")
            {
                if (tokenList[index + 1].value == "=")
                {
                    int attri = compareNames(tokenList[index].value, attributesName);
                    if (attri == -1) throw new Exception("Attribute \"" + tokenList[index].value + "\" is not defined");
                    index += 2;
                    attributesInitValue[attri] = getValue(ref index, attri);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(attributesName[attri] + "=" + attributesInitValue[attri] + ";");
                }
                index++;
            }
        }

        private void pharseEnum()
        {
            
            int pos = searchTokenIndex("Enums");
            if (pos == -1) return;

            string group = "";
            int value = 0;
            int scope = 0;
            do
            {
                pos++;
                switch (tokenList[pos].value[0])
                {
                    case '{': scope++; break;
                    case '}': scope--; break;
                    case ',': value++; break;
                    default:
                        if (scope == 1)
                        {
                            group = tokenList[pos].value;
                            value = 0;
                        }
                        else if (scope == 2)
                        {
                            string name = tokenList[pos].value;
                            if (tokenList[++pos].value == "=")
                            {
                                pos +=2;
                                value = (int)readNativeValue(1, ref pos);
                                pos++;
                            }
                            enumName[enumIndex] = group + '.' + name;
                            enumValue[enumIndex++] = value;
                        }
                        break;
                }
            } while (scope > 0);
            
        }

        private void pharseObjects()
        {
            Result.AttributesNumber = attributesIndex;
            for (int i = 0; i < tokenList.Length; i++)
            {
                if (tokenList[i].value != "<") continue;
                string name = tokenList[i+1].value;
                int pid = -1;i += 3;
                if (tokenList[i].value == ":")
                {
                    pid = compareNames(tokenList[i + 1].value, objectsName); i+=2;
                }
                objectsName[objectsIndex] = name;
                results[objectsIndex].Init(i, pid);
                objectsIndex++;
            }
            for (int i = 0; i < objectsIndex; i++) pharseObject(i);
        }
        private void pharseObject(int id)
        {
            if (!results[id].Used || results[id].State != 0) return;

            results[id].State = 1;
            int pid = results[id].ParentID;

            //valid referral
            if (pid != -1 && results[pid].Used)
            {
                pharseObject(pid);
                for (int i = 0; i < attributesIndex; i++) results[id].AttributesValue[i] = results[pid].AttributesValue[i];
            }
            else
            {
                for (int i = 0; i < attributesIndex; i++) results[id].AttributesValue[i] = attributesInitValue[i];
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(tokenList[results[id].Pos].value);

            int index = results[id].Pos;
            index += 1;
            while (tokenList[index].value != "}")
            {
                int attri = compareNames(tokenList[index].value, attributesName);
                if (attri == -1) throw new Exception("Attribute \"" + tokenList[index].value + "\" in <" + objectsName[id] + "> is not defined");
                index += 2;
                switch (tokenList[index - 1].value)
                {
                    case "=":
                        results[id].AttributesValue[attri] = getValue(ref index, attri);
                        break;
                    case "+":
                        if (attributesTyp[attri * 2 + 1] > 0)
                        {
                            results[id].AttributesValue[attri] = combineArray(attributesTyp[attri * 2], results[id].AttributesValue[attri], getValue(ref index, attri));
                        }
                        break;
                }
                index++;
            }
            results[id].State = 2;
        }
    }
}