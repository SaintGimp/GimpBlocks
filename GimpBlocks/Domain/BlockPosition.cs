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

    public struct BlockPosition
    {
        public int X;
        public int Y;
        public int Z;

        public BlockPosition(int x, int y, int z)
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

        public static BlockPosition operator +(BlockPosition first, Vector3 second)
        {
            return new BlockPosition(first.X + (int)second.X, first.Y + (int)second.Y, first.Z + (int)second.Z);
        }

        public int DistanceSquared(BlockPosition otherPosition)
        {
            int deltaX = this.X - otherPosition.X;
            int deltaY = this.Y - otherPosition.Y;
            int deltaZ = this.Z - otherPosition.Z;

            return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        }
    }
}
