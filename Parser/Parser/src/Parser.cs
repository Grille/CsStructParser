using System.IO;
using System;
using System.Threading;
using System.Reflection;
namespace GGL.IO
{
    public unsafe partial class Parser
    {
        private int parserState;

        int typesIndex;
        Typ[] types;
        string code;
        TokenList tokenList;

        byte attributesIndex;
        byte objectsIndex;

        byte[] attributesTyp;
        bool[] attributesArray;
        string[] attributesName;
        string[] objectNames;
        Object[] attributesInitValue;

        int enumIndex;
        int[] enumValue;
        string[] enumNames;

        Struct[] results;

        bool codeLoaded, objectDeclaretionsParsed, attributesParsed, objectInitializationParsed;
        public Parser()
        {
            Clear();
        }

        private void parse(string code)
        {
            parseToTokenList(code);

            pharseEnums();
            pharseObjectDeclaretions();
            pharseAttributes("Attributes");
            parseObjectInitialization();
        }

        private void parseToTokenList(string data)
        {
            tokenList = new TokenList(data.Length);
            int index = 0;
            int curLine = 1;
            int commentMode = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '\n')
                    curLine++;
                if (commentMode == 0)
                {
                    if (data[i] == '/' && data[i + 1] == '/') commentMode = 1;
                    else if (data[i] == '/' && data[i + 1] == '*') commentMode = 2;
                    else
                    {
                        if (data[i] == '"')
                        {
                            i += 1;
                            int start = i, end = 0;
                            while (data[i] != '"')
                            {
                                end = i++;
                                if (data[i] == '\n')
                                    curLine++;
                            }
                            // += 1;
                            //Console.ForegroundColor = ConsoleColor.Red;
                            //Console.Write(data.Substring(start, end- start+1));
                            tokenList[0].kind = 0;
                            tokenList[index].line = curLine;
                            tokenList[index].kind = TypKind.String;
                            if (end != 0)
                                tokenList[index++].value = data.Substring(start, end - start + 1);
                            else
                                tokenList[index++].value = "";

                        }
                        else if (data[i] == ',' || data[i] == '{' || data[i] == '}' || data[i] == '[' || data[i] == ']' || data[i] == '=' || data[i] == '+' || data[i] == '-' || data[i] == '*' || data[i] == '/' || data[i] == ':' || data[i] == '<' || data[i] == '>'|| data[i] == '&')
                        {
                            //Console.ForegroundColor = ConsoleColor.Blue;
                            //Console.Write(data[i]);
                            tokenList[index].line = curLine;
                            tokenList[index].kind = TypKind.Command;
                            tokenList[index++].value = ""+data[i];
                        }
                        else if (data[i] == '\n' || data[i] == ' ' || data[i] == '\r' || data[i] == ';' || data[i] == ';')
                        {
                            /*
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(data[i]);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Gray;
                            */
                        }
                        else
                        {
                            int start = i, end = 0;
                            while ((data[i] >= 65 && data[i] <= 90) || (data[i] >= 97 && data[i] <= 122) || (data[i] >= 48 && data[i] <= 57) || data[i] == 95 || data[i] == 46)
                            {
                                end = i++;
                            }
                            i -= 1;
                            if (end!=0)
                            {
                                //Console.ForegroundColor = ConsoleColor.Gray;
                                //Console.Write(data.Substring(start, end - start + 1)+" ");

                                tokenList[index].value = data.Substring(start, end - start + 1);
                                tokenList[index].kind = testTypKind(tokenList[index].value);
                                tokenList[index++].line = curLine;
                            }
                            else
                            {
                                throw new Exception("line " + curLine + ": Unexpected symbol \"" + data[i+1] + "\"");
                            }
                        }
                            /*
                                stringMode = !stringMode;

                            if (stringMode) dstData[iDst++] = data[iSrc];
                            else if (data[iSrc] == ' ' || data[iSrc] == '\r' || data[iSrc] == ';' || data[iSrc] == ';') dstData[iDst++] = '\n';
                            else if (data[iSrc] == ',' || data[iSrc] == '{' || data[iSrc] == '}' || data[iSrc] == '[' || data[iSrc] == ']' || data[iSrc] == '=' || data[iSrc] == '+' || data[iSrc] == '-' || data[iSrc] == '*' || data[iSrc] == '/' || data[iSrc] == ':' || data[iSrc] == '<' || data[iSrc] == '>' || data[iSrc] == ';')
                            {
                                dstData[iDst++] = '\n';
                                dstData[iDst++] = data[iSrc];
                                dstData[iDst++] = '\n';
                            }
                            else dstData[iDst++] = data[iSrc];
                            */

                    }
                }
                else if (commentMode == 1 && data[i] == '\n') commentMode = 0;
                else if (commentMode == 2 && data[i] == '*' && data[i + 1] == '/') { commentMode = 0; i++; }
            }
            tokenList.Length = index;

            //Array.Resize(ref tokenList, index);

            
            //Console.WriteLine(tokenList.Length);
            /*
            for (int i = 0; i < tokenList.Length; i++)
            {
                bool space = false;
                switch (tokenList[i].kind)
                {
                    case TypKind.Command:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case TypKind.Number:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case TypKind.String:
                        Console.ForegroundColor = ConsoleColor.Red;
                        space = true;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        space = true;
                        break;
                }
                Console.Write(tokenList[i].value + (space?" ":""));
            }
            Console.WriteLine();
            */
            

        }


        private void pharseAttributes(string fnname)
        {
            int index = searchTokenIndex(fnname);
            if (index == -1) return;
            index += 2;
            while (tokenList[index].value != "}")
            {
                //Console.ForegroundColor = ConsoleColor.Green;
                bool array = false;
                string type;
                if (tokenList[index + 1].value != "=")
                {
                    type = tokenList[index++].value;
                    if (tokenList[index].value == "[")
                    {
                        array = true; index += 2;
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

                        attributesTyp[attributesIndex] = typ;
                        attributesArray[attributesIndex] = array;
                        attributesName[attributesIndex] = tokenList[index].value;
                        if (tokenList[index + 1].value == "=")
                        {
                            index += 2;
                            value = getValue(ref index, attributesIndex);
                            attributesInitValue[attributesIndex] = value;
                        }
                        else
                        {
                            attributesInitValue[attributesIndex] = defaultTypValue(typ,array);
                        }
                        attributesIndex++;

                        //Console.WriteLine(type + "[" + array + "]->" + name + (value != null ? ("=" + value) : "") + ";");
                    } while (tokenList[index + 1].value == ",");

                }
                else if (tokenList[index + 1].value == "=")
                {
                    int attri = compareNames(tokenList[index].value, attributesName);
                    if (attri == -1) throw new Exception("line " + tokenList[index].line + ": Attribute \"" + tokenList[index].value + "\" is not defined");
                    index += 2;
                    attributesInitValue[attri] = getValue(ref index, attri);

                    //Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine(attributesName[attri]+"="+attributesInitValue[attri]+";");
                }

                //Console.ForegroundColor = ConsoleColor.Blue;
                //Console.WriteLine(tokenList[index].value);
                index++;
            }
            //Console.WriteLine("attributesIndex " + attributesIndex);
        }

        private void pharseEnums()
        {
            for (int index = 0; index < tokenList.Length; index++)
            {
                if (tokenList[index].value == "enum")
                {
                    int value = 0;
                    string group = tokenList[index+1].value,name = "";
                    index += 3;
                    while (tokenList[index].value != "}")
                    {
                        name = tokenList[index].value;
                        if (tokenList[index+1].value == "=")
                        {
                            index += 2;
                            value = (int)readNativeValue(1, ref index);
                        }
                        enumNames[enumIndex] = group + '.' + name;
                        enumValue[enumIndex++] = value;
                        if (tokenList[index+1].value == ",")
                        {
                            value++;
                            index++;
                        }
                        index++;
                    }
                }
            }
        }

        private void pharseObjectDeclaretions()
        {
            for (int i = 0; i < tokenList.Length; i++)
            {
                if (tokenList[i].value != "<") continue;
                string name = tokenList[i+1].value;
                string pid = null;i += 3;
                if (tokenList[i].value == ":")
                {
                    pid = tokenList[i + 1].value; i+=2;
                }
                objectNames[objectsIndex] = name;
                results[objectsIndex].Declare(i, pid);
                objectsIndex++;
            }
        }
        private void parseObjectInitialization()
        {
            for (int i = 0; i < objectsIndex; i++) results[i].Define(attributesIndex);
            for (int i = 0; i < objectsIndex; i++) pharseObject(i);
        }
        private void pharseObject(int id)
        {
            int index = results[id].Pos;
            if (results[id].State == 2) return;
            else if (results[id].State == 1) throw new Exception("line " + tokenList[index].line + ": Object <" + objectNames[id] + "> is already in process");

            results[id].State = 1;
            string parent = results[id].ParentName;

            if (parent != null)
            {
                int pid = compareNames(parent, objectNames);
                if (pid == -1) throw new Exception("line " + tokenList[index].line + ": inheriting object <"+ parent+"> is not defined");
                pharseObject(pid);
                for (int i = 0; i < attributesIndex; i++) results[id].AttributesValue[i] = results[pid].AttributesValue[i];
            }
            else
            {
                for (int i = 0; i < attributesIndex; i++) results[id].AttributesValue[i] = attributesInitValue[i];
            }

            index += 1;
            while (tokenList[index].value != "}")
            {
                int attri = compareNames(tokenList[index].value, attributesName);
                if (attri == -1) throw new Exception("line " + tokenList[index].line + ": Attribute \"" + tokenList[index].value + "\" in <" + objectNames[id] + "> is not defined");
                index += 2;
                switch (tokenList[index - 1].value)
                {
                    case "=":
                        results[id].AttributesValue[attri] = getValue(ref index, attri);
                        break;
                    case "+":
                        if (attributesArray[attri])
                        {
                            results[id].AttributesValue[attri] = combineArray(attributesTyp[attri], results[id].AttributesValue[attri], getValue(ref index, attri));
                        }
                        break;
                    default:
                        throw new Exception("line " + tokenList[index].line + ": Unexpected token \"" + tokenList[index-1].value + "\" in <" + objectNames[id] + ">");
                }
                index++;
            }
            results[id].State = 2;
        }
    }
}