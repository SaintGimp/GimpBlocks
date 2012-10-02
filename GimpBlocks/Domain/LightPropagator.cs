using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class LightPropagator
    {
        public readonly int MaximumLightLevel = 15;

        public void PropagateLightFromChunk(Chunk chunk, ChunkBlockPosition blockPosition)
        {
            if (chunk.IsSolid(blockPosition))
            {
                return;
            }

            var newLightValue = chunk.GetLightLevel(blockPosition) - 1;

            RecursivelyPropagateLight(chunk, blockPosition.Left, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Right, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Up, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Down, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Front, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Back, newLightValue);
        }

        public void RecursivelyPropagateLight(Chunk chunk, ChunkBlockPosition blockPosition, int lightValue)
        {
            if (!blockPosition.IsInBounds())
            {
                return;
            }

            if (chunk.GetLightLevel(blockPosition) >= lightValue)
            {
                return;
            }

            if (chunk.IsSolid(blockPosition))
            {
                return;
            }

            if (lightValue <= 1)
            {
                return;
            }

            chunk.SetLightLevel(blockPosition, lightValue);

            var newLightValue = lightValue - 1;

            RecursivelyPropagateLight(chunk, blockPosition.Left, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Right, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Up, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Down, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Front, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Back, newLightValue);
        }
    }
}
