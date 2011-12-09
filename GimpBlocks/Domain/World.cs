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
            BuildLeftQuad(vertexList, indexList, x, y, z);
            BuildRightQuad(vertexList, indexList, x, y, z);
            BuildFrontQuad(vertexList, indexList, x, y, z);
            BuildBackQuad(vertexList, indexList, x, y, z);
            BuildTopQuad(vertexList, indexList, x, y, z);
            BuildBottomQuad(vertexList, indexList, x, y, z);
        }

        void BuildLeftQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, int x, int y, int z)
        {
            if (_blockBuffer.LeftNeighbor(x, y, z).IsSolid)
            {
                return;
            }

            // TODO: should average only three blocks that affect each vertex, not four
            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y + 1, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.LeftNeighbor(x, y, z), _lightBuffer.UpLeftNeighbor(x, y, z), _lightBuffer.LeftBackNeighbor(x, y, z), 0.85f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y + 1, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.LeftNeighbor(x, y, z), _lightBuffer.UpLeftNeighbor(x, y, z), _lightBuffer.LeftFrontNeighbor(x, y, z), 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.LeftNeighbor(x, y, z), _lightBuffer.DownLeftNeighbor(x, y, z), _lightBuffer.LeftFrontNeighbor(x, y, z), 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.LeftNeighbor(x, y, z), _lightBuffer.DownLeftNeighbor(x, y, z), _lightBuffer.LeftBackNeighbor(x, y, z), 0.85f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        void BuildRightQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, int x, int y, int z)
        {
            if (_blockBuffer.RightNeighbor(x, y, z).IsSolid)
            {
                return;
            }

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y + 1, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.RightNeighbor(x, y, z), _lightBuffer.UpRightNeighbor(x, y, z), _lightBuffer.RightBackNeighbor(x, y, z), 0.85f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y + 1, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.RightNeighbor(x, y, z), _lightBuffer.UpRightNeighbor(x, y, z), _lightBuffer.RightFrontNeighbor(x, y, z), 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.RightNeighbor(x, y, z), _lightBuffer.DownRightNeighbor(x, y, z), _lightBuffer.RightBackNeighbor(x, y, z), 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.RightNeighbor(x, y, z), _lightBuffer.DownRightNeighbor(x, y, z), _lightBuffer.RightFrontNeighbor(x, y, z), 0.85f)
            });

            indexList.Add(topRightFrontIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomRightFrontIndex);
        }

        void BuildBackQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, int x, int y, int z)
        {
            if (_blockBuffer.BackNeighbor(x, y, z).IsSolid)
            {
                return;
            }

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.BackNeighbor(x, y, z), _lightBuffer.DownBackNeighbor(x, y, z), _lightBuffer.LeftBackNeighbor(x, y, z), 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.BackNeighbor(x, y, z), _lightBuffer.DownBackNeighbor(x, y, z), _lightBuffer.RightBackNeighbor(x, y, z), 0.85f)
            });

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y + 1, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.BackNeighbor(x, y, z), _lightBuffer.UpBackNeighbor(x, y, z), _lightBuffer.LeftBackNeighbor(x, y, z), 0.85f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y + 1, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.BackNeighbor(x, y, z), _lightBuffer.UpBackNeighbor(x, y, z), _lightBuffer.RightBackNeighbor(x, y, z), 0.85f)
            });

            indexList.Add(topRightBackIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(bottomRightBackIndex);
        }

        void BuildFrontQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, int x, int y, int z)
        {
            if (_blockBuffer.FrontNeighbor(x, y, z).IsSolid)
            {
                return;
            }

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.FrontNeighbor(x, y, z), _lightBuffer.DownFrontNeighbor(x, y, z), _lightBuffer.RightFrontNeighbor(x, y, z), 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.FrontNeighbor(x, y, z), _lightBuffer.DownFrontNeighbor(x, y, z), _lightBuffer.LeftFrontNeighbor(x, y, z), 0.85f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y + 1, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.FrontNeighbor(x, y, z), _lightBuffer.UpFrontNeighbor(x, y, z), _lightBuffer.RightFrontNeighbor(x, y, z), 0.85f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y + 1, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.FrontNeighbor(x, y, z), _lightBuffer.UpFrontNeighbor(x, y, z), _lightBuffer.LeftFrontNeighbor(x, y, z), 0.85f)
            });

            indexList.Add(topLeftFrontIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
        }

        void BuildTopQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, int x, int y, int z)
        {
            if (_blockBuffer.UpNeighbor(x, y, z).IsSolid)
            {
                return;
            }

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y + 1, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.UpNeighbor(x, y, z), _lightBuffer.UpLeftNeighbor(x, y, z), _lightBuffer.UpBackNeighbor(x, y, z), 1f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y + 1, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.UpNeighbor(x, y, z), _lightBuffer.UpRightNeighbor(x, y, z), _lightBuffer.UpBackNeighbor(x, y, z), 1f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y + 1, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.UpNeighbor(x, y, z), _lightBuffer.UpLeftNeighbor(x, y, z), _lightBuffer.UpFrontNeighbor(x, y, z), 1f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y + 1, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.UpNeighbor(x, y, z), _lightBuffer.UpRightNeighbor(x, y, z), _lightBuffer.UpFrontNeighbor(x, y, z), 1f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
        }

        void BuildBottomQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, int x, int y, int z)
        {
            if (_blockBuffer.DownNeighbor(x, y, z).IsSolid)
            {
                return;
            }

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.DownNeighbor(x, y, z), _lightBuffer.DownLeftNeighbor(x, y, z), _lightBuffer.DownBackNeighbor(x, y, z), 0.70f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y, z),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.DownNeighbor(x, y, z), _lightBuffer.DownRightNeighbor(x, y, z), _lightBuffer.DownBackNeighbor(x, y, z), 0.70f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x, y, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.DownNeighbor(x, y, z), _lightBuffer.DownLeftNeighbor(x, y, z), _lightBuffer.DownFrontNeighbor(x, y, z), 0.70f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = new Vector3(x + 1, y, z + 1),
                Color = Color.LightGray,
                Lighting = AverageLightingFor(_lightBuffer.DownNeighbor(x, y, z), _lightBuffer.DownRightNeighbor(x, y, z), _lightBuffer.DownFrontNeighbor(x, y, z), 0.70f)
            });

            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        Vector3 AverageLightingFor(int light1, int light2, int light3, float limit)
        {
            var average = (light1 + light2 + light3) / 3f;
            var percentage = Math.Min(average / _lightBuffer.MaximumLightLevel, limit);

            return new Vector3(percentage);
        }
    }
}
