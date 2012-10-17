using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LibNoise;
using LibNoise.Filter;
using LibNoise.Modifier;
using LibNoise.Primitive;
using LibNoise.Tranformer;
using Microsoft.Xna.Framework;
using StructureMap;

namespace GimpBlocks
{
    public class World
        : IListener<PlaceBlock>,
        IListener<DestroyBlock>,
        IListener<BlockSelectionChanged>,
        IListener<ProfileWorldGeneration>
    {
        // Design tradeoffs: We could have one array of structs that contain all block information.  The advantage there
        // is that if we need to access multiple pieces of information about a block simultaneously, we only need to do one
        // array lookup.  Or we could store each type of information in a separate array which requires a separate array
        // lookup for each piece of information, but in the case where we need to iterate a lot over one type of information,
        // we would get fewer cache misses.  With that approach we could also optimize storage for each type of information separately, so
        // something that's usually sparse could be stored in a sparse octtree where something else that varies a lot could
        // be an an array.  It's not clear which strategy is best in the long term.

        public const byte MaximumLightLevel = 15;

        readonly IWorldRenderer _renderer;
        readonly Func<World, int, int, Chunk> _chunkFactory;
        readonly BoundingBoxRenderer _boundingBoxRenderer;
        readonly Chunk[,] _chunks;
        readonly int _worldSizeInChunks = 4;

        Block _selectedBlock;
        BlockPosition _selectedBlockPlacePosition;

        public World(IWorldRenderer renderer, Func<World, int, int, Chunk> chunkFactory, BoundingBoxRenderer boundingBoxRenderer)
        {
            _renderer = renderer;
            _chunkFactory = chunkFactory;
            _boundingBoxRenderer = boundingBoxRenderer;
            _chunks = new Chunk[_worldSizeInChunks,_worldSizeInChunks];
        }

        // TODO: there may be useful profiling code in the sample code at http://blog.eckish.net/2011/01/10/perfecting-a-cube/

        public void Generate()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var generateTimer = new Stopwatch();
            generateTimer.Start();
            for (int x = 0; x <= _chunks.GetUpperBound(0); x++)
            {
                for (int z = 0; z <= _chunks.GetUpperBound(1); z++)
                {
                    var chunk = _chunkFactory(this, x, z);
                    chunk.Generate();
                    _chunks[x, z] = chunk;
                }
            }
            Trace.WriteLine(string.Format("chunk terrain: {0} ms", generateTimer.ElapsedMilliseconds));

            RebuildChunks(AllChunks);

            stopwatch.Stop();
            Trace.WriteLine(string.Format("Generated world in {0} ms", stopwatch.ElapsedMilliseconds));
            Trace.WriteLine(string.Format("World retrieved {0} blocks", NumberOfBlocksRetrieved));
            Trace.WriteLine(string.Format("Light propagations recursed {0} times", LightPropagator.TotalNumberOfRecursions));
        }

        IEnumerable<Chunk> AllChunks
        {
            get
            {
                // TODO: how can we more easily convert T[,] into IEnumerable<T>?
                var chunkList = new List<Chunk>();
                foreach (var chunk in _chunks)
                {
                    chunkList.Add(chunk);
                }
                return chunkList;
            }
        }

        void RebuildChunks(Chunk chunk)
        {
            RebuildChunks(new [] { chunk });
        }

        void RebuildChunks(IEnumerable<Chunk> chunks)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var chunk in chunks)
            {
                chunk.SetInitialLighting();
            }

            stopwatch.Stop();
            Trace.WriteLine(string.Format("initialize lighting: {0} ms", stopwatch.ElapsedMilliseconds));
            stopwatch.Restart();

            foreach (var chunk in chunks)
            {
                chunk.CalculateLighting();
            }

            stopwatch.Stop();
            Trace.WriteLine(string.Format("calculate lighting: {0} ms", stopwatch.ElapsedMilliseconds));
            stopwatch.Restart();

            foreach (var chunk in chunks)
            {
                chunk.Tessellate();
            }

            stopwatch.Stop();
            Trace.WriteLine(string.Format("tessellation: {0} ms", stopwatch.ElapsedMilliseconds));

            foreach (var chunk in chunks)
            {
                EventAggregator.Instance.SendMessage(new ChunkRebuilt { Chunk = chunk });
            }
        }


        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            _renderer.Draw(cameraLocation, originBasedViewMatrix, projectionMatrix);

            // TODO: drawing near to far chunks may help by allowing the GPU to do occlusion culling

            foreach (var chunk in _chunks)
            {
                chunk.Draw(cameraLocation, originBasedViewMatrix, projectionMatrix);
            }

            if (_selectedBlock != null)
            {
                var boundingBox = _selectedBlock.BoundingBox;
                var offset = new Vector3(0.003f);
                var selectionBox = new BoundingBox(boundingBox.Min - offset, boundingBox.Max + offset);
                _boundingBoxRenderer.Draw(selectionBox, cameraLocation, originBasedViewMatrix, projectionMatrix);
            }
        }

        public void Handle(PlaceBlock message)
        {
            if (_selectedBlock != null)
            {
                // TODO: this will crash if we try to place a block outside of the loaded
                // chunks. In the long run this won't be an issue because the user shouldn't ever
                // be close to the edge of the loaded world.
                SetBlockPrototype(_selectedBlockPlacePosition, BlockPrototype.StoneBlock);

                RebuildAffectedChunks();
            }
        }

        void RebuildAffectedChunks()
        {
            var affectedVolume = new BlockVolume(_selectedBlock.Position, MaximumLightLevel);
            var chunks = GetChunksIntersectingVolume(affectedVolume);
            RebuildChunks(chunks);
        }

        IEnumerable<Chunk> GetChunksIntersectingVolume(BlockVolume affectedVolume)
        {
            return AllChunks.Where(chunk =>
                chunk.Position.Between(affectedVolume.Minimum.ChunkPosition, affectedVolume.Maximum.ChunkPosition)
                ).ToList();
        }

        public void Handle(DestroyBlock message)
        {
            if (_selectedBlock != null)
            {
                SetBlockPrototype(_selectedBlock.Position, BlockPrototype.AirBlock);

                RebuildAffectedChunks();
            }
        }

        Chunk GetChunkFor(BlockPosition blockPosition)
        {
            var chunkPosition = blockPosition.ChunkPosition;

            // TODO: maybe support multi-chunk vertically later
            if (blockPosition.Y < 0 || blockPosition.Y >= Chunk.YDimension)
            {
                return null;
            }

            return IsLoaded(chunkPosition) ? _chunks[chunkPosition.X, chunkPosition.Z] : null;
        }

        bool IsLoaded(ChunkPosition chunkPosition)
        {
            return (chunkPosition.X >= 0 && chunkPosition.X < _worldSizeInChunks && chunkPosition.Z >= 0 && chunkPosition.Z < _worldSizeInChunks);
        }

        public void SetLightLevel(BlockPosition blockPosition, byte lightLevel)
        {
            var chunk = GetChunkFor(blockPosition);

            chunk.SetLightLevel(blockPosition.RelativeBlockPosition, lightLevel);
        }

        public void SetBlockPrototype(BlockPosition blockPosition, BlockPrototype prototype)
        {
            var chunk = GetChunkFor(blockPosition);

            chunk.SetBlockPrototype(blockPosition.RelativeBlockPosition, prototype);
        }

        public Block GetBlockAt(BlockPosition blockPosition)
        {
            NumberOfBlocksRetrieved++;

            var chunk = GetChunkFor(blockPosition);

            if (chunk != null)
            {
                var relativePosition = blockPosition.RelativeBlockPosition;
                var prototype = chunk.GetBlockPrototype(relativePosition);
                var lightLevel = chunk.GetLightLevel(relativePosition);

                return new Block(prototype, blockPosition, lightLevel);
            }
            else
            {
                return new Block(BlockPrototype.VoidBlock, blockPosition, 8);
            }
        }

        public int NumberOfBlocksRetrieved;

        public void Handle(BlockSelectionChanged message)
        {
            _selectedBlock = message.SelectedBlock;
            _selectedBlockPlacePosition = message.SelectedPlacePosition;
        }

        public void Handle(ProfileWorldGeneration message)
        {
            var stopwatch = new Stopwatch();

            int iterations = 10;
            for (int x = 0; x < iterations; x++)
            {
                foreach (var chunk in _chunks)
                {
                    chunk.Dispose();
                }

                stopwatch.Start();

                Generate();

                stopwatch.Stop();
            }

            var result = string.Format("Average world generation time over {0} iterations: {1}", iterations,
                stopwatch.ElapsedMilliseconds / iterations);
            Trace.WriteLine(result);
        }
    }
}
