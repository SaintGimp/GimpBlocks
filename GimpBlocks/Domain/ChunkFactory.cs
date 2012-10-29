using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class ChunkFactory
    {
        readonly IEnvironmentGenerator environmentGenerator;
        readonly Func<IChunkRenderer> chunkRendererFactory;
        readonly BlockPrototypeMap prototypeMap;

        public ChunkFactory(IEnvironmentGenerator environmentGenerator, Func<IChunkRenderer> chunkRendererFactory)
        {
            this.environmentGenerator = environmentGenerator;
            this.chunkRendererFactory = chunkRendererFactory;
            prototypeMap = new BlockPrototypeMap();
        }

        public Chunk Create(World world, ChunkPosition chunkPosition)
        {
            return new Chunk(world, chunkPosition, environmentGenerator, chunkRendererFactory(), prototypeMap);
        }
    }
}
