using System;
using System.Collections.Generic;
using System.Text;

namespace Claunia.PropertyList.Origin
{
    class BinaryOrigin : INsOrigin
    {
        public BinaryOrigin(int location, int length)
        {
            Location = location;
            Length = length;
        }

        public static BinaryOrigin FromRange(int startPosition, int endPosition)
        {
            return new BinaryOrigin(startPosition, endPosition - startPosition);
        }

        public OriginType OriginType => OriginType.Binary;

        public int Location { get; }

        public int Length { get; private set; }

        public void SetEndPosition(int endPosition)
        {
            this.Length = endPosition - this.Location;
        }
    }
}
