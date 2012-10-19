using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class Tessellator
    {
        readonly World _world;

        public Tessellator(World world)
        {
            _world = world;
        }

        public void TessellateBlock(List<VertexPositionColorLighting>[] vertexLists, List<short>[] indexLists, BlockPosition worldBlockPosition, RelativeBlockPosition relativeBlockPosition)
        {
            BuildLeftQuad(vertexLists[Face.Left], indexLists[Face.Left], worldBlockPosition, relativeBlockPosition);
            BuildRightQuad(vertexLists[Face.Right], indexLists[Face.Right], worldBlockPosition, relativeBlockPosition);
            BuildFrontQuad(vertexLists[Face.Front], indexLists[Face.Front], worldBlockPosition, relativeBlockPosition);
            BuildBackQuad(vertexLists[Face.Back], indexLists[Face.Back], worldBlockPosition, relativeBlockPosition);
            BuildTopQuad(vertexLists[Face.Top], indexLists[Face.Top], worldBlockPosition, relativeBlockPosition);
            BuildBottomQuad(vertexLists[Face.Bottom], indexLists[Face.Bottom], worldBlockPosition, relativeBlockPosition);
        }

        // TODO: could maybe generalize these six methods into one once we're happy with the behavior

        // TODO: could maybe collapse these methods into one and lookup all neighboring blocks in one shot so
        // we don't duplicate lookups.  We're looking up many of these multiple times, maybe with different names.


        void BuildLeftQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, RelativeBlockPosition relativeBlockPosition)
        {
            var leftBlock = _world.GetBlockAt(worldBlockPosition.Left);
            if (!leftBlock.CanBeSeenThrough)
            {
                return;
            }

            var leftUpBlock = _world.GetBlockAt(worldBlockPosition.Left.Up);
            var leftBackBlock = _world.GetBlockAt(worldBlockPosition.Left.Back);
            var leftUpBackBlock = _world.GetBlockAt(worldBlockPosition.Left.Up.Back);
            var leftFrontBlock = _world.GetBlockAt(worldBlockPosition.Left.Front);
            var leftUpFrontBlock = _world.GetBlockAt(worldBlockPosition.Left.Up.Front);
            var leftDownBlock = _world.GetBlockAt(worldBlockPosition.Left.Down);
            var leftDownFrontBlock = _world.GetBlockAt(worldBlockPosition.Left.Down.Front);
            var leftDownBackBlock = _world.GetBlockAt(worldBlockPosition.Left.Down.Back);

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(leftBlock, leftUpBlock, leftBackBlock, leftUpBackBlock, 0.85f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(leftBlock, leftUpBlock, leftFrontBlock, leftUpFrontBlock, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(leftBlock, leftDownBlock, leftFrontBlock, leftDownFrontBlock, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(leftBlock, leftDownBlock, leftBackBlock, leftDownBackBlock, 0.85f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        void BuildRightQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, RelativeBlockPosition relativeBlockPosition)
        {
            var rightBlock = _world.GetBlockAt(worldBlockPosition.Right);
            if (!rightBlock.CanBeSeenThrough)
            {
                return;
            }

            var rightUpBlock = _world.GetBlockAt(worldBlockPosition.Right.Up);
            var rightFrontBlock = _world.GetBlockAt(worldBlockPosition.Right.Front);
            var rightUpFrontBlock = _world.GetBlockAt(worldBlockPosition.Right.Up.Front);
            var rightBackBlock = _world.GetBlockAt(worldBlockPosition.Right.Back);
            var rightUpBackBlock = _world.GetBlockAt(worldBlockPosition.Right.Up.Back);
            var rightDownBlock = _world.GetBlockAt(worldBlockPosition.Right.Down);
            var rightDownBackBlock = _world.GetBlockAt(worldBlockPosition.Right.Down.Back);
            var rightDownFrontBlock = _world.GetBlockAt(worldBlockPosition.Right.Down.Front);

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(rightBlock, rightUpBlock, rightFrontBlock, rightUpFrontBlock, 0.85f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(rightBlock, rightUpBlock, rightBackBlock, rightUpBackBlock, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(rightBlock, rightDownBlock, rightBackBlock, rightDownBackBlock, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(rightBlock, rightDownBlock, rightFrontBlock, rightDownFrontBlock, 0.85f)
            });

            indexList.Add(topRightFrontIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomRightFrontIndex);
        }

        void BuildBackQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, RelativeBlockPosition relativeBlockPosition)
        {
            var backBlock = _world.GetBlockAt(worldBlockPosition.Back);
            if (!backBlock.CanBeSeenThrough)
            {
                return;
            }

            var backUpBlock = _world.GetBlockAt(worldBlockPosition.Back.Up);
            var backRightBlock = _world.GetBlockAt(worldBlockPosition.Back.Right);
            var backUpRightBlock = _world.GetBlockAt(worldBlockPosition.Back.Up.Right);
            var backLeftBlock = _world.GetBlockAt(worldBlockPosition.Back.Left);
            var backUpLeftBlock = _world.GetBlockAt(worldBlockPosition.Back.Up.Left);
            var backDownBlock = _world.GetBlockAt(worldBlockPosition.Back.Down);
            var backDownLeftBlock = _world.GetBlockAt(worldBlockPosition.Back.Down.Left);
            var backDownRightBlock = _world.GetBlockAt(worldBlockPosition.Back.Down.Right);

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(backBlock, backUpBlock, backRightBlock, backUpRightBlock, 0.85f)
            });

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(backBlock, backUpBlock, backLeftBlock, backUpLeftBlock, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(backBlock, backDownBlock, backLeftBlock, backDownLeftBlock, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(backBlock, backDownBlock, backRightBlock, backDownRightBlock, 0.85f)
            });

            indexList.Add(topRightBackIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(bottomRightBackIndex);
        }

        void BuildFrontQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, RelativeBlockPosition relativeBlockPosition)
        {
            var frontBlock = _world.GetBlockAt(worldBlockPosition.Front);
            if (!frontBlock.CanBeSeenThrough)
            {
                return;
            }

            var frontUpBlock = _world.GetBlockAt(worldBlockPosition.Front.Up);
            var frontLeftBlock = _world.GetBlockAt(worldBlockPosition.Front.Left);
            var frontUpLeftBlock = _world.GetBlockAt(worldBlockPosition.Front.Up.Left);
            var frontRightBlock = _world.GetBlockAt(worldBlockPosition.Front.Right);
            var frontUpRightBlock = _world.GetBlockAt(worldBlockPosition.Front.Up.Right);
            var frontDownBlock = _world.GetBlockAt(worldBlockPosition.Front.Down);
            var frontDownRightBlock = _world.GetBlockAt(worldBlockPosition.Front.Down.Right);
            var frontDownLeftBlock = _world.GetBlockAt(worldBlockPosition.Front.Down.Left);

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(frontBlock, frontUpBlock, frontLeftBlock, frontUpLeftBlock, 0.85f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(frontBlock, frontUpBlock, frontRightBlock, frontUpRightBlock, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(frontBlock, frontDownBlock, frontRightBlock, frontDownRightBlock, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(frontBlock, frontDownBlock, frontLeftBlock, frontDownLeftBlock, 0.85f)
            });

            indexList.Add(topLeftFrontIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
        }

        void BuildTopQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, RelativeBlockPosition relativeBlockPosition)
        {
            var upBlock = _world.GetBlockAt(worldBlockPosition.Up);
            if (!upBlock.CanBeSeenThrough)
            {
                return;
            }

            var upLeftBlock = _world.GetBlockAt(worldBlockPosition.Up.Left);
            var upBackBlock = _world.GetBlockAt(worldBlockPosition.Up.Back);
            var upLeftBackBlock = _world.GetBlockAt(worldBlockPosition.Up.Left.Back);
            var upRightBlock = _world.GetBlockAt(worldBlockPosition.Up.Right);
            var upRightBackBlock = _world.GetBlockAt(worldBlockPosition.Up.Right.Back);
            var upFrontBlock = _world.GetBlockAt(worldBlockPosition.Up.Front);
            var upRightFrontBlock = _world.GetBlockAt(worldBlockPosition.Up.Right.Front);
            var upLeftFrontBlock = _world.GetBlockAt(worldBlockPosition.Up.Left.Front);

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(upBlock, upLeftBlock, upBackBlock, upLeftBackBlock, 1f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(upBlock, upRightBlock, upBackBlock, upRightBackBlock, 1f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(upBlock, upRightBlock, upFrontBlock, upRightFrontBlock, 1f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(upBlock, upLeftBlock, upFrontBlock, upLeftFrontBlock, 1f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
        }

        void BuildBottomQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, RelativeBlockPosition relativeBlockPosition)
        {
            var downBlock = _world.GetBlockAt(worldBlockPosition.Down);
            if (!downBlock.CanBeSeenThrough)
            {
                return;
            }

            var downLeftBlock = _world.GetBlockAt(worldBlockPosition.Down.Left);
            var downFrontBlock = _world.GetBlockAt(worldBlockPosition.Down.Front);
            var downLeftFrontBlock = _world.GetBlockAt(worldBlockPosition.Down.Left.Front);
            var downRightBlock = _world.GetBlockAt(worldBlockPosition.Down.Right);
            var downRightFrontBlock = _world.GetBlockAt(worldBlockPosition.Down.Right.Front);
            var downBackBlock = _world.GetBlockAt(worldBlockPosition.Down.Back);
            var downRightBackBlock = _world.GetBlockAt(worldBlockPosition.Down.Right.Back);
            var downLeftBackBlock = _world.GetBlockAt(worldBlockPosition.Down.Left.Back);

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(downBlock, downLeftBlock, downFrontBlock, downLeftFrontBlock, 0.70f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(downBlock, downRightBlock, downFrontBlock, downRightFrontBlock, 0.70f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(downBlock, downRightBlock, downBackBlock, downRightBackBlock, 0.70f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(downBlock, downLeftBlock, downBackBlock, downLeftBackBlock, 0.70f)
            });

            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        Vector3 AverageLightingOver(Block adjacent, Block edge1, Block edge2, Block diagonal, float limit)
        {
            // For each vertex we examine four voxels grouped around the vertex in the plane of the face that the vertex belongs to.
            // The voxels we're interested in for a particular vertex are:
            //   The voxel adjacent to the face we're calculating
            //   One voxel along the edge of the face we're calculating
            //   The other voxel along the edge of the face we're calculating 
            //   The voxel diagonal to the face we're calculating

            float average;
            if (!edge1.CanPropagateLight && !edge2.CanPropagateLight)
            {
                // If the two edge voxels are not transparent then light can't get from the diagonal to the vertex we're calculating
                // so we don't include it in the average
                average = (adjacent.LightLevel + edge1.LightLevel + edge2.LightLevel) / 3f;
            }
            else
            {
                average = (adjacent.LightLevel + edge1.LightLevel + edge2.LightLevel + diagonal.LightLevel) / 4f;
            }

            var percentage = Math.Min(average / World.MaximumLightLevel, limit);

            return new Vector3(percentage);
        }
    }
}
