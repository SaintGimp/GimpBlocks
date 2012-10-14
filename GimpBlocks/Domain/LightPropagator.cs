using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class LightPropagator
    {
        public readonly int MaximumLightLevel = 15;

        public void SetAndPropagateLightInChunk(Chunk chunk, ChunkBlockPosition blockPosition, byte lightLevel)
        {
            if (chunk.IsSolid(blockPosition))
            {
                return;
            }

            var existingLightValue = chunk.GetLightLevel(blockPosition);
            if (existingLightValue < lightLevel)
            {
                chunk.SetLightLevel(blockPosition, lightLevel);
                PropagateLightInChunk(chunk, blockPosition);
            }
        }

        public void PropagateLightInChunk(Chunk chunk, ChunkBlockPosition blockPosition)
        {
            if (chunk.IsSolid(blockPosition))
            {
                return;
            }

            var newLightValue = (byte)(chunk.GetLightLevel(blockPosition) - 1);

            RecursivelyPropagateLight(chunk, blockPosition.Left, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Right, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Up, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Down, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Front, newLightValue);
            RecursivelyPropagateLight(chunk, blockPosition.Back, newLightValue);
        }

        void RecursivelyPropagateLight(Chunk chunk, ChunkBlockPosition blockPosition, byte incomingLightValue)
        {
            if (!blockPosition.IsInBounds())
            {
                return;
            }

            if (chunk.GetLightLevel(blockPosition) >= incomingLightValue)
            {
                return;
            }

            if (chunk.IsSolid(blockPosition))
            {
                return;
            }

            if (incomingLightValue <= 1)
            {
                return;
            }

            chunk.SetLightLevel(blockPosition, incomingLightValue);

            var outgoingLightLevel = (byte)(incomingLightValue - 1);

            RecursivelyPropagateLight(chunk, blockPosition.Left, outgoingLightLevel);
            RecursivelyPropagateLight(chunk, blockPosition.Right, outgoingLightLevel);
            RecursivelyPropagateLight(chunk, blockPosition.Up, outgoingLightLevel);
            RecursivelyPropagateLight(chunk, blockPosition.Down, outgoingLightLevel);
            RecursivelyPropagateLight(chunk, blockPosition.Front, outgoingLightLevel);
            RecursivelyPropagateLight(chunk, blockPosition.Back, outgoingLightLevel);
        }
    }
}
