using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JackAnalyser
{
    class VMWriter
    {
        private StreamWriter theWriter;

        public VMWriter(StreamWriter sw)
        {
            theWriter = sw;
        }

        public enum segType { CONST, ARG, LOCAL, STATIC, THIS, THAT, POINTER, TEMP };
        public enum cmdType { ADD, SUB, NEG, EQ, GT, LT, AND, OR, NOT };

        public void writePush(segType theSeg, int indx)
        {
            switch (theSeg)
            {
                case segType.ARG:
                    theWriter.WriteLine("push arg " + indx.ToString());
                    break;
                case segType.CONST: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("push constant " + indx.ToString());
                    break;
                case segType.LOCAL:
                    theWriter.WriteLine("push local " + indx.ToString());
                    break;
                case segType.POINTER: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("push pointer " + indx.ToString());
                    break;
                case segType.STATIC:
                    theWriter.WriteLine("push static " + indx.ToString());
                    break;
                case segType.TEMP: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("push temp " + indx.ToString());
                    break;
                case segType.THAT:
                    theWriter.WriteLine("push that " + indx.ToString());
                    break;
                case segType.THIS: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("push this " + indx.ToString());
                    break;
            }
        }
        public void writePop(segType theSeg, int indx)
        {
            switch (theSeg)
            {
                case segType.ARG:
                    theWriter.WriteLine("pop arg " + indx.ToString());
                    break;
                case segType.CONST: // does this ever happen?
                    theWriter.WriteLine("pop constant " + indx.ToString());
                    break;
                case segType.LOCAL:
                    theWriter.WriteLine("pop local " + indx.ToString());
                    break;
                case segType.POINTER: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("pop pointer " + indx.ToString());
                    break;
                case segType.STATIC:
                    theWriter.WriteLine("pop static " + indx.ToString());
                    break;
                case segType.TEMP: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("pop temp " + indx.ToString());
                    break;
                case segType.THAT:
                    theWriter.WriteLine("pop that " + indx.ToString());
                    break;
                case segType.THIS: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("pop this " + indx.ToString());
                    break;
            }
        }
        public void writeArithmetic(cmdType theCmd)
        {
            switch (theCmd)
            {
                case cmdType.ADD:
                    theWriter.WriteLine("add");
                    break;
                case cmdType.AND: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("and");
                    break;
                case cmdType.EQ:
                    theWriter.WriteLine("eq ");
                    break;
                case cmdType.GT:
                    theWriter.WriteLine("gt");
                    break;
                case cmdType.LT:
                    theWriter.WriteLine("lt ");
                    break;
                case cmdType.NEG: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("neg");
                    break;
                case cmdType.NOT:
                    theWriter.WriteLine("not");
                    break;
                case cmdType.OR: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("or");
                    break;
                case cmdType.SUB: // in this case the index is actually the value of the constant?
                    theWriter.WriteLine("sub");
                    break;
            }
        }

        public void writeLabel(String theLabel)
        {
            theWriter.WriteLine("label " + theLabel);
        }
        public void writeGoto(String theLabel)
        {
            theWriter.WriteLine("goto " + theLabel);
        }
        public void writeIf(String theLabel)
        {
            theWriter.WriteLine("if-goto " + theLabel);
        }
        public void writeCall(String theName, int nArgs)
        {
            theWriter.WriteLine("call " + theName + nArgs.ToString());
        }
        public void writeFunction(String theName, int nLocals)
        {
            theWriter.WriteLine("function " + theName + nLocals.ToString());
        }

        public void writeReturn()
        {
           theWriter.WriteLine("return");
        }
    }
}
