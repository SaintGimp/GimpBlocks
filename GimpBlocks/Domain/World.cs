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
        readonly int _worldSizeInChunks = 10;

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

        public bool CanPropagateLight(BlockPosition blockPosition)
        {
            return GetBlockPrototype(blockPosition).CanPropagateLight;
        }

        public bool CanBeSeenThrough(BlockPosition blockPosition)
        {
            return GetBlockPrototype(blockPosition).CanBeSeenThrough;
        }

        public bool CanBeSelected(BlockPosition blockPosition)
        {
            return GetBlockPrototype(blockPosition).CanBeSelected;
        }

        public byte GetLightLevel(BlockPosition blockPosition)
        {
            // TODO optimize me
            var chunk = GetChunkFor(blockPosition);

            if (chunk != null)
            {
                var relativeX = blockPosition.X >= 0 ? blockPosition.X % Chunk.XDimension : Chunk.XDimension - blockPosition.X % Chunk.XDimension;
                var relativeZ = blockPosition.Z >= 0 ? blockPosition.Z % Chunk.ZDimension : Chunk.ZDimension - blockPosition.Z % Chunk.ZDimension;
                return chunk.GetLightLevel(relativeX, blockPosition.Y, relativeZ);
            }
            else
            {
                return 8;
            }
        }

        Chunk GetChunkFor(BlockPosition blockPosition)
        {
            // TODO: optimize
            if (blockPosition.Y < 0 || blockPosition.Y >= Chunk.YDimension)
            {
                return null;
            }

            int chunkX;
            if (blockPosition.X >= 0)
            {
                chunkX = blockPosition.X / Chunk.XDimension;
            }
            else
            {
                chunkX = blockPosition.X / Chunk.XDimension - 1;
            }

            int chunkZ;
            if (blockPosition.Z >= 0)
            {
                chunkZ = blockPosition.Z / Chunk.ZDimension;
            }
            else
            {
                chunkZ = blockPosition.Z / Chunk.ZDimension - 1;
            }

            if (IsLoaded(chunkX, chunkZ))
            {
                return _chunks[chunkX, chunkZ];
            }
            else
            {
                return null;
            }
        }

        bool IsLoaded(int chunkX, int chunkZ)
        {
            return (chunkX >= 0 && chunkX < _worldSizeInChunks && chunkZ >= 0 && chunkZ < _worldSizeInChunks);
        }

        public void SetLightLevel(BlockPosition blockPosition, byte lightLevel)
        {
            // TODO optimize me
            var chunk = GetChunkFor(blockPosition);
            var relativeX = blockPosition.X >= 0 ? blockPosition.X % Chunk.XDimension : Chunk.XDimension - blockPosition.X % Chunk.XDimension;
            var relativeZ = blockPosition.Z >= 0 ? blockPosition.Z % Chunk.ZDimension : Chunk.ZDimension - blockPosition.Z % Chunk.ZDimension;

            chunk.SetLightLevel(relativeX, blockPosition.Y, relativeZ, lightLevel);
        }

        BlockPrototype GetBlockPrototype(BlockPosition blockPosition)
        {
            // TODO optimize me
            var chunk = GetChunkFor(blockPosition);

            if (chunk != null)
            {
                var relativeX = blockPosition.X >= 0 ? blockPosition.X % Chunk.XDimension : Chunk.XDimension - blockPosition.X % Chunk.XDimension;
                var relativeZ = blockPosition.Z >= 0 ? blockPosition.Z % Chunk.ZDimension : Chunk.ZDimension - blockPosition.Z % Chunk.ZDimension;
                return chunk.GetBlockPrototype(relativeX, blockPosition.Y, relativeZ);
            }
            else
            {
                return new VoidBlock();
            }
        }
    }
}
