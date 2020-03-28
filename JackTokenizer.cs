using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace JackAnalyser
{
    class JackTokenizer
    { 
        private enum tokenType { KEYWORD, SYMBOL, IDENTIFIER, INT_CONST, STRING_CONST };
        private static String[] keyWords = {"class","constructor","function","method","field","static","var",
        "int","char","boolean","void","true","false","null","this","let","do","if","else","while","return"};
        private static String[] symbols = {"{","}","(",")","[","]",".",",",";","+","-","*","/","$","|","<",
        ">","=","~","\u0007"};
        private Boolean hasMoreTokens = false;
        private StreamWriter theWriter;
        private struct token
        {
            public String theToken;
            public tokenType theTokenType;
        };
        private token currentToken;

        public JackTokenizer(StreamWriter sw)
        {
            theWriter = sw;
        }

        public void ReadTheFile()
        {
            String line;
            Boolean multistringComment = false;

            theWriter.WriteLine("<tokens>");

            line = Program.inFile.ReadLine();

            //Continue to read until you reach end of file
            while (line != null)
            {
                //process the line
                line.Trim();
                if (!(line.StartsWith("//") || line.Length == 0)) //Skip empty lines of full line comments
                {
                    // Check for the other type of comment
                    if (multistringComment)
                    {
                        if (line.EndsWith("*/"))
                            multistringComment = false;
                    }
                    else
                    {
                        if (line.StartsWith("/*"))
                        {
                            if (!line.EndsWith("*/"))
                                multistringComment = true;
                        }
                        else
                        {
                            String[] strVals = new string[] { "//" };   // Check for comments at the end of lines
                            String[] strSplit = line.Split(strVals, StringSplitOptions.RemoveEmptyEntries);
                            if (strSplit.Count() > 0)
                            {

                                String inStr = strSplit[0].Trim();
                                tokenize(inStr);
                            }
                        }

                    }
                }
                //Read the next line
                line = Program.inFile.ReadLine();
            }
            theWriter.WriteLine("</tokens>");
        }

        private void tokenize(String inStr)
        {
            if(inStr.Contains("\""))    // The line contains a string, therefore must be processed differently
            {
                String pattern = @"\s";
                Regex rgx = new Regex(pattern);
                Boolean fillingString = false;
                String sTemp = "";
                for(int j=0; j<inStr.Count(); j++)
                {
                    String sc = (inStr[j]).ToString();
                    if (rgx.IsMatch(sc) || symbols.Any(sc.Contains) || sc.Contains("\""))
                    {
                        if(!fillingString)
                        {
                            if(sTemp.Length > 0)
                            {
                                getTokenType(sTemp);
                                writeTheToken();
                            }
                            if (symbols.Any(sc.Contains))
                            {
                                currentToken.theToken = sc;
                                currentToken.theTokenType = tokenType.SYMBOL;
                                writeTheToken();
                            }
                            else
                            {
                                if(sc.Contains("\""))
                                    fillingString = true;
                            }
                            sTemp = "";
                        }
                        else
                        {
                            if (sc.Contains("\""))
                            {
                                currentToken.theToken = sTemp;
                                currentToken.theTokenType = tokenType.STRING_CONST;
                                writeTheToken();
                                sTemp = "";
                                fillingString = false;
                            }
                            else
                                sTemp += sc;
                        }
                    }
                    else
                        sTemp += sc;
                }

            }
            else
            {
                inStr = Regex.Replace(inStr, @"\s+", "\u0007"); // Remove all whitespace inside the string and replace with a nonprinting UNICODE char
                String[] parts = inStr.Split(symbols, StringSplitOptions.RemoveEmptyEntries);   //splits on all symbols
                int charCount = 0;
                for (int i = 0; i < parts.Count(); i++)
                {
                    currentToken.theToken = parts[i];
                    getTokenType(parts[i]);
                    writeTheToken();
                    charCount += parts[i].Length;
                    if (i < parts.Count() - 1)
                    {
                        String sTemp = inStr.Substring(charCount);
                        for (int j = 0; j < sTemp.IndexOf(parts[i + 1]); j++)
                        {
                            if (inStr[charCount] != '\u0007')
                            {
                                currentToken.theToken = inStr[charCount].ToString();
                                currentToken.theTokenType = tokenType.SYMBOL;
                                writeTheToken();
                            }
                            charCount++;
                        }
                    }
                }
                if (charCount < inStr.Length)
                {
                    for (int j = charCount; j < inStr.Length; j++)
                    {
                        if (inStr[j] != '\u0007')
                        {
                            currentToken.theToken = inStr[j].ToString();
                            currentToken.theTokenType = tokenType.SYMBOL;
                            writeTheToken();
                        }
                    }
                }
            }
        }

        private void getTokenType(String sTk)
        {
            currentToken.theToken = sTk;
            if (checkKeywords(sTk))
                currentToken.theTokenType = tokenType.KEYWORD;
            else
                if (sTk.All(Char.IsDigit))
                currentToken.theTokenType = tokenType.INT_CONST;
            else
                currentToken.theTokenType = tokenType.IDENTIFIER;
        }

        private Boolean checkKeywords(String inStr)
        {
            int j;
            for (j = 0; j < keyWords.Count(); j++)
                if (String.Equals(inStr, keyWords[j]))
                    return true;
            return false;
        }
        private void writeTheToken()
        {
            switch (currentToken.theTokenType)
            {
                case tokenType.IDENTIFIER:
                    theWriter.WriteLine("<identifier> " + currentToken.theToken + " </identifier>");
                    break;
                case tokenType.INT_CONST:
                    theWriter.WriteLine("<integerConstant> " + currentToken.theToken + " </integerConstant>");
                    break;
                case tokenType.KEYWORD:
                    theWriter.WriteLine("<keyword> " + currentToken.theToken + " </keyword>");
                    break;
                case tokenType.STRING_CONST:
                    theWriter.WriteLine("<stringConstant> " + currentToken.theToken + " </stringConstant>");
                    break;
                case tokenType.SYMBOL:
                    switch (currentToken.theToken)
                    {
                        case "<":
                            theWriter.WriteLine("<symbol> &lt; </symbol>");
                            break;
                        case ">":
                            theWriter.WriteLine("<symbol> &gt; </symbol>");
                            break;
                        case "\"":
                            theWriter.WriteLine("<symbol> &quot; </symbol>");
                            break;
                        case "&":
                            theWriter.WriteLine("<symbol> &amp; </symbol>");
                            break;
                        default:
                            theWriter.WriteLine("<symbol> " + currentToken.theToken + " </symbol>");
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("Unidentified token: ");
                    break;
            }
        }

    }
}
