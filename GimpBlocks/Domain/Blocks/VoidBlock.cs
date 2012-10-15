    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class VoidBlock : BlockPrototype
    {
        public VoidBlock()
        {
            // This is the block type for areas outside of the loaded world.
            // It cannot transmit light so that light propogation stops, but
            // it can be seen through so we generate faces for real blocks
            // ajacent to it.
            CanPropagateLight = false;
            CanBeSeenThrough = true;
            CanBeSelected = false;
        }
    }
}
