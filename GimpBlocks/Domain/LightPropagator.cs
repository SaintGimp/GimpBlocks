using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class LightPropagator
    {
        public const int MaximumLightLevel = 15;
        
        public static int TotalNumberOfRecursions = 0;
        public int NumberOfRecursions = 0;

        // TODO: add a propagate sunlight method that skips up and down since up for any sunlit block
        // must have also been sunlit and down will either be sunlit or solid

        public void PropagateLightFromBlock(World world, BlockPosition blockPosition)
        {
            NumberOfRecursions = 0;
            // TODO: we could test the block position and see if it is in the home
            // chunk, and if it is, go straight to the chunk rather than through the world, maybe

            var block = world.GetBlockAt(blockPosition);

            if (!block.CanPropagateLight)
            {
                return;
            }

            var newLightValue = (byte)(block.LightLevel - 1);

            RecursivelyPropagateLight(world, blockPosition.Left, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Right, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Up, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Down, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Front, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Back, newLightValue);
        }

        void RecursivelyPropagateLight(World world, BlockPosition blockPosition, byte incomingLightValue)
        {
            TotalNumberOfRecursions++;
            NumberOfRecursions++;

            if (incomingLightValue <= 1)
            {
                return;
            }

            var block = world.GetBlockAt(blockPosition);

            if (block.LightLevel >= incomingLightValue)
            {
                return;
            }

            if (!block.CanPropagateLight)
            {
                return;
            }

            world.SetLightLevel(blockPosition, incomingLightValue);

            var outgoingLightLevel = (byte)(incomingLightValue - 1);

            RecursivelyPropagateLight(world, blockPosition.Left, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Right, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Up, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Down, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Front, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Back, outgoingLightLevel);
        }
    }
}
