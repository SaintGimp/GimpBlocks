using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class Tessellator
    {
        readonly World world;

        Block leftBlock;
        Block leftUpBlock;
        Block leftBackBlock;
        Block leftUpBackBlock;
        Block leftFrontBlock;
        Block leftUpFrontBlock;
        Block leftDownBlock;
        Block leftDownFrontBlock;
        Block leftDownBackBlock;

        Block rightBlock;
        Block rightUpBlock;
        Block rightFrontBlock;
        Block rightUpFrontBlock;
        Block rightBackBlock;
        Block rightUpBackBlock;
        Block rightDownBlock;
        Block rightDownBackBlock;
        Block rightDownFrontBlock;

        Block backBlock;
        Block backUpBlock;
        Block backRightBlock;
        Block backUpRightBlock;
        Block backLeftBlock;
        Block backUpLeftBlock;
        Block backDownBlock;
        Block backDownLeftBlock;
        Block backDownRightBlock;

        Block frontBlock;
        Block frontUpBlock;
        Block frontLeftBlock;
        Block frontUpLeftBlock;
        Block frontRightBlock;
        Block frontUpRightBlock;
        Block frontDownBlock;
        Block frontDownRightBlock;
        Block frontDownLeftBlock;

        Block upBlock;
        Block upLeftBlock;
        Block upBackBlock;
        Block upLeftBackBlock;
        Block upRightBlock;
        Block upRightBackBlock;
        Block upFrontBlock;
        Block upRightFrontBlock;
        Block upLeftFrontBlock;

        Block downBlock;
        Block downLeftBlock;
        Block downFrontBlock;
        Block downLeftFrontBlock;
        Block downRightBlock;
        Block downRightFrontBlock;
        Block downBackBlock;
        Block downRightBackBlock;
        Block downLeftBackBlock;

        public static int NumberOfBlocksTessellated;

        public Tessellator(World world)
        {
            this.world = world;
        }

        public void TessellateBlock(List<VertexPositionColorLighting>[] vertexLists, List<short>[] indexLists, BlockPosition worldBlockPosition, RelativeBlockPosition relativeBlockPosition)
        {
            GetRequiredBlocks(worldBlockPosition);
            BuildLeftQuad(vertexLists[Face.Left], indexLists[Face.Left], relativeBlockPosition);
            BuildRightQuad(vertexLists[Face.Right], indexLists[Face.Right], relativeBlockPosition);
            BuildFrontQuad(vertexLists[Face.Front], indexLists[Face.Front], relativeBlockPosition);
            BuildBackQuad(vertexLists[Face.Back], indexLists[Face.Back], relativeBlockPosition);
            BuildTopQuad(vertexLists[Face.Top], indexLists[Face.Top], relativeBlockPosition);
            BuildBottomQuad(vertexLists[Face.Bottom], indexLists[Face.Bottom], relativeBlockPosition);

            NumberOfBlocksTessellated++;
        }

        void GetRequiredBlocks(BlockPosition worldBlockPosition)
        {
            // We're putting in some work here to try to avoid loading the same blocks
            // multiple times for various faces of the block we're working on. This
            // is only useful for blocks where we're drawing multiple faces, which isn't
            // many of them relatively speaking. It's just barely worth the complexity.

            // TODO: depending on how things work out, it might be better to just do a
            // marching load of blocks along the z axis. That would be 9 blocks loaded
            // per tessellation, and we're currently doing ~7, but that could change as
            // we mess with the terrain and algorithms.

            // TODO: We can save a little time by getting the world
            // positions by relative arithmetic rather than direction chains.

            upBlock = world.GetBlockAt(worldBlockPosition.Up);
            if (upBlock.CanBeSeenThrough)
            {
                upFrontBlock = world.GetBlockAt(worldBlockPosition.Up.Front);
                upBackBlock = world.GetBlockAt(worldBlockPosition.Up.Back);
                upLeftFrontBlock = world.GetBlockAt(worldBlockPosition.Up.Left.Front);
                upLeftBlock = world.GetBlockAt(worldBlockPosition.Up.Left);
                upLeftBackBlock = world.GetBlockAt(worldBlockPosition.Up.Left.Back);
                upRightFrontBlock = world.GetBlockAt(worldBlockPosition.Up.Right.Front);
                upRightBlock = world.GetBlockAt(worldBlockPosition.Up.Right);
                upRightBackBlock = world.GetBlockAt(worldBlockPosition.Up.Right.Back);
            }
            else
            {
                upLeftBlock = null;
                upBackBlock = null;
                upLeftBackBlock = null;
                upRightBlock = null;
                upRightBackBlock = null;
                upFrontBlock = null;
                upRightFrontBlock = null;
                upLeftFrontBlock = null;
            }

            leftBlock = world.GetBlockAt(worldBlockPosition.Left);
            if (leftBlock.CanBeSeenThrough)
            {
                leftFrontBlock = world.GetBlockAt(worldBlockPosition.Left.Front);
                leftBackBlock = world.GetBlockAt(worldBlockPosition.Left.Back);
                leftUpFrontBlock = upLeftFrontBlock ?? world.GetBlockAt(worldBlockPosition.Left.Up.Front);
                leftUpBlock = upLeftBlock ?? world.GetBlockAt(worldBlockPosition.Left.Up);
                leftUpBackBlock = upLeftBackBlock ?? world.GetBlockAt(worldBlockPosition.Left.Up.Back);
                leftDownFrontBlock = world.GetBlockAt(worldBlockPosition.Left.Down.Front);
                leftDownBlock = world.GetBlockAt(worldBlockPosition.Left.Down);
                leftDownBackBlock = world.GetBlockAt(worldBlockPosition.Left.Down.Back);
            }
            else
            {
                leftUpBlock = null;
                leftBackBlock = null;
                leftUpBackBlock = null;
                leftFrontBlock = null;
                leftUpFrontBlock = null;
                leftDownBlock = null;
                leftDownFrontBlock = null;
                leftDownBackBlock = null;
            }

            rightBlock = world.GetBlockAt(worldBlockPosition.Right);
            if (rightBlock.CanBeSeenThrough)
            {
                rightFrontBlock = world.GetBlockAt(worldBlockPosition.Right.Front);
                rightBackBlock = world.GetBlockAt(worldBlockPosition.Right.Back);
                rightUpFrontBlock = upRightFrontBlock ?? world.GetBlockAt(worldBlockPosition.Right.Up.Front);
                rightUpBlock = upRightBlock ?? world.GetBlockAt(worldBlockPosition.Right.Up);
                rightUpBackBlock = upRightBackBlock ?? world.GetBlockAt(worldBlockPosition.Right.Up.Back);
                rightDownFrontBlock = world.GetBlockAt(worldBlockPosition.Right.Down.Front);
                rightDownBlock = world.GetBlockAt(worldBlockPosition.Right.Down);
                rightDownBackBlock = world.GetBlockAt(worldBlockPosition.Right.Down.Back);
            }
            else
            {
                rightUpBlock = null;
                rightFrontBlock = null;
                rightUpFrontBlock = null;
                rightBackBlock = null;
                rightUpBackBlock = null;
                rightDownBlock = null;
                rightDownBackBlock = null;
                rightDownFrontBlock = null;
            }

            backBlock = world.GetBlockAt(worldBlockPosition.Back);
            if (backBlock.CanBeSeenThrough)
            {
                backLeftBlock = leftBackBlock ?? world.GetBlockAt(worldBlockPosition.Back.Left);
                backRightBlock = rightBackBlock ?? world.GetBlockAt(worldBlockPosition.Back.Right);
                backUpLeftBlock = upLeftBackBlock ?? leftUpBackBlock ?? world.GetBlockAt(worldBlockPosition.Back.Up.Left);
                backUpBlock = upBackBlock ?? world.GetBlockAt(worldBlockPosition.Back.Up);
                backUpRightBlock = upRightBackBlock ?? rightUpBackBlock ?? world.GetBlockAt(worldBlockPosition.Back.Up.Right);
                backDownLeftBlock = leftDownBackBlock ?? world.GetBlockAt(worldBlockPosition.Back.Down.Left);
                backDownBlock = world.GetBlockAt(worldBlockPosition.Back.Down);
                backDownRightBlock = rightDownBackBlock ?? world.GetBlockAt(worldBlockPosition.Back.Down.Right);
            }
            else
            {
                backUpBlock = null;
                backRightBlock = null;
                backUpRightBlock = null;
                backLeftBlock = null;
                backUpLeftBlock = null;
                backDownBlock = null;
                backDownLeftBlock = null;
                backDownRightBlock = null;
            }

            frontBlock = world.GetBlockAt(worldBlockPosition.Front);
            if (frontBlock.CanBeSeenThrough)
            {
                frontLeftBlock = leftFrontBlock ?? world.GetBlockAt(worldBlockPosition.Front.Left);
                frontRightBlock = rightFrontBlock ?? world.GetBlockAt(worldBlockPosition.Front.Right);
                frontUpLeftBlock = upLeftFrontBlock ?? leftUpFrontBlock ?? world.GetBlockAt(worldBlockPosition.Front.Up.Left);
                frontUpBlock = upFrontBlock ?? world.GetBlockAt(worldBlockPosition.Front.Up);
                frontUpRightBlock = upRightFrontBlock ?? rightUpFrontBlock ?? world.GetBlockAt(worldBlockPosition.Front.Up.Right);
                frontDownLeftBlock = leftDownFrontBlock ?? world.GetBlockAt(worldBlockPosition.Front.Down.Left);
                frontDownBlock = world.GetBlockAt(worldBlockPosition.Front.Down);
                frontDownRightBlock = rightDownFrontBlock ?? world.GetBlockAt(worldBlockPosition.Front.Down.Right);
            }
            else
            {
                frontUpBlock = null;
                frontLeftBlock = null;
                frontUpLeftBlock = null;
                frontRightBlock = null;
                frontUpRightBlock = null;
                frontDownBlock = null;
                frontDownRightBlock = null;
                frontDownLeftBlock = null;
            }

            downBlock = world.GetBlockAt(worldBlockPosition.Down);
            if (downBlock.CanBeSeenThrough)
            {
                downFrontBlock = frontDownBlock ?? world.GetBlockAt(worldBlockPosition.Down.Front);
                downBackBlock = backDownBlock ?? world.GetBlockAt(worldBlockPosition.Down.Back);
                downLeftFrontBlock = leftDownFrontBlock ?? frontDownLeftBlock ?? world.GetBlockAt(worldBlockPosition.Down.Left.Front);
                downLeftBlock = leftDownBlock ?? world.GetBlockAt(worldBlockPosition.Down.Left);
                downLeftBackBlock = leftDownBackBlock ?? backDownLeftBlock ?? world.GetBlockAt(worldBlockPosition.Down.Left.Back);
                downRightFrontBlock = rightDownFrontBlock ?? frontDownRightBlock ?? world.GetBlockAt(worldBlockPosition.Down.Right.Front);
                downRightBlock = rightDownBlock ?? world.GetBlockAt(worldBlockPosition.Down.Right);
                downRightBackBlock = rightDownBackBlock ?? backDownRightBlock ?? world.GetBlockAt(worldBlockPosition.Down.Right.Back);
            }
            // Don't need to null these out because they're not used anywhere else
        }

        // TODO: could maybe generalize these six methods into one once we're happy with the behavior

        void BuildLeftQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, RelativeBlockPosition relativeBlockPosition)
        {
            if (!leftBlock.CanBeSeenThrough)
            {
                return;
            }

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

        void BuildRightQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, RelativeBlockPosition relativeBlockPosition)
        {
            if (!rightBlock.CanBeSeenThrough)
            {
                return;
            }

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

        void BuildBackQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, RelativeBlockPosition relativeBlockPosition)
        {
            if (!backBlock.CanBeSeenThrough)
            {
                return;
            }

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

        void BuildFrontQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, RelativeBlockPosition relativeBlockPosition)
        {
            if (!frontBlock.CanBeSeenThrough)
            {
                return;
            }

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

        void BuildTopQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, RelativeBlockPosition relativeBlockPosition)
        {
            if (!upBlock.CanBeSeenThrough)
            {
                return;
            }

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

        void BuildBottomQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, RelativeBlockPosition relativeBlockPosition)
        {
            if (!downBlock.CanBeSeenThrough)
            {
                return;
            }

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
