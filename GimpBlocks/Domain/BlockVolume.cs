﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class BlockVolume
    {
        readonly World _world;
        public readonly BlockPosition Minimum;
        public readonly BlockPosition Maximum;

        public BlockVolume(World world, BlockPosition minimum, BlockPosition maximum)
        {
            _world = world;
            Minimum = minimum;
            Maximum = maximum;
        }

        public BlockVolume(World world, BlockPosition center, int radius)
        {
            _world = world;
            Maximum = new BlockPosition(center.X + radius, center.Y + radius, center.Z + radius);
            Minimum = new BlockPosition(center.X - radius, center.Y - radius, center.Z - radius);
        }

        public void SetAllTo(BlockPrototype prototype)
        {
            foreach (var position in AllPositions())
            {
                _world.SetBlockPrototype(position, prototype);
            }
        }

        public IEnumerable<Block> ContainedBlocks()
        {
            return AllPositions().Select(position => _world.GetBlockAt(position));
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
    }
}
