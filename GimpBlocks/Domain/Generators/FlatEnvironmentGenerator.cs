using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class FlatEnvironmentGenerator : IEnvironmentGenerator
    {
        public static readonly int GroundLevel = Chunk.YDimension / 2;

        public void Generate(Chunk chunk)
        {
            for (int x = 0; x < Chunk.XDimension; x++)
            {
                for (int y = 0; y < Chunk.YDimension; y++)
                {
                    for (int z = 0; z < Chunk.ZDimension; z++)
                    {
                        var blockType = y <= GroundLevel ? BlockPrototype.StoneBlock : BlockPrototype.AirBlock;
                        chunk.SetBlockPrototype(new RelativeBlockPosition(x, y, z), blockType);
                    }
                }
            }
        }
    }
}
