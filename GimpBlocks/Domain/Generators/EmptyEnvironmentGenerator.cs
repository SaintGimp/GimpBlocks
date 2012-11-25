using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimpBlocks
{
    public class EmptyEnvironmentGenerator : IEnvironmentGenerator
    {
        public void Generate(Chunk chunk)
        {
            for (int x = 0; x < Chunk.XDimension; x++)
            {
                for (int y = 0; y < Chunk.YDimension; y++)
                {
                    for (int z = 0; z < Chunk.ZDimension; z++)
                    {
                        chunk.SetBlockPrototype(new RelativeBlockPosition(x, y, z), BlockPrototype.AirBlock);
                    }
                }
            }
        }
    }
}
