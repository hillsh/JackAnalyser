using System;
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
        private int spaces = 0;
        private struct token
        {
            public String theToken;
            public tokenType theTokenType;
        };
        private token currentToken, nextToken;
        private StreamReader theReader;
        private StreamWriter theWriter;
        private Boolean tokensExist = true;

        public CompilationEngine(StreamReader sr, StreamWriter sw)
        {
            theReader = sr;
            theWriter = sw;
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

        private static string indent(int count)
        {
            return "".PadLeft(count);
        }
        private Boolean CompileClass() // arriving here means that the keyword 'class' has been read
        {
            Boolean result = true, classVarDeclarations = false;
            theWriter.WriteLine(indent(spaces)  + "<class>");
            spaces+=2;
            theWriter.WriteLine(indent(spaces)  + "<keyword> class </keyword>");
            advance();
            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            else
            {
                theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
                advance();
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                    return false;
                else
                {
                    theWriter.WriteLine(indent(spaces)  + "<symbol> { </symbol>");
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
                                case "field":
                                    if (!classVarDeclarations)
                                    {
                                        theWriter.WriteLine(indent(spaces)  + "<classVarDec>");
                                        spaces += 2;
                                        classVarDeclarations = true;
                                    }
                                    if (!CompileClassVarDec())
                                        return false;
                                    break;
                                case "constructor":
                                case "function":
                                case "method":
                                    if (classVarDeclarations)
                                    {
                                        spaces -= 2;
                                        theWriter.WriteLine(indent(spaces)  + "</classVarDec>");
                                        classVarDeclarations = false;
                                    }
                                    if (!CompileSubroutineDec())
                                        return false;
                                    break;
                                default:
                                    return false;
                            }
                        }
                        advance();
                    }
                    theWriter.WriteLine(indent(spaces)  + "<symbol> } </symbol>");
                    spaces-=2;
                    theWriter.WriteLine(indent(spaces)  + "</class>");
                }
            }
            return result;
        }

        private Boolean CompileSubroutineDec()
        {
            Boolean result = true;
            
            theWriter.WriteLine(indent(spaces)  + "<subroutineDec>");
            spaces+=2;
            theWriter.WriteLine(indent(spaces)  + "<keyword> " + currentToken.theToken + " </keyword>");
            advance();
            if ((currentToken.theTokenType == tokenType.KEYWORD) && (currentToken.theToken == "void"))
                theWriter.WriteLine(indent(spaces)  + "<keyword> void </keyword>");
            else
                if (!CompileType())
                    return false;
            advance();
            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
            advance();
            if ((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "("))
                theWriter.WriteLine(indent(spaces)  + "<symbol> ( </symbol>");
            else
                return false;
            theWriter.WriteLine(indent(spaces)  + "<parameterList>");
            advance();
            if (!CompileParameterList())
                return false;
            theWriter.WriteLine(indent(spaces)  + "</parameterList>");
            theWriter.WriteLine(indent(spaces)  + "<symbol> ) </symbol>");
            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<subroutineBody>");
            spaces += 2;
            theWriter.WriteLine(indent(spaces)  + "<symbol> { </symbol>");
            advance();
            if (!CompileVarDecs())
                return false;
            if (!CompileStatements())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> } </symbol>");
            spaces -= 2;
            theWriter.WriteLine(indent(spaces)  + "</subroutineBody>");
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</subroutineDec>");


            return result;
        }

        private Boolean CompileStatements()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<statements>");
            spaces+=2;
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
                        if (!CompileIfStatement())
                            return false;
                        break;
                    case "while":
                        if (!CompileWhileStatement())
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
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</statements>");
            return result;
        }

        private Boolean CompileLetStatement()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<letStatement>");
            spaces+=2;
            theWriter.WriteLine(indent(spaces)  + "<keyword> let </keyword>");
            advance();
            if (!(currentToken.theTokenType == tokenType.IDENTIFIER))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
            advance();
            if (!(currentToken.theTokenType == tokenType.SYMBOL))
                return false;
            if (currentToken.theToken == "[")
            {
                theWriter.WriteLine(indent(spaces)  + "<symbol> [ </symbol>");
                advance();
                if (!CompileExpression())
                    return false;
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "]")))
                    return false;
                theWriter.WriteLine(indent(spaces)  + "<symbol> ] </symbol>");
                advance();
            }
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "=")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> = </symbol>");
            advance();
            if (!CompileExpression())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ";")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> ; </symbol>");
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</letStatement>");
            advance();
            return result;
        }

        private Boolean CompileDoStatement()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<doStatement>");
            spaces+=2;
            theWriter.WriteLine(indent(spaces)  + "<keyword> do </keyword>");
            advance();
            if (!CompileSubroutineCall())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ";")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> ; </symbol>");
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</doStatement>");
            advance();
            return result;
        }

        private Boolean CompileSubroutineCall()
        {
            Boolean result = true;

            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
            advance();
            if (!(currentToken.theTokenType == tokenType.SYMBOL))
                return false;
            switch (currentToken.theToken)
            {
                case "(":
                    theWriter.WriteLine(indent(spaces)  + "<symbol> ( </symbol>");
                    advance();
                    if (!CompileExpressionList())
                        return false;
                    break;
                case ".":
                    theWriter.WriteLine(indent(spaces)  + "<symbol> . </symbol>");
                    advance();
                    if (currentToken.theTokenType != tokenType.IDENTIFIER)
                        return false;
                    theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
                    advance();
                    if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "(")))
                        return false;
                    theWriter.WriteLine(indent(spaces)  + "<symbol> ( </symbol>");
                    advance();
                    if (!CompileExpressionList())
                        return false;
                    break;
                default:
                    return false;
            }
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> ) </symbol>");
            advance();
            return result;
        }

        private Boolean CompileWhileStatement()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<whileStatement>");
            spaces+=2;
            theWriter.WriteLine(indent(spaces)  + "<keyword> while </keyword>");
            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "(")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> ( </symbol>");
            advance();
            if (!CompileExpression())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> ) </symbol>");
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> { </symbol>");
            if (!CompileStatements())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> } </symbol>");
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</whileStatement>");
            advance();
            return result;
        }

        private Boolean CompileExpressionList()
        {
            Boolean result = true, firstExp = true;

            theWriter.WriteLine(indent(spaces)  + "<expressionList>");
            spaces+=2;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))   //empty list
            {
                do
                {
                    if (firstExp)
                        firstExp = false;
                    else
                    {
                        theWriter.WriteLine(indent(spaces)  + "<symbol> , </symbol>");
                        advance();
                    }
                    if (!CompileExpression())
                        return false;
                } while (((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ",")));

            }
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</expressionList>");
            return result;
        }

        private Boolean CompileExpression()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<expression>");
            spaces+=2;
            if (!CompileTerm())
                return false;
            if(currentToken.theTokenType == tokenType.SYMBOL)
                switch (currentToken.theToken)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                    case "&":
                    case "|":
                    case "<":
                    case ">":
                    case "=":
                        theWriter.WriteLine(indent(spaces)  + "<symbol> "+currentToken.theToken + " </symbol>");
                        advance();
                        if (!CompileTerm())
                            return false;
                        break;
                    case ";":
                    case ")":
                    case "]":
                        break;
                    default:
                        return false;
                }
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</expression>");
            return result;
        }

        private Boolean CompileTerm()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<term>");
            spaces+=2;
            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
            advance();
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</term>");
            return result;
        }
        private Boolean CompileIfStatement()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<ifStatement>");
            spaces+=2;
            theWriter.WriteLine(indent(spaces)  + "<keyword> if </keyword>");
            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "(")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> ( </symbol>");
            advance();
            if (!CompileExpression())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> ) </symbol>");
            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> { </symbol>");
            advance();
            if (!CompileStatements())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> } </symbol>");
            advance();
            if (((currentToken.theTokenType == tokenType.KEYWORD) && (currentToken.theToken == "else")))
            {
                theWriter.WriteLine(indent(spaces)  + "<keyword> else </keyword>");
                advance();
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                    return false;
                theWriter.WriteLine(indent(spaces)  + "<symbol> { </symbol>");
                advance();
                if (!CompileStatements())
                    return false;
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                    return false;
                theWriter.WriteLine(indent(spaces)  + "<symbol> } </symbol>");
                advance();
            }
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</ifStatement>");
            return result;
        }
        private Boolean CompileReturnStatement()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<returnStatement>");
            spaces+=2;
            theWriter.WriteLine(indent(spaces)  + "<keyword> return </keyword>");
            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ";")))
            {
                if (!CompileExpression())
                    return false;
            }
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ";")))
                return false;
            theWriter.WriteLine(indent(spaces)  + "<symbol> ; </symbol>");
            advance();
            spaces-=2;
            theWriter.WriteLine(indent(spaces)  + "</returnStatement>");
            return result;
        }

        private Boolean CompileVarDecs()
        {
            Boolean result = true;
            while ((currentToken.theTokenType == tokenType.KEYWORD) && (currentToken.theToken == "var"))
            {
                theWriter.WriteLine(indent(spaces)  + "<varDec>");
                spaces+=2;
                theWriter.WriteLine(indent(spaces)  + "<keyword> var </keyword>");
                advance();
                if (!CompileType())
                    return false;
                while (true)
                {
                    advance();
                    if (currentToken.theTokenType != tokenType.IDENTIFIER)
                        return false;
                    theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
                    advance();
                    if (currentToken.theTokenType != tokenType.SYMBOL)
                        return false;
                    if (currentToken.theToken == ";")
                        break;
                    if (currentToken.theToken != ",")
                        return false;
                    theWriter.WriteLine(indent(spaces)  + "<symbol> , </symbol>");
                }
                theWriter.WriteLine(indent(spaces)  + "<symbol> ; </symbol>");
                spaces-=2;
                theWriter.WriteLine(indent(spaces)  + "</varDec>");
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
                theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
                advance();
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                {
                    if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ",")))
                        return false;
                    theWriter.WriteLine(indent(spaces)  + "<symbol> , </symbol>");
                    advance();
                }
            }
            return result;
        }
        
        private Boolean CompileClassVarDec()
        {
            Boolean result = true;

            theWriter.WriteLine(indent(spaces)  + "<keyword> " + currentToken.theToken + " </keyword>");
            advance();
            if (!CompileType())
                return false;
            while(true)
            {
                advance();
                if (currentToken.theTokenType != tokenType.IDENTIFIER)
                    return false;
                theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
                advance();
                if (currentToken.theTokenType != tokenType.SYMBOL)
                    return false;
                if (currentToken.theToken == ";")
                    break;
                if (currentToken.theToken != ",")
                    return false;
                theWriter.WriteLine(indent(spaces)  + "<symbol> , </symbol>");
            }
            theWriter.WriteLine(indent(spaces)  + "<symbol> ; </symbol>");
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
                    theWriter.WriteLine(indent(spaces)  + "<keyword> " + currentToken.theToken + " </keyword>");
                }
                else
                    theWriter.WriteLine(indent(spaces)  + "<identifier> " + currentToken.theToken + " </identifier>");
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
