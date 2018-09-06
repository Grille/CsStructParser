using System.IO;
using System;
using System.Threading;
namespace GGL.IO
{
    public partial class Parser
    {
        private int length = 0;
        private char[] data;

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
            pharseEnum();
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
                else if (data[iSrc] == ',' || data[iSrc] == '{' || data[iSrc] == '}' || data[iSrc] == '[' || data[iSrc] == ']' || data[iSrc] == '=' || data[iSrc] == '+' || data[iSrc] == '-' || data[iSrc] == '*' || data[iSrc] == '/' || data[iSrc] == ':' || data[iSrc] == '<' || data[iSrc] == '>')
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
            int pos = searchString("Attributes");
            if (pos == -1) return;
            int scope = 0;

            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
            // cond = a.a >= 9
            byte typ = 0;
            byte mode = 0;
            byte array = 0;
            do
            {
                pos = nextItem(pos);
                switch (data[pos])
                {
                    case '{': scope++; break;
                    case '}': scope--; break;
                    case '[': mode = 2; break;
                    case ']': mode = 1; break;
                    case ',': if (mode == 0) mode = 1; break;
                    default:
                        if (mode == 0)
                        {
                            //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
                            if (data[pos] == 'b' && data[pos+1] == 'y') typ = 0;
                            else if (data[pos] == 'i') typ = 1;
                            else if (data[pos] == 'f') typ = 2;
                            else if (data[pos] == 'd') typ = 3;
                            else if (data[pos] == 'b' && data[pos+1] == 'o') typ = 4;
                            else if (data[pos] == 's') typ = 5;
                            else if (data[pos] == 'v') typ = 6;
                            array = 0;
                            mode = 1;
                        }
                        else if (mode == 1)
                        {
                            attributesTyp[attributesIndex * 2] = typ;
                            attributesTyp[attributesIndex * 2 + 1] = array;
                            attributesName[attributesIndex] = getItem(pos);
                            attributesInitValue[attributesIndex] = "s";
                            attributesIndex++;
                            mode = 0;
                        }
                        break;
                }
                if (mode == 2) array++;

            } while (scope == 1);

            attributesInitValue = new Object[attributesIndex];
            Result.AttributesNumber = attributesIndex;

            //throw new Exception("Put your error message here.");
            
            Console.WriteLine();
            for (int i = 0; i < attributesIndex; i++)
            {
                Console.WriteLine("typ:" + attributesTyp[i * 2] + "[" + attributesTyp[i * 2 + 1] + "] " + attributesName[i]+" = "+ attributesInitValue[i]);
            }
            

        }

        private void pharseInit()
        {
            int pos = searchString("Init");
            if (pos == -1) return;

            int scope = 0;
            int attri = 0;
            byte mode = 0;
            do
            {
                pos = nextItem(pos);
                switch (data[pos])
                {
                    case '{': scope++; break;
                    case '}': scope--; break;
                    case '=': mode = 1; break;
                    default:
                        switch (mode)
                        {
                            case 0: attri = compareNames(getItem(pos),attributesName,attributesIndex);break;
                            case 1:
                                attributesInitValue[attri] = getValue(ref pos, attri);
                                mode = 0;
                                break;
                        }
                        break;
                }
            } while (scope == 1);
        }

        private void pharseEnum()
        {
            int pos = searchString("Enum");
            if (pos == -1) return;

            string group = "";
            int value = 0;
            int scope = 0;
            do
            {
                pos = nextItem(pos);
                switch (data[pos])
                {
                    case '{': scope++; break;
                    case '}': scope--; break;
                    case ',': value++; break;
                    default:
                        if (scope == 1)
                        {
                            group = getItem(pos);
                            value = 0;
                        }
                        else if (scope == 2)
                        {
                            string name = getItem(pos);
                            if (getItem(nextItem(pos)) == "=")
                            {
                                pos = nextItem(nextItem(pos));
                                value = (int)convertTyp(1, ref pos);
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
            int pos = 0;
            globalPos = 0;
            while (true)
            {
                //search next ID

                searchString("<", globalPos);
                if (globalPos == -1) break;

                nextItem();
                int startPos = globalPos, pid = -1;
                string name = getItem();
                nextItem();
                if (data[nextItem()] == ':') pid = compareNames(getItem(nextItem()),objectsName);

                objectsName[objectsIndex] = name;
                results[objectsIndex].Init(startPos, pid);
                objectsIndex++;
            }
            for (int i = 0; i < 256; i++) pharseObject(i);

            /*
            for (int i = 0; i < 256; i++)
            {
                if (!results[i].Used) continue;
                Console.WriteLine("\n<objectID " + i+">");
                for (int ia = 0; ia < attributesLenght; ia++)
                    Console.WriteLine(attributesName[ia] + " = "+ results[i].AttributesValue[ia]);
            }
            */

        }
        private void pharseObject(int id)
        {
            if (!results[id].Used || results[id].State == 2) return;

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

            int pos = searchString("{", results[id].Pos);
            int scope = 0;
            int mode = 0;
            int attri = 0;
            do
            {
                switch (data[pos])
                {
                    case '{': scope++; break;
                    case '}': scope--; break;
                    case '=': mode = 1; break;
                    case '+': mode = 2; break;
                    default:
                        switch (mode)
                        {
                            case 0:
                                attri = compareNames(getItem(pos), attributesName, attributesIndex);
                                //if (attri == -1) throw new Exception("Attribute \""+ getItem(pos)+"\" in <"+objectsName[id]+"> not found!");
                                break;
                            case 1:
                                results[id].AttributesValue[attri] = getValue(ref pos,attri);
                                mode = 0;
                                break;
                            case 2:
                                if (attributesTyp[attri * 2 + 1] > 0)
                                {
                                    results[id].AttributesValue[attri] = combineArray(attributesTyp[attri * 2],results[id].AttributesValue[attri], getValue(ref pos, attri));
                                }
                                mode = 0;
                                break;
                        }
                        break;
                }
                pos = nextItem(pos);
            } while (scope == 1);
            results[id].State = 2;
        }
    }
}