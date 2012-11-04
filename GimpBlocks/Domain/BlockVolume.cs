using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class BlockVolume
    {
        readonly World world;
        public readonly BlockPosition Minimum;
        public readonly BlockPosition Maximum;

        public BlockVolume(World world, BlockPosition minimum, BlockPosition maximum)
        {
            this.world = world;
            Minimum = minimum;
            Maximum = maximum;
        }

        public BlockVolume(World world, BlockPosition center, int radius)
        {
            this.world = world;
            Maximum = new BlockPosition(center.X + radius, center.Y + radius, center.Z + radius);
            Minimum = new BlockPosition(center.X - radius, center.Y - radius, center.Z - radius);
        }

        public BlockVolume(World world, BlockPosition anchor, int xExtent, int yExtent, int zExtent)
        {
            this.world = world;
            int minimumX = Math.Min(anchor.X, anchor.X + xExtent);
            int minimumY = Math.Min(anchor.Y, anchor.Y + yExtent);
            int minimumZ = Math.Min(anchor.Z, anchor.Z + zExtent);
            int maximumX = Math.Max(anchor.X, anchor.X + xExtent);
            int maximumY = Math.Max(anchor.Y, anchor.Y + yExtent);
            int maximumZ = Math.Max(anchor.Z, anchor.Z + zExtent);
            Maximum = new BlockPosition(maximumX, maximumY, maximumZ);
            Minimum = new BlockPosition(minimumX, minimumY, minimumZ); 
        }

        public void SetAllTo(BlockPrototype prototype)
        {
            foreach (var position in AllPositions())
            {
                world.SetBlockPrototype(position, prototype);
            }
        }

        public IEnumerable<Block> ContainedBlocks()
        {
            return AllPositions().Select(position => world.GetBlockAt(position));
        }
        
        IEnumerable<BlockPosition> AllPositions()
        {
            for (int x = Minimum.X; x <= Maximum.X; x++)
            {
                for (int y = Minimum.Y; y <= Maximum.Y; y++)
                {
                    for (int z = Minimum.Z; z <= Maximum.Z; z++)
                    {
                        yield return new BlockPosition(x, y, z);
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Format("(min: {0}, max: {1}", Minimum, Maximum);
        }
    }
}
