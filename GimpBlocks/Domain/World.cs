using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class World
    {
        readonly IWorldRenderer _renderer;
        readonly BlockBuffer _blockBuffer;
        readonly LightBuffer _lightBuffer;

        public World(IWorldRenderer renderer)
        {
            _renderer = renderer;
            _blockBuffer = new BlockBuffer(16, 16, 16);
            _lightBuffer = new LightBuffer(16, 16, 16, _blockBuffer);
        }

        public void Generate()
        {

            var random = new Random(123);

            _blockBuffer.Initialize((x, y, z) =>
            {
                if (x > 0 && x < 15 && y > 0 && y < 15 && z > 0 && z < 15)
                {
                    return new Block()
                    {
                        Type = random.Next(4) > 0 ? BlockType.Air : BlockType.Stone
                    };
                }
                else
                {
                    return new Block()
                    {
                        Type = BlockType.Air
                    };
                }
            });

            _lightBuffer.Calculate();

            var vertexList = new List<VertexPositionColorLighting>();
            var indexList = new List<short>();

            _blockBuffer.ForEach((block, x, y, z) =>
            {
                if (block.IsSolid)
                {
                    BuildQuads(vertexList, indexList, x, y, z);
                }
            });

            _renderer.Initialize(vertexList, indexList);
        }

        void BuildQuads(List<VertexPositionColorLighting> vertexList, List<short> indexList, int x, int y, int z)
        {
            var block = new BufferLocation(x, y, z);
            BuildLeftQuad(vertexList, indexList, block);
            BuildRightQuad(vertexList, indexList, block);
            BuildFrontQuad(vertexList, indexList, block);
            BuildBackQuad(vertexList, indexList, block);
            BuildTopQuad(vertexList, indexList, block);
            BuildBottomQuad(vertexList, indexList, block);
        }

        void BuildLeftQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BufferLocation block)
        {
            if (_blockBuffer[block.Left].IsSolid)
            {
                return;
            }

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Left, block.Left.Up, block.Left.Back, block.Left.Up.Back, 0.85f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Left, block.Left.Up, block.Left.Front, block.Left.Up.Front, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Left, block.Left.Down, block.Left.Front, block.Left.Down.Front, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Left, block.Left.Down, block.Left.Back, block.Left.Down.Back, 0.85f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        void BuildRightQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BufferLocation block)
        {
            if (_blockBuffer[block.Right].IsSolid)
            {
                return;
            }

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Up.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Right, block.Right.Up, block.Right.Front, block.Right.Up.Front, 0.85f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Up.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Right, block.Right.Up, block.Right.Back, block.Right.Up.Back, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Right, block.Right.Down, block.Right.Back, block.Right.Down.Back, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Right, block.Right.Down, block.Right.Front, block.Right.Down.Front, 0.85f)
            });

            indexList.Add(topRightFrontIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomRightFrontIndex);
        }

        void BuildBackQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BufferLocation block)
        {
            if (_blockBuffer[block.Back].IsSolid)
            {
                return;
            }

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Back, block.Back.Up, block.Back.Right, block.Back.Up.Right, 0.85f)
            });

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Back, block.Back.Up, block.Back.Left, block.Back.Up.Left, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Back, block.Back.Down, block.Back.Left, block.Back.Down.Left, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Back, block.Back.Down, block.Back.Right, block.Back.Down.Right, 0.85f)
            });

            indexList.Add(topRightBackIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(bottomRightBackIndex);
        }

        void BuildFrontQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BufferLocation block)
        {
            if (_blockBuffer[block.Front].IsSolid)
            {
                return;
            }

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Front, block.Front.Up, block.Front.Left, block.Front.Up.Left, 0.85f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Front, block.Front.Up, block.Front.Right, block.Front.Up.Right, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Front, block.Front.Down, block.Front.Right, block.Front.Down.Right, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Front, block.Front.Down, block.Front.Left, block.Front.Down.Left, 0.85f)
            });

            indexList.Add(topLeftFrontIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
        }

        void BuildTopQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BufferLocation block)
        {
            if (_blockBuffer[block.Up].IsSolid)
            {
                return;
            }

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Up, block.Up.Left, block.Up.Back, block.Up.Left.Back, 1f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Up, block.Up.Right, block.Up.Back, block.Up.Right.Back, 1f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Up, block.Up.Right, block.Up.Front, block.Up.Right.Front, 1f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Up, block.Up.Left, block.Up.Front, block.Up.Left.Front, 1f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
        }

        void BuildBottomQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BufferLocation block)
        {
            if (_blockBuffer[block.Down].IsSolid)
            {
                return;
            }

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Down, block.Down.Left, block.Down.Front, block.Down.Left.Front, 0.70f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Down, block.Down.Right, block.Down.Front, block.Down.Right.Front, 0.70f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Down, block.Down.Right, block.Down.Back, block.Down.Right.Back, 0.70f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = block,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(block.Down, block.Down.Left, block.Down.Back, block.Down.Left.Back, 0.70f)
            });

            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        Vector3 AverageLightingOver(BufferLocation adjacent, BufferLocation edge1, BufferLocation edge2, BufferLocation diagonal, float limit)
        {
            // For each vertex we examine four voxels grouped around the vertex in the plane of the face that the vertex belongs to.
            // The voxels we're interested in for a particular vertex are:
            //   The voxel adjacent to the face we're calculating
            //   One voxel along the edge of the face we're calculating
            //   The other voxel along the edge of the face we're calculating 
            //   The voxel diagonal to the face we're calculating

            float average;
            if (_blockBuffer[edge1].IsSolid && _blockBuffer[edge2].IsSolid)
            {
                // If the two edge voxels are solid then light can't get from the diagonal to the vertex we're calculating
                // so we don't include it in the average
                average = (_lightBuffer[adjacent] + _lightBuffer[edge1] + _lightBuffer[edge2]) / 3f;
            }
            else
            {
                average = (_lightBuffer[adjacent] + _lightBuffer[edge1] + _lightBuffer[edge2] + _lightBuffer[diagonal]) / 4f;
            }

            var percentage = Math.Min(average / _lightBuffer.MaximumLightLevel, limit);

            return new Vector3(percentage);
        }
    }
}
