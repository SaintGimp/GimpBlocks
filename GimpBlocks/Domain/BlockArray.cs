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

        public BlockPrototype this[int i]
        {
            get { return _prototypeMap[_blockIndexes[i]]; }
            set { _blockIndexes[i] = _prototypeMap[value]; }
        }

        public BlockPrototype this[ChunkBlockPosition location]
        {
            get { return this[location.X, location.Y, location.Z]; }
            set { _blockIndexes[location.X, location.Y, location.Z] = _prototypeMap[value]; }
        }

        public bool IsInBounds(ChunkBlockPosition position)
        {
            return _blockIndexes.IsInBounds(position);
        }

        public void Initialize(Func<int, int, int, BlockPrototype> initializerFunction)
        {
            _blockIndexes.Initialize((x, y, z) => _prototypeMap[initializerFunction(x, y, z)]);
        }

        public void ForEach(Action<Block> action)
        {
            _blockIndexes.ForEach((index, x, y, z) => action(new Block(_prototypeMap[index], new ChunkBlockPosition(x, y, z))));
        }

        public void ForEachInVolume(ChunkBlockPosition lowerBound, ChunkBlockPosition upperBound, Action<Block> action)
        {
            _blockIndexes.ForEachInVolume(lowerBound.X, lowerBound.Y, lowerBound.Z, upperBound.X, upperBound.Y, upperBound.Z,
                (index, x, y, z) => action(new Block(_prototypeMap[index], new ChunkBlockPosition(x, y, z))));
        }

        public Block GetAt(ChunkBlockPosition blockPosition)
        {
            return new Block(this[blockPosition], blockPosition);
        }
    }
}
