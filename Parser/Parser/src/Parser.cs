using System.IO;
using System;
using System.Threading;
using System.Reflection;
using Grille.Parsing;
using System.Collections.Generic;
namespace Grille.Parsing.Tcf
{
    public partial class TcfParser
    {
        TokenList tokens;

        public Dictionary<string, TcfType> Types { get; }
        public Dictionary<string, int> Constants { get; }

        int objectIndex;
        List<TcfObjectParserCtx> objects;
        public Dictionary<string, TcfObject> Result { get; private set; }

        public TcfType DefaultType { get; }

        bool codeLoaded, objectDeclaretionsParsed, attributesParsed, objectInitializationParsed;

        public string GlobalTypeDefName { get; set; } = "Attributes";

        public TcfParser()
        {
            DefaultType = new TcfType("0_default");
            Types = new Dictionary<string, TcfType>();
            Constants = new Dictionary<string, int>();
            objects = new List<TcfObjectParserCtx>();
            Result = new Dictionary<string, TcfObject>();
        }

        private void parse(string code)
        {
            tokens = new TokenList(Lexer.Tokenize(code));

            parseDefinitions();
            parseInitializations();

            while (objectIndex < objects.Count) {
                var ctx = objects[objectIndex];
                Result[ctx.Name] = ctx.Object;
                objectIndex += 1;
            }
        }

        private void parseTypeDef(int index, TcfType cfgtype)
        {
            assertToken(index, TokenKind.Symbol, "{");

            index += 1;
            while (tokens[index].Value != "}")
            {
                //Console.ForegroundColor = ConsoleColor.Green;
                bool array = false;
                string type;
                if (tokens[index + 1].Value != "=")
                {
                    type = tokens[index++].Value;
                    if (tokens[index].Value == "[")
                    {
                        array = true; index += 2;
                    }

                    TypeName typ = TypeName.Var; //0 byte, 1 int, 2 float, 3 double, 4 bool, 5 string, 6 var,7 cond
                    if (type[0] == 'b' && type[1] == 'y') typ = TypeName.Byte;
                    else if (type[0] == 'i') typ = TypeName.Int;
                    else if (type[0] == 'f') typ = TypeName.Float;
                    else if (type[0] == 'd') typ = TypeName.Double;
                    else if (type[0] == 'b' && type[1] == 'o') typ = TypeName.Bool;
                    else if (type[0] == 's') typ = TypeName.String;
                    else if (type[0] == 'r') typ = TypeName.Ref;
                    else if (type[0] == 'v') typ = TypeName.Var;

                    int i = 0;
                    do
                    {
                        if (i++ > 0) index += 2;
                        string name = tokens[index].Value;
                        object value;

                        if (tokens[index + 1].Value == "=")
                        {
                            index += 2;
                            value = getValue(ref index, typ, array);
                        }
                        else
                        {
                            value = defaultTypValue(typ, array);
                        }

                        cfgtype.CreateProperty(typ, array, name, value);

                        //Console.WriteLine(type + "[" + array + "]->" + name + (value != null ? ("=" + value) : "") + ";");
                    } while (tokens[index + 1].Value == ",");

                }
                else if (tokens[index + 1].Value == "=")
                {
                    int attri = findIndexByName(tokens[index].Value, cfgtype.Properties);
                    if (attri == -1) throw new TcfParserException(tokens[index], "Attribute \"" + tokens[index].Value + "\" is not defined");
                    index += 2;
                    var property = cfgtype.Properties[attri];
                    property.DefaultValue = getValue(ref index, property.Type, property.IsArray);

                    //Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine(attributesName[attri]+"="+attributesInitValue[attri]+";");
                }
                else
                {
                    throw new TcfParserException(tokens[index + 1]);
                }

                //Console.ForegroundColor = ConsoleColor.Blue;
                //Console.WriteLine(tokenList[index].value);
                index++;
            }
            //Console.WriteLine("attributesIndex " + attributesIndex);
        }

        private void parseDefinitions()
        {
            int scope = 0;
            for (int index = 0; index < tokens.Length; index++)
            {
                if (tokens[index].Equals(TokenKind.Symbol, "{"))
                {
                    scope += 1;
                }
                else if (tokens[index].Equals(TokenKind.Symbol, "}"))
                {
                    scope -= 1;
                    if (scope < 0)
                    {
                        throw new TcfParserException(tokens[index], "Scope -1");
                    }
                }

                else if (scope == 0)
                {
                    if (tokens[index].Equals(TokenKind.Word))
                    {
                        if (tokens[index].Equals(GlobalTypeDefName))
                        {
                            parseTypeDef(index + 1, DefaultType);
                        }
                        else if (tokens[index].Equals("const"))
                        {
                            index += 1;
                            string name = tokens[index].Value;

                            index += 1;
                            assertToken(index, TokenKind.Symbol, "=");

                            index += 1;
                            int value = (int)readNativeValue(TypeName.Int, ref index);

                            AddConst(name, value);
                        }
                        else if (tokens[index].Equals("enum"))
                        {
                            int value = 0;
                            string group = tokens[index + 1].Value, name;

                            assertToken(index + 2, TokenKind.Symbol, "{");

                            index += 3;
                            while (tokens[index].Value != "}")
                            {
                                name = tokens[index].Value;
                                if (tokens[index + 1].Value == "=")
                                {
                                    index += 2;
                                    value = (int)readNativeValue(TypeName.Int, ref index);
                                }

                                AddEnum(group, name, value);

                                if (tokens[index + 1].Value == ",")
                                {
                                    value++;
                                    index++;
                                }
                                index++;
                            }
                        }
                        else if (tokens[index].Equals("typedef"))
                        {
                            index += 1;
                            assertToken(index, TokenKind.Word);
                            var name = tokens[index].Value;
                            var type = new TcfType(name);

                            parseTypeDef(index + 1, type);
                            Types.Add(name, type);
                        }
                        else
                        {
                            var typename = tokens[index].Value;

                            if (Types.TryGetValue(typename, out var type))
                            {
                                index += 1;
                                string name = tokens[index].Value;
                                string parent = null;
                                if (tokens[index + 1].Value == ":")
                                {
                                    parent = tokens[index + 2].Value;
                                    index += 2;
                                }

                                assertToken(index + 1, TokenKind.Symbol, "{");

                                var obj = new TcfObjectParserCtx(type, name, parent, index + 1);
                                objects.Add(obj);
                            }
                            else
                            {
                                throw new TcfParserException(tokens[index], "Type not found.");
                            }

                        }
                    }
                    if (tokens[index].Equals(TokenKind.Symbol, "<"))
                    {
                        index += 1;
                        string name = tokens[index].Value;

                        index += 1;
                        assertToken(index, TokenKind.Symbol, ">");

                        string parent = null;
                        if (tokens[index + 1].Value == ":")
                        {
                            parent = tokens[index + 2].Value;
                            index += 2;
                        }

                        assertToken(index + 1, TokenKind.Symbol, "{");

                        var obj = new TcfObjectParserCtx(DefaultType, name, parent, index + 1);
                        objects.Add(obj);
                    }

                }
            }
        }

        private void parseInitializations()
        {
            for (int i = 0; i < objects.Count; i++) parseObject(objects[i]);
        }
        private void parseObject(TcfObjectParserCtx ctx)
        {

            var type = ctx.Object.Type;

            int index = ctx.TokenIndex;
            if (ctx.State == ParseStatus.Done) return;
            else if (ctx.State == ParseStatus.Parsing) throw new TcfParserException(tokens[index].Line, $"Object <{type.Name} {ctx.Name}> is already in process, possible circular reference.");

            ctx.State = ParseStatus.Parsing;
            string parentName = ctx.ParentName;

            ctx.Object.Init();
            var values = ctx.Values;

            if (parentName != null)
            {
                int pid = findIndexByName(parentName, objects);
                if (pid == -1)
                {
                    throw new TcfParserException(tokens[index].Line, "Inheriting object <" + parentName + "> is not defined");
                }

                var parrent = objects[pid];
                if (type != parrent.Object.Type)
                {
                    throw new TcfParserException(tokens[index].Line, $"<{type.Name} {ctx.Name}> can not inherent from <{parrent.Object.Type.Name} {parentName}>.");
                }

                parseObject(parrent);
                for (int i = 0; i < type.Properties.Count; i++) values[i] = parrent.Values[i];
            }
            else
            {
                for (int i = 0; i < type.Properties.Count; i++) values[i] = type.Properties[i].DefaultValue;
            }

            assertToken(index, TokenKind.Symbol, "{");
            index += 1;

            while (tokens[index].Value != "}")
            {
                int attri = findIndexByName(tokens[index].Value, type.Properties);
                if (attri == -1) throw new TcfParserException(tokens[index], "Attribute \"" + tokens[index].Value + "\" in <" + ctx.Name + "> is not defined.");
                index += 2;
                var property = type.Properties[attri];
                switch (tokens[index - 1].Value)
                {
                    case "=":
                        values[attri] = getValue(ref index, property.Type, property.IsArray);
                        break;
                    case "+":
                        if (property.IsArray)
                        {
                            if (values[attri] == null)
                                throw new TcfParserException(tokens[index], "Array \"" + type.Properties[attri].Name + "\" is null");
                            values[attri] = combineArray(property.Type, values[attri], getValue(ref index, property.Type, property.IsArray));
                        }
                        break;
                    default:
                        throw new TcfParserException(tokens[index], "Unexpected token \"" + tokens[index - 1].Value + "\" in <" + ctx.Name + ">");
                }
                index++;
            }
            ctx.State = ParseStatus.Done;
        }
    }
}
