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
            Y = chunkPosition.Y * Chunk.YDimension + relativeBlockPosition.Y;
            Z = chunkPosition.Z * Chunk.XDimension + relativeBlockPosition.Z;
        }

        public ChunkPosition ChunkPosition
        {
            get
            {
                int chunkX = X >> Chunk.Log2X;
                int chunkY = Y >> Chunk.Log2Y;
                int chunkZ = Z >> Chunk.Log2Z;
                return new ChunkPosition(chunkX, chunkY, chunkZ);
            }
        }

        public RelativeBlockPosition RelativeBlockPosition
        {
            get
            {
                var relativeX = X & Chunk.BitmaskX;
                var relativeY = Y & Chunk.BitmaskY;
                var relativeZ = Z & Chunk.BitmaskZ;
                return new RelativeBlockPosition(relativeX, relativeY, relativeZ);
            }
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

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}", X, Y, Z);
        }
    }
}
