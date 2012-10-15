using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public abstract class BlockPrototype
    {
        // Can light propogate through this block to other blocks?
        public bool CanPropagateLight { get; protected set; }

        // Will this block expose faces of adjacent blocks?
        public bool CanBeSeenThrough { get; protected set; }

        // Can the user select and manipulate this block?
        public bool CanBeSelected { get; protected set; }
    }
}
