﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JackAnalyser
{
    class CompilationEngine
    {
        private enum tokenType { KEYWORD, SYMBOL, IDENTIFIER, INT_CONST, STRING_CONST, ERROR };
        private int whileLblCount, ifLabelCount;
        private struct token
        {
            public String theToken;
            public tokenType theTokenType;
        };
        private token currentToken, nextToken;
        private StreamReader theReader;
        private StreamWriter theWriter;
        private VMWriter vmW;
        private Boolean tokensExist = true;
        private LinkedList<SymbolTable> tableList;
        private SymbolTable.identifier currentVar;
        private String className;
        Boolean writeSymbolInfo = false;

        public CompilationEngine(StreamReader sr, StreamWriter sw)
        {
            theReader = sr;
            theWriter = sw;
            tableList = new LinkedList<SymbolTable>();
            vmW = new VMWriter(sw);
        }

        public Boolean CompiletheTokens()
        {
            Boolean result;

            String line = theReader.ReadLine(); // the first line of the file contains <tokens>
            line = theReader.ReadLine();        // load in the first token
            nextToken = getTheToken(line);

            if (tokensExist)
                advance();
            if ((currentToken.theTokenType != tokenType.KEYWORD) && (currentToken.theToken != "class"))
                result = false;
            else
                result = CompileClass();
            return result;
        }

        private Boolean LookupVar(String n)
        {
            SymbolTable.identifier id = new SymbolTable.identifier();

            LinkedListNode<SymbolTable> current = tableList.Last;
            do
            {
                SymbolTable sT = current.Value;
                id = sT.FindVar(n);
                if (id.iName != "null")
                    break;
            } while ((current = current.Previous) != null);
            if (id.iName != "null")
            {
                currentVar = id;
                return true;
            }
            else
            {
                Console.WriteLine("Cannot find variable " + n + " in the symbol tables.");
                return false;
            }
        }

        private static string indent(int count)
        {
            return "".PadLeft(count);
        }
        private Boolean CompileClass() // arriving here means that the keyword 'class' has been read
        {
            Boolean result = true;

            whileLblCount = -1;
            ifLabelCount = -1;
            SymbolTable classTable = new SymbolTable();
            tableList.AddFirst(classTable);
            advance();
            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            else
            {
                className = currentToken.theToken;
                advance();
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                    return false;
                else
                {
                    advance();
                    while(!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                    {
                        if (currentToken.theTokenType != tokenType.KEYWORD)
                            return false;
                        else
                        {
                            switch (currentToken.theToken)
                            {
                                case "static":
                                    currentVar.iKind = SymbolTable.varTypes.STATIC;
                                    if (!CompileClassVarDec())
                                        return false;
                                    break;
                                case "field":
                                    currentVar.iKind = SymbolTable.varTypes.FIELD;
                                    if (!CompileClassVarDec())
                                        return false;
                                    break;
                                case "constructor":
                                case "function":
                                case "method":
                                    if (!CompileSubroutineDec())
                                        return false;
                                    break;
                                default:
                                    return false;
                            }
                        }
                        advance();
                    }
                }
            }
            return result;
        }

        private Boolean CompileSubroutineDec()
        {
            Boolean result = true;
            SymbolTable subTable;
            Boolean isMethod = false, isConstructor = false, isVoid = false;
            
            subTable = new SymbolTable();
            tableList.AddLast(subTable);
            if(currentToken.theToken == "method")
            {
                isMethod = true;
                currentVar.iKind = SymbolTable.varTypes.ARG;
                currentVar.iName = "this";
                currentVar.iType = className;
                currentVar.iIndex = 0;
                tableList.Last.Value.newVar("this", currentVar);
                tableList.Last.Value.argCount++;
            }
            if (currentToken.theToken == "constructor")
            {
                isConstructor = true;
            }
            advance();
            if ((currentToken.theTokenType == tokenType.KEYWORD) && (currentToken.theToken == "void"))
                //                theWriter.WriteLine(indent(spaces)  + "<keyword> void </keyword>");
                isVoid = true;
            else
                if (!CompileType())
                return false;
            advance();
            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            theWriter.Write("function " + className + "." + currentToken.theToken);
            advance();
            if ((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "("))
                //                theWriter.WriteLine(indent(spaces)  + "<symbol> ( </symbol>");
                result = true;
            else
                return false;
            advance();
            if (!CompileParameterList())
                return false;
            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                return false;
            advance();
            if (!CompileVarDecs())
                return false;
            theWriter.WriteLine(" " + tableList.Last.Value.varCount.ToString());    // finish the function declaration
            if(isConstructor)
            {
                theWriter.WriteLine("push constant " + tableList.First.Value.fieldCount.ToString());
                theWriter.WriteLine("call Memory.alloc 1");
                theWriter.WriteLine("pop pointer 0");   // now "this" is defined
            }
            if (isMethod)
            {
                theWriter.WriteLine("push argument 0");
                theWriter.WriteLine("pop pointer 0");
            }
            while (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
            {
                if (currentToken.theTokenType != tokenType.KEYWORD)
                    return false;
                switch (currentToken.theToken)
                {
                    case "let":
                        if (!CompileLetStatement())
                            return false;
                        break;
                    case "if":
                        if (!CompileIfStatement(++ifLabelCount))
                            return false;
                        break;
                    case "while":
                        if (!CompileWhileStatement(++whileLblCount))
                            return false;
                        break;
                    case "do":
                        if (!CompileDoStatement())
                            return false;
                        break;
                    case "return":
                        if ((!isMethod && !isConstructor) || (isMethod && isVoid))
                            theWriter.WriteLine("push constant 0"); //dummy return of a variable
                        if (!CompileReturnStatement())
                            return false;
                        break;
                    default:
                        return false;
                }
            }
           if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                return false;
             foreach(KeyValuePair<String, SymbolTable.identifier> entry in tableList.Last.Value.theSymbolTable)
            {
                currentVar = entry.Value;
                if (writeSymbolInfo)
                    WriteVarXML();
            }
            tableList.RemoveLast();
            whileLblCount = -1;
            ifLabelCount = -1;
            return result;
        }

        private Boolean CompileStatements()
        {
            Boolean result = true;

            while (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
            {
                if (currentToken.theTokenType != tokenType.KEYWORD)
                    return false;
                switch (currentToken.theToken)
                {
                    case "let":
                        if (!CompileLetStatement())
                            return false;
                        break;
                    case "if":
                        if (!CompileIfStatement(++ifLabelCount))
                            return false;
                        break;
                    case "while":
                        if (!CompileWhileStatement(++whileLblCount))
                            return false;
                        break;
                    case "do":
                        if (!CompileDoStatement())
                            return false;
                        break;
                    case "return":
                        if (!CompileReturnStatement())
                            return false;
                        break;
                    default:
                        return false;
                }
            }
            return result;
        }

        private Boolean CompileLetStatement()
        {
            Boolean result = true, isArray = false;
            SymbolTable.identifier target;

            advance();
            if (!(currentToken.theTokenType == tokenType.IDENTIFIER))
                return false;
            LookupVar(currentToken.theToken);
            target = currentVar;
            advance();
            if (!(currentToken.theTokenType == tokenType.SYMBOL))
                return false;
            if (currentToken.theToken == "[")
            {
                isArray = true;
                vmWriterPush(target);
                advance();
                if (!CompileExpression())
                    return false;
                vmW.writeArithmetic(VMWriter.cmdType.ADD);
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "]")))
                    return false;
                advance();
            }
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "=")))
                return false;
            advance();
            if (!CompileExpression())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ";")))
                return false;

            if (isArray)
            {
                vmW.writePop(VMWriter.segType.TEMP, 0);
                vmW.writePop(VMWriter.segType.POINTER, 1);
                vmW.writePush(VMWriter.segType.TEMP, 0);
                vmW.writePop(VMWriter.segType.THAT, 0);
            }
            else
                vmWriterPop(target);
            advance();
            return result;
        }

        private Boolean CompileDoStatement()
        {
            Boolean result = true;

            advance();
            if (!CompileSubroutineCall(true))
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ";")))
                return false;
            advance();
            vmW.writePop(VMWriter.segType.TEMP, 0);
            return result;
        }

        private Boolean CompileSubroutineCall(Boolean isFunction)
        {
            Boolean result = true;
            String subCall = "call ";
            int nArgs;
            Boolean isObject = false;
            Boolean isMethod = false;

            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            if((nextToken.theTokenType == tokenType.SYMBOL) && (nextToken.theToken == "."))
            {
                if (!LookupVar(currentToken.theToken))
                {
                    subCall = subCall + currentToken.theToken + ".";
                    isObject = false;
                }
                else
                {
                    subCall = subCall + currentVar.iType + ".";
                    isObject = true;
                }
                advance();
                advance();
            }
            else
            {
                if (!LookupVar(currentToken.theToken))
                {
                    subCall = subCall + className + ".";
                    isObject = false;
                    isMethod = true;
                }
                else
                {
                    subCall = subCall + currentVar.iType + ".";
                    isObject = true;
                }
            }
            if (isObject)
            {
                vmWriterPush(currentVar);
            }
            if (isMethod)
            {
                vmW.writePush(VMWriter.segType.POINTER, 0);
            }
            subCall += currentToken.theToken;
            subCall += " ";
           
            advance();
            if (!(currentToken.theTokenType == tokenType.SYMBOL))
                return false;
            switch (currentToken.theToken)
            {
                case "(":
                    advance();
                    if (!CompileExpressionList(out nArgs))
                        return false;
                    break;
                default:
                    return false;
            }
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                return false;
            if (isObject || isMethod)
                nArgs++;
            subCall += nArgs.ToString();
            theWriter.WriteLine(subCall);
            advance();
            return result;
        }

        private void vmWriterPush(SymbolTable.identifier id)
        {
            switch (id.iKind)
            {
                case SymbolTable.varTypes.FIELD:
                    vmW.writePush(VMWriter.segType.THIS, id.iIndex);
                    break;
                case SymbolTable.varTypes.VAR:
                    vmW.writePush(VMWriter.segType.LOCAL, id.iIndex);
                    break;
                case SymbolTable.varTypes.STATIC:
                    vmW.writePush(VMWriter.segType.STATIC, id.iIndex);
                    break;
                case SymbolTable.varTypes.ARG:
                    vmW.writePush(VMWriter.segType.ARG, id.iIndex);
                    break;
                default:
                    Console.WriteLine("Unprepared for push segtype " + id.iKind);
                    break;
            }
        }

        private void vmWriterPop(SymbolTable.identifier id)
        {
            switch (id.iKind)
            {
                case SymbolTable.varTypes.FIELD:
                    vmW.writePop(VMWriter.segType.THIS, id.iIndex);
                    break;
                case SymbolTable.varTypes.VAR:
                    vmW.writePop(VMWriter.segType.LOCAL, id.iIndex);
                    break;
                case SymbolTable.varTypes.STATIC:
                    vmW.writePop(VMWriter.segType.STATIC, id.iIndex);
                    break;
                case SymbolTable.varTypes.ARG:
                    vmW.writePop(VMWriter.segType.ARG, id.iIndex);
                    break;
                default:
                    Console.WriteLine("Unprepared for pop segtype " + id.iKind);
                    break;
            }
        }

        private Boolean CompileWhileStatement(int count)
        {
            Boolean result = true;
            

            advance();
            theWriter.Write("label WHILE_EXP");
            theWriter.WriteLine(count.ToString());
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "(")))
                return false;
            advance();
            if (!CompileExpression())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                return false;
            advance();
            theWriter.WriteLine("not");
            theWriter.Write("if-goto WHILE_END");
            theWriter.WriteLine(count.ToString());
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                return false;
            advance();
            if (!CompileStatements())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                return false;
            theWriter.Write("goto WHILE_EXP");
            theWriter.WriteLine(count.ToString());

            theWriter.Write("label WHILE_END");
            theWriter.WriteLine(count.ToString());
            advance();
            return result;
        }

        private Boolean CompileExpressionList(out int nInList)
        {
            Boolean result = true, firstExp = true;
            nInList = 0;

            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))   // if not an empty list
            {
                do
                {
                    if (firstExp)
                        firstExp = false;
                    else
                    {
                        advance();
                    }
                    if (!CompileExpression())
                        return false;
                    nInList++;
                } while (((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ",")));

            }
            return result;
        }

        private Boolean CompileExpression()
        {
            Boolean result = true;
            String op = "";

            if (!CompileTerm())
                return false;
            if(currentToken.theTokenType == tokenType.SYMBOL)
                switch (currentToken.theToken)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                    case "&amp;":
                    case "|":
                    case "&lt;":
                    case "&gt;":
                    case "=":
                        op = currentToken.theToken;
                        advance();
                        if (!CompileTerm())
                            return false;
                        break;
                    case ";":
                    case ")":
                    case "]":
                    case ",":
                        break;
                    default:
                        return false;
                }
            if(op != "")
                vmW.writeArithmetic(op);
            return result;
        }

        private Boolean CompileTerm()
        {
            Boolean result = true, doAdvance = true; ;

            switch (currentToken.theTokenType)
            {
                case tokenType.IDENTIFIER:
                    if (nextToken.theTokenType == tokenType.SYMBOL)
                    {
                        switch(nextToken.theToken)
                        {
                            case "[":
                                LookupVar(currentToken.theToken);
                                vmWriterPush(currentVar);
                                advance();
                                advance();
                                if (!CompileExpression())
                                    return false;
                                vmW.writeArithmetic(VMWriter.cmdType.ADD);
                                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "]")))
                                    return false;
                                theWriter.WriteLine("pop pointer 1");
                                theWriter.WriteLine("push that 0");
                                break;
                            case ".":
                            case "(":
                                if (!CompileSubroutineCall(false))
                                    return false;
                                doAdvance = false;
                                break;
                            default:
                                LookupVar(currentToken.theToken);
                                vmWriterPush(currentVar);
                                break;
                       }

                    }
                    else
                    {
                        LookupVar(currentToken.theToken);
                        vmWriterPush(currentVar);
                    }
                    break;
                case tokenType.KEYWORD:
                    switch(currentToken.theToken)
                    {
                        case "true":
                            vmW.writePush(VMWriter.segType.CONST, 0);
                            vmW.writeArithmetic(VMWriter.cmdType.NOT);
                            break;
                        case "false":
                        case "null":
                            vmW.writePush(VMWriter.segType.CONST, 0);
                            break;
                        case "this":
                            vmW.writePush(VMWriter.segType.POINTER, 0);
                            break;
                        default:
                            return false;
                    }
                    break;
                case tokenType.SYMBOL:
                    if((currentToken.theToken == "~") || (currentToken.theToken == "-")) //unary operator
                    {
                        String op = currentToken.theToken;
                        advance();
                        if (!CompileTerm())
                            return false;
                        if (op == "-")
                            vmW.writeArithmetic(VMWriter.cmdType.NEG);
                        else
                            vmW.writeArithmetic(VMWriter.cmdType.NOT);
                        doAdvance = false;
                        break;
                    }
                    else
                    {
                        if (currentToken.theToken == "(")   //Expression in brackets
                        {
                            advance();
                            if (!CompileExpression())
                                return false;
                            if (currentToken.theToken != ")")
                                return false;
                        }
                    }
                    break;
                case tokenType.INT_CONST:
                    int x = 0;
                    Int32.TryParse(currentToken.theToken, out x);
                    vmW.writePush(VMWriter.segType.CONST, x);
                    break;
                case tokenType.STRING_CONST:
                    vmW.writePush(VMWriter.segType.CONST, currentToken.theToken.Length);
                    theWriter.WriteLine("call String.new 1");   // returns the address of the string at the top of the stack
                    foreach (char c in currentToken.theToken)
                    {
                        vmW.writePush(VMWriter.segType.CONST, (int)c);
                        theWriter.WriteLine("call String.appendChar 2");
                        //ends with the address of the string at the top of the stack (I hope)
                    }
                    break;
                default:
                    return false;
            }
            if (doAdvance)
                advance();
            return result;
        }
        private Boolean CompileIfStatement(int count)
        {
            Boolean result = true;

            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "(")))
                return false;
            advance();
            if (!CompileExpression())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                return false;
            //            theWriter.WriteLine(indent(spaces)  + "<symbol> ) </symbol>");
            theWriter.Write("if-goto IF_TRUE");
            theWriter.WriteLine(count.ToString());
            theWriter.Write("goto IF_FALSE");
            theWriter.WriteLine(count.ToString());
            theWriter.Write("label IF_TRUE");
            theWriter.WriteLine(count.ToString());
            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                return false;
            advance();
            if (!CompileStatements())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                return false;
            advance();
            if ((currentToken.theTokenType == tokenType.KEYWORD) && (currentToken.theToken == "else"))
            {
                theWriter.Write("goto IF_END");
                theWriter.WriteLine(count.ToString());
                theWriter.Write("label IF_FALSE");
                theWriter.WriteLine(count.ToString());
                advance();
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                    return false;
                advance();
                if (!CompileStatements())
                    return false;
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                    return false;
                theWriter.Write("label IF_END");
                theWriter.WriteLine(count.ToString());
               advance();
            }
            else
            {
                theWriter.Write("label IF_FALSE");
                theWriter.WriteLine(count.ToString());
            }
            return result;
        }
        private Boolean CompileReturnStatement()
        {
            Boolean result = true;

            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ";")))
            {
                if (!CompileExpression())
                    return false;
            }
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ";")))
                return false;
            advance();
            theWriter.WriteLine("return");
            return result;
        }

        private Boolean CompileVarDecs()
        {
            Boolean result = true;
            while ((currentToken.theTokenType == tokenType.KEYWORD) && (currentToken.theToken == "var"))
            {
                currentVar.iKind = SymbolTable.varTypes.VAR;
                advance();
                if (!CompileType())
                    return false;
                while (true)
                {
                    advance();
                    if (currentToken.theTokenType != tokenType.IDENTIFIER)
                        return false;
                    currentVar.iName = currentToken.theToken;
                    currentVar.iIndex = tableList.Last.Value.varCount;
                    tableList.Last.Value.newVar(currentVar.iName, currentVar);
                    tableList.Last.Value.varCount++;
                    advance();
                    if (currentToken.theTokenType != tokenType.SYMBOL)
                        return false;
                    if (currentToken.theToken == ";")
                        break;
                    if (currentToken.theToken != ",")
                        return false;
                }
                advance();
            }
            return result;
        }

        private Boolean CompileParameterList()
        {
            Boolean result = true;

            while(!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
            {
                if (!CompileType())
                    return false;
                advance();
                if (!(currentToken.theTokenType == tokenType.IDENTIFIER))
                    return false;
                currentVar.iName = currentToken.theToken;
                currentVar.iKind = SymbolTable.varTypes.ARG;
                currentVar.iIndex = tableList.Last.Value.argCount;
                tableList.Last.Value.newVar(currentVar.iName, currentVar);
                tableList.Last.Value.argCount++;
                advance();
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                {
                    if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ",")))
                        return false;
                    advance();
                }
            }
            return result;
        }

        private void WriteVarXML()
        {
            theWriter.WriteLine( "<symboltable_entry> ");
            theWriter.WriteLine( "<varname> " + currentVar.iName + " </varname>");
            theWriter.WriteLine( "<varkind> " + currentVar.iKind + " </varkind>");
            theWriter.WriteLine( "<vartype> " + currentVar.iType + " </vartype>");
            theWriter.WriteLine( "<varindex> " + currentVar.iIndex + " </varindex>");
            theWriter.WriteLine( "</symboltable_entry> ");
        }
        
        private Boolean CompileClassVarDec()
        {
            Boolean result = true;

            advance();
            if (!CompileType())
                return false;
            while(true)
            {
                advance();
                if (currentToken.theTokenType != tokenType.IDENTIFIER)
                    return false;
                currentVar.iName = currentToken.theToken;
                if (currentVar.iKind == SymbolTable.varTypes.STATIC)
                {
                    currentVar.iIndex = tableList.First.Value.staticCount;
                    tableList.First.Value.staticCount++;
                }
                else   // There are only two choices here...
                {
                    currentVar.iIndex = tableList.First.Value.fieldCount;
                    tableList.First.Value.fieldCount++;
                }
                tableList.First.Value.newVar(currentVar.iName, currentVar); //Add the new variable to the class symbol table
                if (writeSymbolInfo)
                    WriteVarXML();
                advance();
                if (currentToken.theTokenType != tokenType.SYMBOL)
                    return false;
                if (currentToken.theToken == ";")
                    break;
                if (currentToken.theToken != ",")
                    return false;
            }
            return result;
        }

        private Boolean CompileType()
        {
            Boolean result = true;
            if ((currentToken.theTokenType == tokenType.KEYWORD) || (currentToken.theTokenType == tokenType.IDENTIFIER))
            {
                if (currentToken.theTokenType == tokenType.KEYWORD)
                {
                    switch (currentToken.theToken)
                    {
                        case "int":
                        case "char":
                        case "boolean":
                            break;
                        default:
                            return false;   // not an allowable keyword for 'type'
                    }
                }
                else
                    result = true;

                currentVar.iType = currentToken.theToken;
            }
            else
                return false;
            return result;
        }

        private void advance()
        {
            currentToken = nextToken;
            String line = theReader.ReadLine().Trim();
            if (line.Contains("/token"))
                return;
            if (line != null)
                nextToken = getTheToken(line);
            else
                tokensExist = false;
        }

        private token getTheToken(String inStr)
        {
            token tt;

            String theType = inStr.Substring(inStr.IndexOf("<") + 1, inStr.IndexOf(">") - 1);
            String tinStr = inStr.Substring(inStr.IndexOf("<") + 1);
            tt.theToken = tinStr.Substring(tinStr.IndexOf(">") + 2, tinStr.IndexOf("<") - tinStr.IndexOf(">") - 3);
            switch (theType)
            {
                case "keyword":
                    tt.theTokenType = tokenType.KEYWORD;
                    break;
                case "symbol":
                    tt.theTokenType = tokenType.SYMBOL;
                    break;
                case "identifier":
                    tt.theTokenType = tokenType.IDENTIFIER;
                    break;
                case "stringConstant":
                    tt.theTokenType = tokenType.STRING_CONST;
                    break;
                case "integerConstant":
                    tt.theTokenType = tokenType.INT_CONST;
                    break;
                default:
                    Console.WriteLine("Unidentified token type: " + theType);
                    tt.theTokenType = tokenType.ERROR;
                    break;
            }
            return tt;
        }
    }
}
