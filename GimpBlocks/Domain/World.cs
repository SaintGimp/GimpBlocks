using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class World
        : IListener<PlaceBlock>,
        IListener<DestroyBlock>
    {
        readonly IWorldRenderer _renderer;
        readonly BlockArray _blockArray;
        readonly LightArray _lightArray;
        readonly BlockPrototypeMap _prototypeMap;
        readonly BlockPicker _blockPicker;

        public World(IWorldRenderer renderer, BlockArray blockArray, LightArray lightArray, BlockPrototypeMap prototypeMap, BlockPicker blockPicker)
        {
            _renderer = renderer;
            _blockArray = blockArray;
            _lightArray = lightArray;
            _prototypeMap = prototypeMap;
            _blockPicker = blockPicker;
        }

        public void Generate()
        {
            GenerateRandom();

            GenerateSlab();

            Rebuild();
        }

        void GenerateSlab()
        {
            for (int x = 1; x < _blockArray.XDimension - 1; x++)
            {
                for (int z = 1; z < _blockArray.ZDimension - 1; z++)
                {
                    _blockArray[x, 1, z] = _prototypeMap[1];
                }
            }
        }

        void GenerateRandom()
        {
            var random = new Random(123);

            _blockArray.Initialize((x, y, z) =>
            {
                if (x > 0 && x < 15 && y > 0 && y < 15 && z > 0 && z < 15)
                {
                    return random.Next(4) > 0 ? _prototypeMap[0] : _prototypeMap[1];
                }
                else
                {
                    return _prototypeMap[0];
                }
            });
        }

        void Rebuild()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _lightArray.Calculate();

            var vertexList = new List<VertexPositionColorLighting>();
            var indexList = new List<short>();

            _blockArray.ForEach(block =>
            {
                if (block.Prototype.IsSolid)
                {
                    BuildQuads(vertexList, indexList, block.Position);
                }
            });

            _renderer.Initialize(vertexList, indexList);

            stopWatch.Stop();
            Debug.WriteLine("Chunk rebuild time: " + stopWatch.ElapsedMilliseconds + " ms");

            EventAggregator.Instance.SendMessage(new ChunkRebuilt());
        }

        void BuildQuads(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            BuildLeftQuad(vertexList, indexList, blockPosition);
            BuildRightQuad(vertexList, indexList, blockPosition);
            BuildFrontQuad(vertexList, indexList, blockPosition);
            BuildBackQuad(vertexList, indexList, blockPosition);
            BuildTopQuad(vertexList, indexList, blockPosition);
            BuildBottomQuad(vertexList, indexList, blockPosition);
        }

        void BuildLeftQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Left].IsSolid)
            {
                return;
            }

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Left, blockPosition.Left.Up, blockPosition.Left.Back, blockPosition.Left.Up.Back, 0.85f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Left, blockPosition.Left.Up, blockPosition.Left.Front, blockPosition.Left.Up.Front, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Left, blockPosition.Left.Down, blockPosition.Left.Front, blockPosition.Left.Down.Front, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Left, blockPosition.Left.Down, blockPosition.Left.Back, blockPosition.Left.Down.Back, 0.85f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        void BuildRightQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Right].IsSolid)
            {
                return;
            }

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Right, blockPosition.Right.Up, blockPosition.Right.Front, blockPosition.Right.Up.Front, 0.85f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Right, blockPosition.Right.Up, blockPosition.Right.Back, blockPosition.Right.Up.Back, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Right, blockPosition.Right.Down, blockPosition.Right.Back, blockPosition.Right.Down.Back, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Right, blockPosition.Right.Down, blockPosition.Right.Front, blockPosition.Right.Down.Front, 0.85f)
            });

            indexList.Add(topRightFrontIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomRightFrontIndex);
        }

        void BuildBackQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Back].IsSolid)
            {
                return;
            }

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Back, blockPosition.Back.Up, blockPosition.Back.Right, blockPosition.Back.Up.Right, 0.85f)
            });

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Back, blockPosition.Back.Up, blockPosition.Back.Left, blockPosition.Back.Up.Left, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Back, blockPosition.Back.Down, blockPosition.Back.Left, blockPosition.Back.Down.Left, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Back, blockPosition.Back.Down, blockPosition.Back.Right, blockPosition.Back.Down.Right, 0.85f)
            });

            indexList.Add(topRightBackIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(bottomRightBackIndex);
        }

        void BuildFrontQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Front].IsSolid)
            {
                return;
            }

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Front, blockPosition.Front.Up, blockPosition.Front.Left, blockPosition.Front.Up.Left, 0.85f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Front, blockPosition.Front.Up, blockPosition.Front.Right, blockPosition.Front.Up.Right, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Front, blockPosition.Front.Down, blockPosition.Front.Right, blockPosition.Front.Down.Right, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Front, blockPosition.Front.Down, blockPosition.Front.Left, blockPosition.Front.Down.Left, 0.85f)
            });

            indexList.Add(topLeftFrontIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
        }

        void BuildTopQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Up].IsSolid)
            {
                return;
            }

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Up, blockPosition.Up.Left, blockPosition.Up.Back, blockPosition.Up.Left.Back, 1f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Up, blockPosition.Up.Right, blockPosition.Up.Back, blockPosition.Up.Right.Back, 1f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Up, blockPosition.Up.Right, blockPosition.Up.Front, blockPosition.Up.Right.Front, 1f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Up, blockPosition.Up.Left, blockPosition.Up.Front, blockPosition.Up.Left.Front, 1f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
        }

        void BuildBottomQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Down].IsSolid)
            {
                return;
            }

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Down, blockPosition.Down.Left, blockPosition.Down.Front, blockPosition.Down.Left.Front, 0.70f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Down, blockPosition.Down.Right, blockPosition.Down.Front, blockPosition.Down.Right.Front, 0.70f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Down, blockPosition.Down.Right, blockPosition.Down.Back, blockPosition.Down.Right.Back, 0.70f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Down, blockPosition.Down.Left, blockPosition.Down.Back, blockPosition.Down.Left.Back, 0.70f)
            });

            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        Vector3 AverageLightingOver(BlockPosition adjacent, BlockPosition edge1, BlockPosition edge2, BlockPosition diagonal, float limit)
        {
            // For each vertex we examine four voxels grouped around the vertex in the plane of the face that the vertex belongs to.
            // The voxels we're interested in for a particular vertex are:
            //   The voxel adjacent to the face we're calculating
            //   One voxel along the edge of the face we're calculating
            //   The other voxel along the edge of the face we're calculating 
            //   The voxel diagonal to the face we're calculating

            float average;
            if (_blockArray[edge1].IsSolid && _blockArray[edge2].IsSolid)
            {
                // If the two edge voxels are solid then light can't get from the diagonal to the vertex we're calculating
                // so we don't include it in the average
                average = (_lightArray[adjacent] + _lightArray[edge1] + _lightArray[edge2]) / 3f;
            }
            else
            {
                average = (_lightArray[adjacent] + _lightArray[edge1] + _lightArray[edge2] + _lightArray[diagonal]) / 4f;
            }

            var percentage = Math.Min(average / _lightArray.MaximumLightLevel, limit);

            return new Vector3(percentage);
        }

        public void Handle(PlaceBlock message)
        {
            if (_blockPicker.SelectedBlock != null)
            {
                _blockArray[_blockPicker.SelectedPlacePosition] = _prototypeMap[1];

                Rebuild();
            }
        }

        public void Handle(DestroyBlock message)
        {
            if (_blockPicker.SelectedBlock != null)
            {
                _blockArray[_blockPicker.SelectedBlock.Position] = _prototypeMap[0];

                Rebuild();
            }
        }
    }
}
