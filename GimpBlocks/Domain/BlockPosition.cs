using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    // TODO: This isn't the most performant way to navigate around a 3D array but it's easy
    // to write and reason about.  The resulting code could be transformed into something
    // better once it's stable.
    // TODO: would it be better to cache the neighboring locations once we create them?
    public struct BlockPosition
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        public BlockPosition(int x, int y, int z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BlockPosition Left
        {
            get { return new BlockPosition(X - 1, Y, Z); }
        }

        public BlockPosition Right
        {
            get { return new BlockPosition(X + 1, Y, Z); }
        }

        public BlockPosition Front
        {
            get { return new BlockPosition(X, Y, Z + 1); }
        }

        public BlockPosition Back
        {
            get { return new BlockPosition(X, Y, Z - 1); }
        }

        public BlockPosition Up
        {
            get { return new BlockPosition(X, Y + 1, Z); }
        }

        public BlockPosition Down
        {
            get { return new BlockPosition(X, Y - 1, Z); }
        }

        public static implicit operator Vector3(BlockPosition location)
        {
            return new Vector3(location.X, location.Y, location.Z);
        }

        public static BlockPosition operator +(BlockPosition first, BlockPosition second)
        {
            return new BlockPosition(first.X + second.X, first.Y + second.Y, first.Z + second.Z);        
        }

        public static BlockPosition operator +(BlockPosition first, Vector3 second)
        {
            return new BlockPosition(first.X + (int)second.X, first.Y + (int)second.Y, first.Z + (int)second.Z);
        }
    }
}
