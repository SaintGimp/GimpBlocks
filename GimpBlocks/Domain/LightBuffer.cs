using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class LightBuffer : VoxelBuffer<int>
    {
        public readonly int MaximumLightLevel = 15;
        readonly BlockBuffer _blockBuffer;

        public LightBuffer(int xDimension, int yDimension, int zDimension, BlockBuffer blockBuffer)
            : base(xDimension, yDimension, zDimension)
        {
            _blockBuffer = blockBuffer;
        }

        public void Calculate()
        {
            CastSunlight();
            ForEach(BeginPropogateLight);
        }

        void CastSunlight()
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    int y = 15;
                    while (y >= 0 && !_blockBuffer[x, y, z].IsSolid)
                    {
                        this[x, y, z] = MaximumLightLevel;
                        y--;
                    }
                }
            }
        }

        void BeginPropogateLight(int lightValue, int x, int y, int z)
        {
            if (lightValue < MaximumLightLevel)
            {
                return;
            }

            if (_blockBuffer[x, y, z].IsSolid)
            {
                return;
            }

            var newLightValue = lightValue - 1;

            PropogateLight(newLightValue, x - 1, y, z);
            PropogateLight(newLightValue, x + 1, y, z);
            PropogateLight(newLightValue, x, y - 1, z);
            PropogateLight(newLightValue, x, y + 1, z);
            PropogateLight(newLightValue, x, y, z - 1);
            PropogateLight(newLightValue, x, y, z + 1);
        }

        void PropogateLight(int lightValue, int x, int y, int z)
        {
            if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 15)
            {
                return;
            }

            if (this[x, y, z] >= lightValue)
            {
                return;
            }

            if (_blockBuffer[x, y, z].IsSolid)
            {
                return;
            }

            this[x, y, z] = lightValue;

            if (lightValue == 1)
            {
                return;
            }

            // TODO: we want to track sunlight and light sources separately, maybe, and not propogate up or down
            // if sunlight is full strength
            // TODO: can we prevent backtracking?

            var newLightValue = lightValue - 1;

            PropogateLight(newLightValue, x - 1, y, z);
            PropogateLight(newLightValue, x + 1, y, z);
            PropogateLight(newLightValue, x, y - 1, z);
            PropogateLight(newLightValue, x, y + 1, z);
            PropogateLight(newLightValue, x, y, z - 1);
            PropogateLight(newLightValue, x, y, z + 1);
        }
    }
}
