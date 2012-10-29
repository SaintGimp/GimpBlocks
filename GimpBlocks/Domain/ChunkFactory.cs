using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class ChunkFactory
    {
        readonly IEnvironmentGenerator _environmentGenerator;
        readonly Func<IChunkRenderer> _chunkRendererFactory;
        readonly BlockPrototypeMap _prototypeMap;

        public ChunkFactory(IEnvironmentGenerator environmentGenerator, Func<IChunkRenderer> chunkRendererFactory)
        {
            _environmentGenerator = environmentGenerator;
            _chunkRendererFactory = chunkRendererFactory;
            _prototypeMap = new BlockPrototypeMap();
        }

        public Chunk Create(World world, ChunkPosition chunkPosition)
        {
            return new Chunk(world, chunkPosition, _environmentGenerator, _chunkRendererFactory(), _prototypeMap);
        }
    }
}
