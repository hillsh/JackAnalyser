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

        private Boolean CompileClass() // arriving here means that the keyword 'class' has been read
        {
            Boolean result = true, classVarDeclarations = false;
            theWriter.WriteLine("<class>");
            theWriter.WriteLine("<keyword> class </keyword>");
            advance();
            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            else
            {
                theWriter.WriteLine("<identifier> " + currentToken.theToken + "</identifier>");
                advance();
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                    return false;
                else
                {
                    theWriter.WriteLine("<symbol> { </symbol>");
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
                                        theWriter.WriteLine("<classVarDec>");
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
                                        theWriter.WriteLine("</classVarDec>");
                                        classVarDeclarations = false;
                                    }
                                    if (!CompileSubroutineDec())
                                        return false;
                                    break;
                                default:
                                    return false;
                                    break;
                            }
                        }
                        advance();
                    }
                    theWriter.WriteLine("<symbol> } </symbol>");
                    theWriter.WriteLine("</class>");
                }
            }
            return result;
        }

        private Boolean CompileSubroutineDec()
        {
            Boolean result = true;
            
            theWriter.WriteLine("<subroutineDec>");
            theWriter.WriteLine("<keyword> " + currentToken.theToken + " </keyword>");
            advance();
            if ((currentToken.theTokenType == tokenType.KEYWORD) && (currentToken.theToken == "void"))
                theWriter.WriteLine("<keyword> void </keyword>");
            else
                if (!CompileType())
                    return false;
            advance();
            if (currentToken.theTokenType != tokenType.IDENTIFIER)
                return false;
            theWriter.WriteLine("<identifier> " + currentToken.theToken + " </identifier>");
            advance();
            if ((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "("))
                theWriter.WriteLine("<symbol> ( </symbol>");
            else
                return false;
            theWriter.WriteLine("<parameterList>");
            advance();
            if (!CompileParameterList())
                return false;
            theWriter.WriteLine("</parameterList>");
            theWriter.WriteLine("<symbol> ) </symbol>");
            advance();
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "{")))
                return false;
            theWriter.WriteLine("<subroutineBody>");
            theWriter.WriteLine("<symbol> { </symbol>");
            advance();
            if (!CompileVarDecs())
                return false;
            if (!CompileStatements())
                return false;
            if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == "}")))
                return false;
            theWriter.WriteLine("<symbol> } </symbol>");
            theWriter.WriteLine("</subroutineBody>");
            theWriter.WriteLine("</subroutineDec>");


            return result;
        }

        private Boolean CompileStatements()
        {
            Boolean result = true;
            return result;
        }

        private Boolean CompileVarDecs()
        {
            Boolean result = true;
            while ((currentToken.theTokenType == tokenType.KEYWORD) && (currentToken.theToken == "var"))
            {
                theWriter.WriteLine("<varDec>");
                theWriter.WriteLine("<keyword> var </keyword>");
                advance();
                if (!CompileType())
                    return false;
                while (true)
                {
                    advance();
                    if (currentToken.theTokenType != tokenType.IDENTIFIER)
                        return false;
                    theWriter.WriteLine("<identifier> " + currentToken.theToken + " </identifier>");
                    advance();
                    if (currentToken.theTokenType != tokenType.SYMBOL)
                        return false;
                    if (currentToken.theToken == ";")
                        break;
                    if (currentToken.theToken != ",")
                        return false;
                    theWriter.WriteLine("<symbol> , </symbol>");
                }
                theWriter.WriteLine("<symbol> ; </symbol>");
                theWriter.WriteLine("</varDec>");
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
                theWriter.WriteLine("<identifier> " + currentToken.theToken + " </identifier>");
                advance();
                if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ")")))
                {
                    if (!((currentToken.theTokenType == tokenType.SYMBOL) && (currentToken.theToken == ",")))
                        return false;
                    theWriter.WriteLine("<symbol> , </symbol>");
                    advance();
                }
            }
            return result;
        }
        
        private Boolean CompileClassVarDec()
        {
            Boolean result = true;

            theWriter.WriteLine("<keyword> " + currentToken.theToken + " </keyword>");
            advance();
            if (!CompileType())
                return false;
            while(true)
            {
                advance();
                if (currentToken.theTokenType != tokenType.IDENTIFIER)
                    return false;
                theWriter.WriteLine("<identifier> " + currentToken.theToken + " </identifier>");
                advance();
                if (currentToken.theTokenType != tokenType.SYMBOL)
                    return false;
                if (currentToken.theToken == ";")
                    break;
                if (currentToken.theToken != ",")
                    return false;
                theWriter.WriteLine("<symbol> , </symbol>");
            }
            theWriter.WriteLine("<symbol> ; </symbol>");
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
                    theWriter.WriteLine("<keyword> " + currentToken.theToken + " </keyword>");
                }
                else
                    theWriter.WriteLine("<identifier> " + currentToken.theToken + " </identifier>");
            }
            else
                return false;
            return result;
        }

        private void advance()
        {
            currentToken = nextToken;
            String line = theReader.ReadLine().Trim();
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
