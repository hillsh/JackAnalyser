using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        private struct token
        {
            public String theToken;
            public tokenType theTokenType;
        };
        private token currentToken;

        public JackTokenizer()
        {
        }

        public void ReadTheFile()
        {
            String line;
            Boolean multistringComment = false;

            Program.outFile.WriteLine("<tokens>");

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
                                String[] parts = inStr.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Count() > 0)
                                {
                                }
                            }
                        }

                    }
                }

                //Read the next line
                line = Program.inFile.ReadLine();
            }
            Program.outFile.WriteLine("</tokens>");
        }

        private void tokenize(String inStr)
        {
            inStr = Regex.Replace(inStr, @"\s+", "\u0007"); // Remove all whitespace inside the string and replace with a nonprinting UNICODE char
            String[] parts = inStr.Split(symbols, StringSplitOptions.RemoveEmptyEntries);   //splits on all symbols
            int charCount = 0;
            String makeString = "";
            Boolean stringConstruct = false;
            if (parts.Count() > 0)
            {
                for (int i = 0; i< parts.Count(); i++)
                {
                    if (parts[i].Contains("\"") || stringConstruct)
                    {
                        makeString += parts[i];
                        charCount += parts[i].Length;
                        if(makeString.Substring(2).Contains("\""))
                        {
                            currentToken.theToken = makeString;
                            currentToken.theTokenType = tokenType.STRING_CONST;
 //                           writeTheToken();
                            stringConstruct = false;
                        }
                        else
                        {
                            makeString += " ";
                            charCount++;
                            stringConstruct = true;
                        }
                    }
                    else
                    {
                        currentToken.theToken = parts[i];
                        if (keyWords.Any(parts[i].Contains))
                            currentToken.theTokenType = tokenType.KEYWORD;
                        else
                           if (parts[i].Contains("\""))
                            currentToken.theTokenType = tokenType.STRING_CONST;
                        else
                            if (parts[i].All(Char.IsDigit))
                            currentToken.theTokenType = tokenType.INT_CONST;
                        else
                            currentToken.theTokenType = tokenType.IDENTIFIER;
//                    writeTheToken();
                        charCount += parts[i].Length;
                        if(i < parts.Count()-1)
                        {
                            String sTemp = inStr.Substring(charCount);
                            for(int j=0; j < sTemp.IndexOf(parts[i+1]); j++)
                            {
                                if (inStr[charCount] != '\u0007')
                                {
                                    currentToken.theToken = inStr[charCount].ToString();
                                    currentToken.theTokenType = tokenType.SYMBOL;
//                                    writeTheToken();
                                }
                                charCount++;
                            }
                        }
                    }
                }
                if (charCount < inStr.Length)
                {
                    for(int j = charCount; j<inStr.Length; j++)
                    {
                        if (inStr[j] != '\u0007')
                        {
                            currentToken.theToken = inStr[j].ToString();
                            currentToken.theTokenType = tokenType.SYMBOL;
//                            writeTheToken();
                        }
                    }
                }
            }
        }
        private void writeTheToken()
        {
            switch (currentToken.theTokenType)
            {
                case tokenType.IDENTIFIER:
                    break;
                case tokenType.INT_CONST:
                    break;
                case tokenType.KEYWORD:
                    break;
                case tokenType.STRING_CONST:
                    break;
                case tokenType.SYMBOL:
                    break;
                default:
                    Console.WriteLine("Unidentified token: ");
                    break;
            }
        }

    }
}
