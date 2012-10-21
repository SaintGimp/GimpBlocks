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
            CanBeSeen = false;
            // TODO: this should be set to false once we have dynamic chunk loading
            // and physics because the camera should never be out in the void where
            // we'd need faces generated for the outsides of the chunk
            CanBeSeenThrough = true;
            CanBeSelected = false;
        }
    }
}
