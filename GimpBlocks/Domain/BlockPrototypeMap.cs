using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class BlockPrototypeMap
    {
        readonly BlockPrototype[] blockPrototypes;
        readonly Dictionary<BlockPrototype, byte> blockIndexes;

        public BlockPrototypeMap()
        {
            blockPrototypes = new BlockPrototype[256];
            blockPrototypes[0] = BlockPrototype.AirBlock;
            blockPrototypes[1] = BlockPrototype.StoneBlock;
            blockPrototypes[2] = BlockPrototype.VoidBlock;

            byte index = 0;
            blockIndexes = blockPrototypes.Where(block => block != null).ToDictionary(block => block, block => index++);
        }

        public BlockPrototype this[byte blockIndex]
        {
            get { return blockPrototypes[blockIndex]; }
        }

        public byte this[BlockPrototype blockPrototype]
        {
            get { return blockIndexes[blockPrototype]; }
        }
    }
}
