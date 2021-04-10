using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfMath
{
    public class DelimiterMappingNotFoundException : Exception
    {
        internal DelimiterMappingNotFoundException(char delimiter)
            : base(string.Format("Не удалось сопоставить разделители для символа '{0}'.", delimiter))
        {
        }
    }
}
