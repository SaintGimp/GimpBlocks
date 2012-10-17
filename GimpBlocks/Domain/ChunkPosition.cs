using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class ChunkPosition
    {
        public int X;
        public int Y;
        public int Z;

        public ChunkPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public ChunkPosition(Vector3 vector)
        {
            X = (int)vector.X;
            Y = (int)vector.Y;
            Z = (int)vector.Z;
        }

        public ChunkPosition Left
        {
            get { return new ChunkPosition(X - 1, Y, Z); }
        }

        public ChunkPosition Right
        {
            get { return new ChunkPosition(X + 1, Y, Z); }
        }

        public ChunkPosition Front
        {
            get { return new ChunkPosition(X, Y, Z + 1); }
        }

        public ChunkPosition Back
        {
            get { return new ChunkPosition(X, Y, Z - 1); }
        }

        public ChunkPosition Up
        {
            get { return new ChunkPosition(X, Y + 1, Z); }
        }

        public ChunkPosition Down
        {
            get { return new ChunkPosition(X, Y - 1, Z); }
        }

        public static ChunkPosition operator +(ChunkPosition first, Vector3 second)
        {
            return new ChunkPosition(first.X + (int)second.X, first.Y + (int)second.Y, first.Z + (int)second.Z);
        }

        public int DistanceSquared(ChunkPosition otherPosition)
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

        public bool Between(ChunkPosition minimum, ChunkPosition maximum)
        {
            return X >= minimum.X && X <= maximum.X &&
                Y >= minimum.Y && Y <= maximum.Y &&
                    Z >= minimum.Z && Z <= maximum.Z;
        }
    }
}
