using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class BlockArray
    {
        readonly BlockPrototypeMap prototypeMap;
        readonly Array3<byte> blockIndexes;

        public BlockArray(BlockPrototypeMap prototypeMap, int xDimension, int yDimension, int zDimension)
        {
            this.prototypeMap = prototypeMap;
            blockIndexes = new Array3<byte>(xDimension, yDimension, zDimension);
        }

        public int XDimension
        {
            get { return blockIndexes.XDimension; }
        }

        public int YDimension
        {
            get { return blockIndexes.YDimension; }
        }

        public int ZDimension
        {
            get { return blockIndexes.ZDimension; }
        }

        public BlockPrototype this[int x, int y, int z]
        {
            get { return prototypeMap[blockIndexes[x, y, z]]; }
            set { blockIndexes[x, y, z] = prototypeMap[value]; }
        }

        public BlockPrototype this[BlockPosition location]
        {
            get { return this[location.X, location.Y, location.Z]; }
            set { blockIndexes[location.X, location.Y, location.Z] = prototypeMap[value]; }
        }

        public void Initialize(Func<int, int, int, BlockPrototype> initializerFunction)
        {
            blockIndexes.Initialize((x, y, z) => prototypeMap[initializerFunction(x, y, z)]);
        }

        public void ForEach(Action<BlockPrototype, RelativeBlockPosition> action)
        {
            blockIndexes.ForEach((index, x, y, z) => action(prototypeMap[index], new RelativeBlockPosition(x, y, z)));
        }
    }
}
