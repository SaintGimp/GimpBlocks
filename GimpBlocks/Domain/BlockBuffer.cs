using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class BlockBuffer : VoxelBuffer<Block>
    {
        public BlockBuffer(int xDimension, int yDimension, int zDimension)
            : base(xDimension, yDimension, zDimension)
        {
        }
    }
}
