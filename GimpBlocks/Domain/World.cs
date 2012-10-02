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
        readonly Func<int, int, Chunk> _chunkFactory;
        readonly BlockPrototypeMap _prototypeMap;
        readonly BlockPicker _blockPicker;
        readonly BoundingBoxRenderer _boundingBoxRenderer;
        Chunk[,] _chunks = new Chunk[4,4];

        public World(IWorldRenderer renderer, Func<int, int, Chunk> chunkFactory, BlockPrototypeMap prototypeMap, BlockPicker blockPicker, BoundingBoxRenderer boundingBoxRenderer)
        {
            _renderer = renderer;
            _chunkFactory = chunkFactory;
            _prototypeMap = prototypeMap;
            _blockPicker = blockPicker;
            _boundingBoxRenderer = boundingBoxRenderer;
        }

        public void Generate()
        {
            for (int x = 0; x <= _chunks.GetUpperBound(0); x++)
            {
                for (int z = 0; z <= _chunks.GetUpperBound(1); z++)
                {
                    var chunk = _chunkFactory(x, z);
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
                chunk.CalculateInternalLighting();
                chunk.BuildGeometry();
            }
            
            EventAggregator.Instance.SendMessage(new ChunkRebuilt());
        }


        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            _renderer.Draw(cameraLocation, originBasedViewMatrix, projectionMatrix);

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
            //    _chunk.SetBlock(_blockPicker.SelectedPlacePosition, _prototypeMap[1]);

            //    Rebuild();
            //}
        }

        public void Handle(DestroyBlock message)
        {
            //if (_blockPicker.SelectedBlock != null)
            //{
            //    _chunk.SetBlock(_blockPicker.SelectedPlacePosition, _prototypeMap[0]);

            //    Rebuild();
            //}
        }
    }
}
