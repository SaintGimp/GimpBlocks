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
        private int _propogations;


        public LightArray(int xDimension, int yDimension, int zDimension, BlockArray blockArray)
            : base(xDimension, yDimension, zDimension)
        {
            _blockArray = blockArray;
        }

        public void Calculate()
        {
            _propogations = 0;

            CastSunlight();
            ForEach(BeginPropagateLight);

            Debug.WriteLine("Light propogations: " + _propogations);
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

        void BeginPropagateLight(int lightValue, int x, int y, int z)
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

            // At this point we know we're in full sunlight so there's no need to propagate up or down
            // because both directions are going to be either sunlit or solid
            PropagateLight(newLightValue, x - 1, y, z);
            PropagateLight(newLightValue, x + 1, y, z);
            PropagateLight(newLightValue, x, y, z - 1);
            PropagateLight(newLightValue, x, y, z + 1);
        }

        void PropagateLight(int lightValue, int x, int y, int z)
        {
            _propogations++;

            if (!IsInBounds(x, y, z))
            {
                return;
            }

            var linearIndex = LinearIndex(x, y, z);

            if (_blockArray[linearIndex].IsSolid)
            {
                return;
            }

            if (this[linearIndex] >= lightValue)
            {
                return;
            }

            this[linearIndex] = lightValue;

            if (lightValue == 1)
            {
                return;
            }

            // TODO: we want to track sunlight and light sources separately, maybe?

            var newLightValue = lightValue - 1;

            PropagateLight(newLightValue, x - 1, y, z);
            PropagateLight(newLightValue, x + 1, y, z);
            PropagateLight(newLightValue, x, y - 1, z);
            PropagateLight(newLightValue, x, y + 1, z);
            PropagateLight(newLightValue, x, y, z - 1);
            PropagateLight(newLightValue, x, y, z + 1);
        }
    }
}
