using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class LightPropagator
    {
        public readonly int MaximumLightLevel = 15;

        public void PropagateLightFromBlock(World world, BlockPosition blockPosition)
        {
            // TODO: if we get a block with all of our needed properties, we won't have
            // to go back to the world multiple times

            if (!world.CanPropagateLight(blockPosition))
            {
                return;
            }

            var newLightValue = (byte)(world.GetLightLevel(blockPosition) - 1);

            RecursivelyPropagateLight(world, blockPosition.Left, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Right, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Up, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Down, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Front, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Back, newLightValue);
        }

        void RecursivelyPropagateLight(World world, BlockPosition blockPosition, byte incomingLightValue)
        {
            if (world.GetLightLevel(blockPosition) >= incomingLightValue)
            {
                return;
            }

            if (!world.CanPropagateLight(blockPosition))
            {
                return;
            }

            if (incomingLightValue <= 1)
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
