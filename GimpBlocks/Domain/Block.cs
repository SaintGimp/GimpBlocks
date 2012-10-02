    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class Block
    {
        public BlockPrototype Prototype;
        public ChunkBlockPosition Position;

        public Block(BlockPrototype prototype, ChunkBlockPosition position)
        {
            Prototype = prototype;
            Position = position;
        }

        public BoundingBox BoundingBox
        {
            get { return new BoundingBox(new Vector3(Position.X, Position.Y, Position.Z), new Vector3(Position.X + 1, Position.Y + 1, Position.Z + 1));}
        }
    }
}
