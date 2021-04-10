using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfMath
{
    public class SymbolMappingNotFoundException : Exception
    {
        internal SymbolMappingNotFoundException(string symbolName)
            : base(string.Format("Не удаётся найти символ с данным именем '{0}'.", symbolName))
        {
        }
    }
}
