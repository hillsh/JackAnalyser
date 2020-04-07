using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalyser
{
    class SymbolTable
    {
        public enum varTypes { STATIC, FIELD, ARG, VAR, NULL};
        private int staticCount, fieldCount, argCount, varCount;
        private struct identifier
        {
            public String iName;
            public varTypes iKind;
            public String iType;
            public int iIndex;
        }
        private Dictionary<String, identifier>  theSymbolTable;

        public SymbolTable()
        {
            theSymbolTable = new Dictionary<string, identifier>();
            staticCount = fieldCount = argCount = varCount = 0;
        }

        public void startSubroutine()
        {
            foreach (KeyValuePair<String, identifier> kvp in theSymbolTable)
            {
                if ((kvp.Value.iKind == varTypes.ARG) || (kvp.Value.iKind == varTypes.VAR))
                {
                    theSymbolTable.Remove(kvp.Key);
                }
            }
            argCount = 0;
            varCount = 0;
        }

        public void newVar(String tname, String ttype, varTypes tKind)
        {
            identifier id;

            id = new identifier();
            id.iName = tname;
            id.iKind = tKind;
            id.iType = ttype;
            switch (tKind)
            {
                case varTypes.STATIC:
                    id.iIndex = staticCount;
                    staticCount++;
                    break;
                case varTypes.FIELD:
                    id.iIndex = fieldCount;
                    fieldCount++;
                    break;
                case varTypes.ARG:
                    id.iIndex = argCount;
                    argCount++;
                    break;
                case varTypes.VAR:
                    id.iIndex = varCount;
                    varCount++;
                    break;
            }
            theSymbolTable.Add(tname, id);
        }
        public int idCount(varTypes tKind)
        {
            int tCount = 0;

            switch (tKind)
            {
                case varTypes.STATIC:
                    tCount = staticCount;
                    break;
                case varTypes.FIELD:
                    tCount = fieldCount;
                    break;
               case varTypes.ARG:
                    tCount = argCount;
                    break;
                case varTypes.VAR:
                    tCount = varCount;
                    break;
            }
            return tCount;
        }

        public varTypes kindOf(String name)
        {
            identifier value;

            if (theSymbolTable.TryGetValue(name, out value))
            {
                return value.iKind;
            }
            else
            {
                Console.WriteLine("Key = \"" + name + "\" is not found.");
                return varTypes.NULL;
            }
        }

        public String typeOf(String name)
        {
            identifier value;

            if (theSymbolTable.TryGetValue(name, out value))
            {
                return value.iType;
            }
            else
            {
                Console.WriteLine("Key = \"" + name + "\" is not found.");
                return "none";
            }
        }
        public int indexOf(String name)
        {
            identifier value;

            if (theSymbolTable.TryGetValue(name, out value))
            {
                return value.iIndex;
            }
            else
            {
                Console.WriteLine("Key = \"" + name + "\" is not found.");
                return -1;
            }
        }
    }

}
