﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class LightPropagator
    {
        public static int TotalNumberOfRecursions = 0;

        public void PropagateSunlightFromBlock(World world, BlockPosition blockPosition)
        {
            // We assume that the block position passed to us has full sunlight and can
            // propogate light.

            const byte newLightValue = (byte)(World.MaximumLightLevel - 1);

            // We know we're starting with a sunlit block so we don't need to recurse up because
            // anything above this block is either sunlit or void. We don't need to recurse down
            // because anything below this block will either be sunlit or solid.

            RecursivelyPropagateLight(world, blockPosition.Left, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Right, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Front, newLightValue);
            RecursivelyPropagateLight(world, blockPosition.Back, newLightValue);
        }

        public void PropagateLightFromBlock(World world, BlockPosition blockPosition)
        {
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

            if (incomingLightValue < 1)
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
            //Trace.WriteLine(string.Format("Replaced light value {0} with {1} at {2}", block.LightLevel, incomingLightValue, blockPosition));

            var outgoingLightLevel = (byte)(incomingLightValue - 1);

            // TODO: right now this is a depth-first fill.  We really need to do a breadth-first fill so that blocks gets filled
            // first with the highest possible levels of light and we don't spend time overwriting lower levels from long recursion
            // chains that doubled back with higher levels from shorter recursion chains.
            // http://en.wikipedia.org/wiki/Iterative_deepening
            // It would probably be smart to get some unit tests going before messing with this too much.

            // It might also be interesting to do iterative deepening across the whole chunk so that multiple
            // sunlit blocks contributing to a large dark space fill it in cooperatively rather than the first
            // block flood-filling the whole space, then the second doing the same with slightly different values.
            // That would probably not be a net win but it's worth thinking about.  We could do a first pass on all
            // sunlit blocks and make a list of just the ones that managed to propogate light somewhere, then do
            // iterative deepening on just those.

            // Another way of looking at it might be to put these recursive visits into a queue and pull them out
            // in a FIFO fashion, so you'd do all of the brightest blocks first, then all of the second-brightest
            // blocks, then all of the third-brightest, etc. What about garbage collection, though?

            if (outgoingLightLevel > 0)
            {
                RecursivelyPropagateLight(world, blockPosition.Left, outgoingLightLevel);
                RecursivelyPropagateLight(world, blockPosition.Right, outgoingLightLevel);
                RecursivelyPropagateLight(world, blockPosition.Up, outgoingLightLevel);
                RecursivelyPropagateLight(world, blockPosition.Down, outgoingLightLevel);
                RecursivelyPropagateLight(world, blockPosition.Front, outgoingLightLevel);
                RecursivelyPropagateLight(world, blockPosition.Back, outgoingLightLevel);
            }
        }
    }
}
