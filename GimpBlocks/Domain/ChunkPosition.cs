using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public struct ChunkPosition
    {
        public int X;
        public int Z;

        // TODO: right now we don't support multiple chunks in the vertical axis (it wouldn't be hard
        // except that there are some tricky issues around sunlight and updating everything that could
        // be affected on a block place/destroy).  For performance we don't do anything with the Y axis
        // since we don't need it.

        public ChunkPosition(int x, int z)
        {
            X = x;
            Z = z;
        }

        public ChunkPosition(Vector3 vector)
        {
            X = (int)vector.X;
            Z = (int)vector.Z;
        }

        public ChunkPosition Left
        {
            get { return new ChunkPosition(X - 1, Z); }
        }

        public ChunkPosition Right
        {
            get { return new ChunkPosition(X + 1, Z); }
        }

        public ChunkPosition Front
        {
            get { return new ChunkPosition(X, Z + 1); }
        }

        public ChunkPosition Back
        {
            get { return new ChunkPosition(X, Z - 1); }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}", X, Z);
        }

        public bool Between(ChunkPosition minimum, ChunkPosition maximum)
        {
            return X >= minimum.X && X <= maximum.X &&
                Z >= minimum.Z && Z <= maximum.Z;
        }
    }
}
