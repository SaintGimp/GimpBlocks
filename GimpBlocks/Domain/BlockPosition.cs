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

    // Creating structs all the time in order to pass around sets of numbers might
    // have a small adverse impact on performance (on the order of maybe 2% in release build). Right now
    // we're favoring readable code over extreme performance, though.

    // TODO: experiment with MethodImplOptions.AggressiveInlining in CLR 4.5
	
    public struct BlockPosition
    {
        // TODO: do we want to make this immutable all the time? Right now we
        // use it both ways (mutable and immutable)

        public int X;
        public int Y;
        public int Z;

        public BlockPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BlockPosition(Vector3 vector)
        {
            X = (int)vector.X;
            Y = (int)vector.Y;
            Z = (int)vector.Z;
        }

        public BlockPosition(ChunkPosition chunkPosition, RelativeBlockPosition relativeBlockPosition)
        {
            X = chunkPosition.X * Chunk.XDimension + relativeBlockPosition.X;
            Y = relativeBlockPosition.Y;
            Z = chunkPosition.Z * Chunk.XDimension + relativeBlockPosition.Z;
        }

        // The chunk coordinates and relative block position are encoded in the absolute
        // world block position.  The higher bits are the chunk coordinate and the lower
        // bits are the relative coordinate. For negative chunk coordinates this kind of
        // makes things backwards in terms of how data is stored in the chunk arrays but
        // it turns out it doesn't actually matter because it gets reversed again when
        // we display it (I think - verify this).

        // Performance-wise it appears to be significantly better to calculate these as
        // needed rather than calculate them once and store them (presumably because of
        // the extra work involved in copying the struct by value for function calls).

        public ChunkPosition ChunkPosition
        {
            get { return new ChunkPosition(X >> Chunk.Log2X, Z >> Chunk.Log2Z); }
        }

        public RelativeBlockPosition RelativeBlockPosition
        {
            get { return new RelativeBlockPosition(X & Chunk.BitmaskX, Y, Z & Chunk.BitmaskZ); }
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

        public IEnumerable<BlockPosition> Neighbors
        {
            get
            {
                return new[]
                {
                    Left,
                    Right,
                    Front,
                    Back,
                    Up,
                    Down
                };
            }
        }

        public BlockPosition Relative(int relativeX, int relativeY, int relativeZ)
        {
            return new BlockPosition(X + relativeX, Y + relativeY, Z + relativeZ);
        }

        public static BlockPosition operator +(BlockPosition first, Vector3 second)
        {
            return new BlockPosition(first.X + (int)second.X, first.Y + (int)second.Y, first.Z + (int)second.Z);
        }

        public int DistanceSquared(BlockPosition otherPosition)
        {
            int deltaX = X - otherPosition.X;
            int deltaY = Y - otherPosition.Y;
            int deltaZ = Z - otherPosition.Z;

            return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        }

        public static explicit operator Vector3(BlockPosition position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }
    }
}
