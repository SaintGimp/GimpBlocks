using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class BlockArray
    {
        readonly BlockPrototypeMap _prototypeMap;
        readonly Array3<byte> _blockIndexes;

        public BlockArray(BlockPrototypeMap prototypeMap, int xDimension, int yDimension, int zDimension)
        {
            _prototypeMap = prototypeMap;
            _blockIndexes = new Array3<byte>(xDimension, yDimension, zDimension);
        }

        public int XDimension
        {
            get { return _blockIndexes.XDimension; }
        }

        public int YDimension
        {
            get { return _blockIndexes.YDimension; }
        }

        public int ZDimension
        {
            get { return _blockIndexes.ZDimension; }
        }

        public BlockPrototype this[int x, int y, int z]
        {
            get { return _prototypeMap[_blockIndexes[x, y, z]]; }
            set { _blockIndexes[x, y, z] = _prototypeMap[value]; }
        }

        public BlockPrototype this[BlockPosition location]
        {
            get { return this[location.X, location.Y, location.Z]; }
            set { _blockIndexes[location.X, location.Y, location.Z] = _prototypeMap[value]; }
        }

        public bool IsInBounds(BlockPosition position)
        {
            return _blockIndexes.IsInBounds(position);
        }

        public void Initialize(Func<int, int, int, BlockPrototype> initializerFunction)
        {
            _blockIndexes.Initialize((x, y, z) => _prototypeMap[initializerFunction(x, y, z)]);
        }

        public void ForEach(Action<BlockPrototype, int, int, int> action)
        {
            _blockIndexes.ForEach((index, x, y, z) => action(_prototypeMap[index], x, y, z));
        }
    }
}
