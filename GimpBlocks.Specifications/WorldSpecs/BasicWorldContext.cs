using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using NSubstitute;

namespace GimpBlocks.Specifications.WorldSpecs  
{
    public class BasicWorldContext
    {
        public static IBoundingBoxRenderer boundingBoxRenderer;
        public static ChunkFactory chunkFactory;
        public static World world;
        public static BlockPosition groundBlockPosition = 
            new BlockPosition(Chunk.XDimension / 2, FlatEnvironmentGenerator.GroundLevel, Chunk.ZDimension / 2);

        Establish context = () =>
        {
            boundingBoxRenderer = Substitute.For<IBoundingBoxRenderer>();
            chunkFactory = new ChunkFactory(new FlatEnvironmentGenerator(), () => Substitute.For<IChunkRenderer>());
        };

        public static void CreateWorld(int viewDistance)
        {
            world = new World(viewDistance, chunkFactory, boundingBoxRenderer);
            world.Generate();
        }
    }
}
