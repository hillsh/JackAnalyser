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
        public static StreamWriter outFile;
        public static int labelPtr = 0;
        public static int retPtr = 0;
       static void Main(string[] args)
        {
            JackTokenizer theTokenizer;
            String sBase, line;

            sBase = "E:\\Learning\\Coursera\\nand2tetris\\projects\\10\\";

            Console.WriteLine("Enter the name of the file with Jack code, or a directory name containing Jack files ");
            Console.WriteLine("The file name should be the complete path including the file type field (.jack} if it is a Jack file.");
            Console.WriteLine("Enter the name->");
            line = Console.ReadLine();

            if (line.Contains(".jack"))
            {
                // Get the filename without extension from the entry
                String[] strVals = new string[] { "\\" };
                String[] strSplit = line.Split(strVals, StringSplitOptions.RemoveEmptyEntries);
                theTokenizer = new JackTokenizer();
                line = sBase + line;
                inFile = new StreamReader(line);
                int indx = line.LastIndexOf(".");
                line = line.Substring(0, indx + 1) + "xml";
                outFile = new StreamWriter(line);
                theTokenizer.ReadTheFile();
                //sr.DiscardBufferedData();
                //sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                //theProcessor.FinalPass(sr, theTable, sw);
                //close the Streamreader and Streamwriter
                inFile.Close();
                outFile.Close();
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
                Boolean outFileDone = false, bootstrapDone = false;
                if (fI.Length > 0)
                {
                    if (fI.Length > 1)
                    {
                        outFile = new StreamWriter(line + ".xml");
                        outFileDone = true;
                    }
                    foreach (var file in fI)
                    {
                        Console.WriteLine("processing file " + file.Name);
                        fname = file.Name.Trim();
                        fname = fname.Substring(0, fname.IndexOf("."));
                        if (!outFileDone)
                        {
                            String tmp = file.FullName.Substring(0, file.FullName.LastIndexOf(".") + 1);
                            outFile = new StreamWriter(tmp + "xml");
                            outFileDone = true;
                        }
                        theTokenizer = new JackTokenizer();
                        inFile = new StreamReader(file.FullName);
                        theTokenizer.ReadTheFile();
                        inFile.Close();
                    }
                }
                else
                    Console.WriteLine("There are no VM files in the directory!");
            }
            if (outFile.BaseStream != null)
                outFile.Close();
            Console.WriteLine("Press any key to close this console.");
            Console.ReadKey();
        }
    }
}
