    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class Block
    {
        public BlockPrototype Prototype { get; private set; }
        public BlockPosition Position { get; private set; }
        public byte LightLevel { get; private set; }

        public Block(BlockPrototype prototype, BlockPosition position, byte lightLevel)
        {
            Position = position;
            LightLevel = lightLevel;
            Prototype = prototype;
        }

        public BoundingBox BoundingBox
        {
            get { return new BoundingBox(new Vector3(Position.X, Position.Y, Position.Z),
                new Vector3(Position.X + 1, Position.Y + 1, Position.Z + 1)); }
        }

        public bool CanPropagateLight
        {
            get { return Prototype.CanPropagateLight; }
        }

        public bool CanBeSeenThrough
        {
            get { return Prototype.CanBeSeenThrough; }
        }

        public bool CanBeSelected
        {
            get { return Prototype.CanBeSelected; }
        }
    }
}
