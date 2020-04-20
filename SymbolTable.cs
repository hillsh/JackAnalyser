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
        public enum varTypes { STATIC, FIELD, ARG, VAR, CLASS, SUBROUTINE, NULL };
        public int staticCount, fieldCount, argCount, varCount;
        public struct identifier
        {
            public String iName;
            public varTypes iKind;
            public String iType;
            public int iIndex;
        }
        public identifier nullID;
        public Dictionary<String, identifier> theSymbolTable;

        public SymbolTable()
        {
            theSymbolTable = new Dictionary<string, identifier>();
            staticCount = fieldCount = argCount = varCount = 0;
            nullID.iName = "null";
            nullID.iKind = varTypes.NULL;
            nullID.iType = "";
            nullID.iIndex = 0;

        }

        public void resetIndices()
        {
            argCount = varCount = 0;
        }

        public identifier FindVar(String n)
        {
            identifier id;

            id = new identifier();
            if(theSymbolTable.TryGetValue(n, out id))
            {
                return id;
            }
            else
            {
                return nullID;
            }
        }

        public void newVar(String tname, String ttype, varTypes tKind, Boolean beingUsed)
        {
            identifier id;

            if (theSymbolTable.ContainsKey(tname))
                Console.WriteLine("The dictionary already contains the key " + tname);

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
                default:
                    id.iIndex = 0;
                    break;
            }
            theSymbolTable.Add(tname, id);
        }
        public void newVar(String tname, identifier id)
        {
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
