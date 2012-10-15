using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class BlockPrototypeMap
    {
        readonly BlockPrototype[] _blockPrototypes;
        readonly Dictionary<BlockPrototype, byte> _blockIndexes;

        public BlockPrototypeMap()
        {
            _blockPrototypes = new BlockPrototype[256];
            _blockPrototypes[0] = BlockPrototype.AirBlock;
            _blockPrototypes[1] = BlockPrototype.StoneBlock;
            _blockPrototypes[2] = BlockPrototype.VoidBlock;

            byte index = 0;
            _blockIndexes = _blockPrototypes.Where(block => block != null).ToDictionary(block => block, block => index++);
        }

        public BlockPrototype this[byte blockIndex]
        {
            get { return _blockPrototypes[blockIndex]; }
        }

        public byte this[BlockPrototype blockPrototype]
        {
            get { return _blockIndexes[blockPrototype]; }
        }
    }
}
