using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class AirBlock : BlockPrototype
    {
        public AirBlock()
        {
            CanPropagateLight = true;
            CanBeSeenThrough = true;
            CanBeSelected = false;
        }
    }
}
