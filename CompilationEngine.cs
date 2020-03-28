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
        private StreamReader theReader;
        private StreamWriter theWriter;

        public CompilationEngine(StreamReader sr, StreamWriter sw)
        {
            theReader = sr;
            theWriter = sw;
        }
    }
}
