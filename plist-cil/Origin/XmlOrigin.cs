using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Claunia.PropertyList.Origin
{
    class XmlOrigin : INsOrigin
    {
        public XmlOrigin(int location, int length, int linePosition, int lineNumber)
        {
            Location = location;
            Length = length;
            LinePosition = linePosition;
            LineNumber = lineNumber;
        }

        public OriginType OriginType => OriginType.XmlText;

        public int Location { get; }

        public int Length { get; }

        public int LineNumber { get; }

        public int LinePosition { get; }
    }
}
