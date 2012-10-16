using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class BlockVolume
    {
        public readonly BlockPosition Minimum;
        public readonly BlockPosition Maximum;

        public BlockVolume(BlockPosition minimum, BlockPosition maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public BlockVolume(BlockPosition center, int radius)
        {
            Maximum = new BlockPosition(center.X + radius, center.Y + radius, center.Z + radius);
            Minimum = new BlockPosition(center.X - radius, center.Y - radius, center.Z - radius);
        }
    }
}
