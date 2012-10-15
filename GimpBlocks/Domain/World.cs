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
        IListener<DestroyBlock>
    {
        // Design tradeoffs: We could have one array of structs that contain all block information.  The advantage there
        // is that if we need to access multiple pieces of information about a block simultaneously, we only need to do one
        // array lookup.  Or we could store each type of information in a separate array which requires a separate array
        // lookup for each piece of information, but in the case where we need to iterate a lot over one type of information,
        // we would get fewer cache misses.  With that approach we could also optimize storage for each type of information separately, so
        // something that's usually sparse could be stored in a sparse octtree where something else that varies a lot could
        // be an an array.  It's not clear which strategy is best in the long term.

        readonly IWorldRenderer _renderer;
        readonly Func<World, int, int, Chunk> _chunkFactory;
        readonly BlockPicker _blockPicker;
        readonly BoundingBoxRenderer _boundingBoxRenderer;
        readonly Chunk[,] _chunks;
        readonly int _worldSizeInChunks = 4;

        public World(IWorldRenderer renderer, Func<World, int, int, Chunk> chunkFactory, BlockPicker blockPicker, BoundingBoxRenderer boundingBoxRenderer)
        {
            _renderer = renderer;
            _chunkFactory = chunkFactory;
            _blockPicker = blockPicker;
            _boundingBoxRenderer = boundingBoxRenderer;
            _chunks = new Chunk[_worldSizeInChunks,_worldSizeInChunks];
        }

        // TODO: there may be useful profiling code in the sample code at http://blog.eckish.net/2011/01/10/perfecting-a-cube/

        public void Generate()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int x = 0; x <= _chunks.GetUpperBound(0); x++)
            {
                for (int z = 0; z <= _chunks.GetUpperBound(1); z++)
                {
                    var chunk = _chunkFactory(this, x, z);
                    chunk.Generate();
                    _chunks[x, z] = chunk;
                }
            }

            Rebuild();

            stopwatch.Stop();
            Trace.WriteLine(string.Format("Generated world in {0} ms", stopwatch.ElapsedMilliseconds));
        }

        void Rebuild()
        {
            foreach (var chunk in _chunks)
            {
                chunk.CalculateLighting();
            }

            foreach (var chunk in _chunks)
            {
                chunk.BuildGeometry();
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

            if (_blockPicker.SelectedBlock != null)
            {
                var boundingBox = _blockPicker.SelectedBlock.BoundingBox;
                var offset = new Vector3(0.003f);
                var selectionBox = new BoundingBox(boundingBox.Min - offset, boundingBox.Max + offset);
                _boundingBoxRenderer.Draw(selectionBox, cameraLocation, originBasedViewMatrix, projectionMatrix);
            }
        }

        public void Handle(PlaceBlock message)
        {
            //if (_blockPicker.SelectedBlock != null)
            //{
            //    _chunk.SetBlockPrototype(_blockPicker.SelectedPlacePosition, _prototypeMap[1]);

            //    Rebuild();
            //}
        }

        public void Handle(DestroyBlock message)
        {
            //if (_blockPicker.SelectedBlock != null)
            //{
            //    _chunk.SetBlockPrototype(_blockPicker.SelectedPlacePosition, _prototypeMap[0]);

            //    Rebuild();
            //}
        }

        Chunk GetChunkFor(BlockPosition blockPosition)
        {
            // TODO: optimize
            if (blockPosition.Y < 0 || blockPosition.Y >= Chunk.YDimension)
            {
                return null;
            }

            int chunkX = blockPosition.X >> Chunk.Log2X;
            int chunkZ = blockPosition.Z >> Chunk.Log2Z;
            
            return IsLoaded(chunkX, chunkZ) ? _chunks[chunkX, chunkZ] : null;
        }

        bool IsLoaded(int chunkX, int chunkZ)
        {
            return (chunkX >= 0 && chunkX < _worldSizeInChunks && chunkZ >= 0 && chunkZ < _worldSizeInChunks);
        }

        public void SetLightLevel(BlockPosition blockPosition, byte lightLevel)
        {
            // TODO optimize me
            var chunk = GetChunkFor(blockPosition);
            var relativeX = (blockPosition.X & Chunk.BitMaskX);
            var relativeZ = (blockPosition.Z & Chunk.BitMaskZ);

            chunk.SetLightLevel(relativeX, blockPosition.Y, relativeZ, lightLevel);
        }

        public Block GetBlockAt(BlockPosition blockPosition)
        {
            // TODO optimize me
            var chunk = GetChunkFor(blockPosition);

            if (chunk != null)
            {
                var relativeX = (blockPosition.X & Chunk.BitMaskX);
                var relativeZ = (blockPosition.Z & Chunk.BitMaskZ);
                var prototype = chunk.GetBlockPrototype(relativeX, blockPosition.Y, relativeZ);
                var lightLevel = chunk.GetLightLevel(relativeX, blockPosition.Y, relativeZ);

                return new Block(prototype, blockPosition, lightLevel);
            }
            else
            {
                return new Block(new VoidBlock(), blockPosition, 8);
            }
        }
    }
}
