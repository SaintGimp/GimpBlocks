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

        // TODO: Is it better to have these create new objects or to mutate
        // the existing object?  Tradeoff between immutability benefits and performance.
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

        public BoundingBox BoundingBox
        {
            get { return new BoundingBox(new Vector3(X, Y, Z), new Vector3(X + 1, Y + 1, Z + 1)); }
        }

        public static implicit operator Vector3(ChunkBlockPosition location)
        {
            return new Vector3(location.X, location.Y, location.Z);
        }

        public static implicit operator ChunkBlockPosition(Vector3 location)
        {
            return new ChunkBlockPosition((int)location.X, (int)location.Y, (int)location.Z);
        }

        public static ChunkBlockPosition operator +(ChunkBlockPosition first, Vector3 second)
        {
            return new ChunkBlockPosition(first.X + (int)second.X, first.Y + (int)second.Y, first.Z + (int)second.Z);
        }

        public int DistanceSquared(ChunkBlockPosition otherPosition)
        {
            int deltaX = this.X - otherPosition.X;
            int deltaY = this.Y - otherPosition.Y;
            int deltaZ = this.Z - otherPosition.Z;

            return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        }

        public bool IsInBounds()
        {
            return X >= 0 && X < Chunk.XDimension &&
                Y >= 0 && Y < Chunk.YDimension &&
                Z >= 0 && Z < Chunk.ZDimension;
        }
    }
}
