using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalyser
{
    class Program
    {
        public static StreamReader inFile;
//        public static StreamWriter outFile, xmlFile;
        public static StreamWriter outFile, vmFile;
        public static MemoryStream memStr;
        public static int labelPtr = 0;
        public static int retPtr = 0;
       static void Main(string[] args)
        {
            JackTokenizer theTokenizer;
//            XMLEngine theCompiler;
            CompilationEngine theCompiler;
            String sBase, line;

            sBase = "E:\\Learning\\Coursera\\nand2tetris\\projects\\11\\";

            Console.WriteLine("Enter the name of the file with Jack code, or a directory name containing Jack files ");
            Console.WriteLine("The file name should be the complete path including the file type field (.jack} if it is a Jack file.");
            Console.WriteLine("Enter the name->");
            line = Console.ReadLine();
            memStr = new MemoryStream();

            if (line.Contains(".jack"))
            {
                // Get the filename without extension from the entry
                String[] strVals = new string[] { "\\" };
                String[] strSplit = line.Split(strVals, StringSplitOptions.RemoveEmptyEntries);
                line = sBase + line;
                inFile = new StreamReader(line);
                int indx = line.LastIndexOf(".");
                line = line.Substring(0, indx) + ".vm";
                memStr = new MemoryStream();
                outFile = new StreamWriter(memStr);
                theTokenizer = new JackTokenizer(outFile);
                theTokenizer.ReadTheFile();
                outFile.Flush();
                memStr.Position = 0;
                inFile.Close();
                inFile = new StreamReader(memStr);
//                xmlFile = new StreamWriter(line);
//                theCompiler = new XMLEngine(inFile, xmlFile);
//                theCompiler.CompiletheTokens();
                vmFile = new StreamWriter(line);
                theCompiler = new CompilationEngine(inFile, vmFile);
                theCompiler.CompiletheTokens(); // The first pass fills the Symbol Table
                //close the Streamreader and Streamwriter
 //               memStr.Position = 0;
 //               inFile.Close();
 //               inFile = new StreamReader(memStr);
//                theCompiler.CompiletheTokens(); // The second pass writes the VM file
                //close the Streamreader and Streamwriter
                inFile.Close();
                outFile.Close();    // this closes the underlying memory stream
//                xmlFile.Close();
                vmFile.Close();
            }
            else
            {
                Console.WriteLine("You have entered a directory name");
                line = sBase + line;
                String[] strVals = new string[] { "\\" };
                String[] strSplit = line.Split(strVals, StringSplitOptions.RemoveEmptyEntries);
                String fname = (strSplit[strSplit.Count() - 1]).Trim();
                DirectoryInfo d = new DirectoryInfo(line);
                FileInfo[] fI = d.GetFiles("*.jack");
                if (fI.Length > 0)
                {
                    foreach (var file in fI)
                    {
                        Console.WriteLine("processing file " + file.Name);
                        fname = file.Name.Trim();
                        fname = fname.Substring(0, fname.IndexOf("."));
                        String tmp = file.FullName.Substring(0, file.FullName.LastIndexOf("."));
                        memStr = new MemoryStream();
                        outFile = new StreamWriter(memStr);
                        theTokenizer = new JackTokenizer(outFile);
                        inFile = new StreamReader(file.FullName);
                        theTokenizer.ReadTheFile();
                        Console.WriteLine("Tokenizing finished.");
                        outFile.Flush();
                        memStr.Position = 0;
                        inFile.Close();
                        inFile = new StreamReader(memStr);
//                        xmlFile = new StreamWriter(tmp + "SH.xml");
//                        theCompiler = new XMLEngine(inFile, xmlFile);
//                        theCompiler.CompiletheTokens();
                        vmFile = new StreamWriter(tmp + ".vm");
                        theCompiler = new CompilationEngine(inFile, vmFile);
                        theCompiler.CompiletheTokens();
//                        memStr.Position = 0;
//                        inFile.Close();
//                        inFile = new StreamReader(memStr);
//                        theCompiler.CompiletheTokens();
                       //close the Streamreader and Streamwriter
                        inFile.Close();
                        outFile.Close();
//                        xmlFile.Close();
                        vmFile.Close();
                    }
                }
                else
                    Console.WriteLine("There are no JACK files in the directory!");
            }
            Console.WriteLine("Press any key to close this console.");
            Console.ReadKey();
        }
    }
}
