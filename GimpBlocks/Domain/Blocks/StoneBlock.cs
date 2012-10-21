using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class StoneBlock : BlockPrototype
    {
        public StoneBlock()
        {
            CanPropagateLight = false;
            CanBeSeen = true;
            CanBeSeenThrough = false;
            CanBeSelected = true;
        }
    }
}
