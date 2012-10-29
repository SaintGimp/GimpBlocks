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
        public static IWorldRenderer _worldRenderer;
        public static IBoundingBoxRenderer _boundingBoxRenderer;
        public static ChunkFactory _chunkFactory;
        public static World _world;
        public static BlockPosition _groundBlockPosition = 
            new BlockPosition(Chunk.XDimension / 2, FlatEnvironmentGenerator.GroundLevel, Chunk.ZDimension / 2);

        Establish context = () =>
        {
            _worldRenderer = Substitute.For<IWorldRenderer>();
            _boundingBoxRenderer = Substitute.For<IBoundingBoxRenderer>();
            _chunkFactory = new ChunkFactory(new FlatEnvironmentGenerator(), () => Substitute.For<IChunkRenderer>());
        };

        public static void CreateWorld(int viewDistance)
        {
            _world = new World(viewDistance, _chunkFactory, _worldRenderer, _boundingBoxRenderer);
            _world.Generate();
        }
    }
}
