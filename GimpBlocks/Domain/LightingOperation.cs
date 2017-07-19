using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimpBlocks
{
    public class LightingOperation
    {
        private World world;
        private BlockPosition blockPosition;
        private byte incomingLightLevel;
        private bool isSunlight;

        public LightingOperation(World world, BlockPosition blockPosition, byte incomingLightLevel, bool isSunlight)
        {
            this.world = world;
            this.blockPosition = blockPosition;
            this.incomingLightLevel = incomingLightLevel;
            this.isSunlight = isSunlight;
        }

        public IEnumerable<LightingOperation> Propagate()
        {
            // We assume that the target block already has its own light level set.
            // This operation will set the light values for all of its neighbors, and if that neighbor can propagate light of its own, 
            // it will create new pending lighting operations to be queued and executed later.

            byte outgoingLightLevel = (byte)(incomingLightLevel - 1);

            IEnumerable<BlockPosition> neighbors;
            if (isSunlight)
            {
                // If this is a sunlit block then we know we don't have to consider up (because the one above must also be sunlit)
                // or down (because it'll either be sunlit too or solid).
                neighbors = new[] { blockPosition.Left, blockPosition.Right, blockPosition.Front, blockPosition.Back };
            }
            else
            {
                neighbors = blockPosition.Neighbors;
            }

            var pendingOperations = neighbors.Select(neighbor => PropagateLightToBlock(world, neighbor, outgoingLightLevel)).Where(operation => operation != null);
            return pendingOperations;
        }

        private LightingOperation PropagateLightToBlock(World world, BlockPosition blockPosition, byte outgoingLightLevel)
        {
            var block = world.GetBlockAt(blockPosition);

            if (!block.CanPropagateLight)
            {
                // Block is solid, nothing to do
                return null;
            }

            if (block.LightLevel >= outgoingLightLevel)
            {
                // Block already has as much or more light than we were going to give it, propagation stops
                return null;
            }

            world.SetLightLevel(blockPosition, outgoingLightLevel);

            if (outgoingLightLevel == 1)
            {
                // This block got the lowest possible light level so it has nothing to propagate further
                return null;
            }

            // This block got light and has something left over to propagate further
            return new LightingOperation(world, blockPosition, outgoingLightLevel, false);
        }
    }
}
