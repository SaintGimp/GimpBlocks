using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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

        public void Calculate()
        {
            ResetLight();
            CastSunlight();
            ForEach(BeginPropogateLight);
        }

        void ResetLight()
        {
            for (int x = 0; x < BufferSize; x++)
            {
                this[x] = 0;
            }
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
                }
            }
        }

        void BeginPropogateLight(int lightValue, int x, int y, int z)
        {
            if (lightValue < MaximumLightLevel)
            {
                return;
            }

            if (_blockArray[x, y, z].IsSolid)
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

            if (_blockArray[x, y, z].IsSolid)
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
