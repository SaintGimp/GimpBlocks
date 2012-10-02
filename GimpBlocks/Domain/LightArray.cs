using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class LightArray : Array3<int>
    {
        public readonly int MaximumLightLevel = 15;
        readonly BlockArray _blockArray;

        public LightArray(int xDimension, int yDimension, int zDimension, BlockArray blockArray)
            : base(xDimension, yDimension, zDimension)
        {
            _blockArray = blockArray;
        }

        public void Calculate(Chunk chunk)
        {
            CastSunlight();
            var propogator = new LightPropagator();
            ForEach((lightLevel, x, y, z) =>
            {
                // TODO: we don't need to propogate light from blocks that contain only light that's already
                // been propogated from elsewhere. For now we can propogate only if the light is full strength,
                // but that won't work for light sources that are less than full strength.  Maybe have a source
                // and destination light map so we don't have to deal with half-calculated data?

                var blockPosition = new ChunkBlockPosition(x, y, z);
                if (chunk.GetLightLevel(blockPosition) == MaximumLightLevel)
                {
                    propogator.PropagateLightFromChunk(chunk, blockPosition);
                }
            });
        }

        void CastSunlight()
        {
            for (int x = 0; x < XDimension; x++)
            {
                for (int z = 0; z < ZDimension; z++)
                {
                    int y = YDimension - 1;
                    while (y >= 0 && !_blockArray[x, y, z].IsSolid)
                    {
                        this[x, y, z] = MaximumLightLevel;
                        y--;
                    }

                    // Anything not in sunlight starts out completely dark
                    while (y >= 0)
                    {
                        this[x, y, z] = 0;
                        y--;
                    }
                }
            }
        }


    }
}
