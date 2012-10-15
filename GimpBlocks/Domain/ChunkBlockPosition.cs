using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    // TODO: This isn't the most performant way to navigate around a 3D world but it's easy
    // to write and reason about.  The resulting code could be transformed into something
    // better once it's stable.

    // TODO: experiment with MethodImplOptions.AggressiveInlining in CLR 4.5

    // TODO: if the chunk size is a power of two in each dimension, then we can store both
    // the chunk id and the relative block coord in an int by just bit masking/shifting

    public struct ChunkBlockPosition
    {
        public int X;
        public int Y;
        public int Z;

        public ChunkBlockPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public ChunkBlockPosition Left
        {
            get { return new ChunkBlockPosition(X - 1, Y, Z); }
        }

        public ChunkBlockPosition Right
        {
            get { return new ChunkBlockPosition(X + 1, Y, Z); }
        }

        public ChunkBlockPosition Front
        {
            get { return new ChunkBlockPosition(X, Y, Z + 1); }
        }

        public ChunkBlockPosition Back
        {
            get { return new ChunkBlockPosition(X, Y, Z - 1); }
        }

        public ChunkBlockPosition Up
        {
            get { return new ChunkBlockPosition(X, Y + 1, Z); }
        }

        public ChunkBlockPosition Down
        {
            get { return new ChunkBlockPosition(X, Y - 1, Z); }
        }

        public static implicit operator Vector3(ChunkBlockPosition location)
        {
            return new Vector3(location.X, location.Y, location.Z);
        }

        public static implicit operator ChunkBlockPosition(Vector3 location)
        {
            return new ChunkBlockPosition((int)location.X, (int)location.Y, (int)location.Z);
        }
    }
}
