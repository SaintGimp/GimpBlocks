using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class VoxelBuffer<T>
    {
        // left-right
        protected readonly int _xDimension;
        // up-down
        protected readonly int _yDimension;
        // in-out (positive toward viewer);
        protected readonly int _zDimension;

        protected readonly int _layerSize;
        protected readonly int _bufferSize;

        protected readonly T[] _buffer;

        public VoxelBuffer(int xDimension, int yDimension, int zDimension)
        {
            _xDimension = xDimension;
            _yDimension = yDimension;
            _zDimension = zDimension;

            _layerSize = _xDimension * _yDimension;
            _bufferSize = _xDimension * _yDimension * _zDimension;
            _buffer = new T[_bufferSize];
        }

        public T this[int x, int y, int z]
        {
            get { return _buffer[(x * _layerSize) + (y * _yDimension) + z]; }
            set { _buffer[(x * _layerSize) + (y * _yDimension) + z] = value; }
        }

        public T this[int i]
        {
            get { return _buffer[i]; }
        }

        public void Initialize(Func<int, int, int, T> initializerFunction)
        {
            for (int x = 0; x < _xDimension; x++)
            {
                for (int y = 0; y < _yDimension; y++)
                {
                    for (int z = 0; z < _zDimension; z++)
                    {
                        this[x, y, z] = initializerFunction(x, y, z);
                    }
                }
            }
        }

        public void ForEach(Action<T, int, int, int> action)
        {
            for (int x = 0; x < _xDimension; x++)
            {
                for (int y = 0; y < _yDimension; y++)
                {
                    for (int z = 0; z < _zDimension; z++) 
                    {
                        action(this[x, y, z], x, y, z);
                    }
                }
            }
        }

        public T UpNeighbor(int x, int y, int z)
        {
            return this[x, y + 1, z];
        }

        public T DownNeighbor(int x, int y, int z)
        {
            return this[x, y - 1, z];
        }

        public T LeftNeighbor(int x, int y, int z)
        {
            return this[x - 1, y, z];
        }

        public T RightNeighbor(int x, int y, int z)
        {
            return this[x + 1, y, z];
        }

        public T FrontNeighbor(int x, int y, int z)
        {
            return this[x, y, z + 1];
        }

        public T BackNeighbor(int x, int y, int z)
        {
            return this[x, y, z - 1];
        }

        public T UpLeftNeighbor(int x, int y, int z)
        {
            return this[x - 1, y + 1, z];
        }

        public T UpRightNeighbor(int x, int y, int z)
        {
            return this[x + 1, y + 1, z];
        }

        public T UpBackNeighbor(int x, int y, int z)
        {
            return this[x, y + 1, z - 1];
        }

        public T UpFrontNeighbor(int x, int y, int z)
        {
            return this[x, y + 1, z + 1];
        }

        public T DownLeftNeighbor(int x, int y, int z)
        {
            return this[x - 1, y - 1, z];
        }

        public T DownRightNeighbor(int x, int y, int z)
        {
            return this[x + 1, y - 1, z];
        }

        public T DownBackNeighbor(int x, int y, int z)
        {
            return this[x, y - 1, z - 1];
        }

        public T DownFrontNeighbor(int x, int y, int z)
        {
            return this[x, y - 1, z + 1];
        }

        public T LeftBackNeighbor(int x, int y, int z)
        {
            return this[x - 1, y, z - 1];
        }

        public T LeftFrontNeighbor(int x, int y, int z)
        {
            return this[x - 1, y, z + 1];
        }

        public T RightBackNeighbor(int x, int y, int z)
        {
            return this[x + 1, y, z - 1];
        }

        public T RightFrontNeighbor(int x, int y, int z)
        {
            return this[x + 1, y, z + 1];
        }

        public IEnumerable<T> GetVolume(int x0, int x1, int y0, int y1, int z0, int z1)
        {
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    for (int z = z0; z <= z1; z++)
                    {
                        yield return this[x, y, z];
                    }
                }
            }
        }
    }
}
