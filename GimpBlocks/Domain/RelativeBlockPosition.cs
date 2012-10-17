using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    // TODO: experiment with MethodImplOptions.AggressiveInlining in CLR 4.5

    public struct RelativeBlockPosition
    {
        public int X;
        public int Y;
        public int Z;

        public RelativeBlockPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public RelativeBlockPosition Left
        {
            get { return new RelativeBlockPosition(X - 1, Y, Z); }
        }

        public RelativeBlockPosition Right
        {
            get { return new RelativeBlockPosition(X + 1, Y, Z); }
        }

        public RelativeBlockPosition Front
        {
            get { return new RelativeBlockPosition(X, Y, Z + 1); }
        }

        public RelativeBlockPosition Back
        {
            get { return new RelativeBlockPosition(X, Y, Z - 1); }
        }

        public RelativeBlockPosition Up
        {
            get { return new RelativeBlockPosition(X, Y + 1, Z); }
        }

        public RelativeBlockPosition Down
        {
            get { return new RelativeBlockPosition(X, Y - 1, Z); }
        }

        public static implicit operator Vector3(RelativeBlockPosition location)
        {
            return new Vector3(location.X, location.Y, location.Z);
        }

        public static implicit operator RelativeBlockPosition(Vector3 location)
        {
            return new RelativeBlockPosition((int)location.X, (int)location.Y, (int)location.Z);
        }
    }
}
