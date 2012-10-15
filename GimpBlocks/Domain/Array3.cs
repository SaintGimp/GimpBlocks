using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class Array3<T>
    {
        // left-right
        public int XDimension { get; protected set; }
        // up-down
        public int YDimension { get; protected set; }
        // in-out (positive toward viewer);
        public int ZDimension { get; protected set; }

        protected readonly int LayerSize;
        protected readonly int BufferSize;

        protected readonly T[] Buffer;

        public Array3(int xDimension, int yDimension, int zDimension)
        {
            XDimension = xDimension;
            YDimension = yDimension;
            ZDimension = zDimension;

            // TODO: should figure out the most efficient layout for the array so that typical access is sequential for cache coherency

            LayerSize = ZDimension * YDimension;
            BufferSize = XDimension * YDimension * ZDimension;
            Buffer = new T[BufferSize];
        }

        public T this[int x, int y, int z]
        {
            get { return Buffer[LinearIndex(x, y, z)]; }
            set { Buffer[LinearIndex(x, y, z)] = value; }
        }

        protected int LinearIndex(int x, int y, int z)
        {
            return (x * LayerSize) + (y * ZDimension) + z;
        }

        public T this[int i]
        {
            get { return Buffer[i]; }
            set { Buffer[i] = value; }
        }

        public T this[BlockPosition position]
        {
            get { return this[position.X, position.Y, position.Z]; }
            set { this[position.X, position.Y, position.Z] = value; }
        }

        public bool IsInBounds(BlockPosition position)
        {
            return IsInBounds(position.X, position.Y, position.Z);
        }

        public bool IsInBounds(int x, int y, int z)
        {
            return x >= 0 && x < XDimension && y >= 0 && y < YDimension && z >= 0 && z < ZDimension;
        }

        public void Initialize(Func<int, int, int, T> initializerFunction)
        {
            for (int x = 0; x < XDimension; x++)
            {
                for (int y = 0; y < YDimension; y++)
                {
                    for (int z = 0; z < ZDimension; z++)
                    {
                        this[x, y, z] = initializerFunction(x, y, z);
                    }
                }
            }
        }

        public void ForEachInVolume(int x0, int y0, int z0, int x1, int y1, int z1, Action<T, int, int, int> action)
        {
            x0 = Math.Max(x0, 0);
            y0 = Math.Max(y0, 0);
            z0 = Math.Max(z0, 0);
            x1 = Math.Min(x1, XDimension - 1);
            y1 = Math.Min(y1, YDimension - 1);
            z1 = Math.Min(z1, ZDimension - 1);

            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    for (int z = z0; z <= z1; z++)
                    {
                        action(this[x, y, z], x, y, z);
                    }
                }
            }
        }

        public void ForEach(Action<T, int, int, int> action)
        {
            ForEachInVolume(0, 0, 0, XDimension - 1, YDimension - 1, ZDimension - 1, action);
        }
    }
}
