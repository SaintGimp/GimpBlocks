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

        public void PropagateSunlightFromBlock(World world, BlockPosition blockPosition)
        {
            NumberOfRecursions = 0;

            var block = world.GetBlockAt(blockPosition);

            if (!block.CanPropagateLight)
            {
                return;
            }

            var newLightValue = (byte)(block.LightLevel - 1);

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
            NumberOfRecursions = 0;

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

            RecursivelyPropagateLight(world, blockPosition.Left, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Right, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Up, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Down, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Front, outgoingLightLevel);
            RecursivelyPropagateLight(world, blockPosition.Back, outgoingLightLevel);
        }
    }
}
