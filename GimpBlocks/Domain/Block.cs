using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public struct Block
    {
        public BlockType Type;
        
        public bool IsSolid
        {
            get { return Type != BlockType.Air; }
        }
    }

    public enum BlockType
    {
        Air,
        Stone
    }
}
