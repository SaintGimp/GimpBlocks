using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public static class RayExtensions
    {
        enum Face
        {
            X,
            Y,
            Z
        };

        public static IntersectionResult Intersects(this Ray ray, World world, float maximumRayLength)
        {
            // From A Fast Voxel Traversal Algorithm for Ray Tracing: http://www.cse.yorku.ca/~amana/research/grid.pdf

            var currentBlockPosition = new BlockPosition(ray.Position);
            int stepX, stepY, stepZ;
            float tMaxX, tMaxY, tMaxZ;
            float tDeltaX, tDeltaY, tDeltaZ;

            if (ray.Direction.X > 0)
            {
                stepX = 1;
                tDeltaX = 1 / ray.Direction.X;
                tMaxX = (currentBlockPosition.X + 1 - ray.Position.X) * tDeltaX;
            }
            else if (ray.Direction.X < 0)
            {
                stepX = -1;
                tDeltaX = 1 / -ray.Direction.X;
                tMaxX = (ray.Position.X - currentBlockPosition.X) * tDeltaX;
            }
            else
            {
                stepX = 0;
                tDeltaX = 0;
                tMaxX = float.MaxValue;
            }

            if (ray.Direction.Y > 0)
            {
                stepY = 1;
                tDeltaY = 1 / ray.Direction.Y;
                tMaxY = (currentBlockPosition.Y + 1 - ray.Position.Y) * tDeltaY;
            }
            else if (ray.Direction.Y < 0)
            {
                stepY = -1;
                tDeltaY = 1 / -ray.Direction.Y;
                tMaxY = (ray.Position.Y - currentBlockPosition.Y) * tDeltaY;
            }
            else
            {
                stepY = 0;
                tDeltaY = 0;
                tMaxY = float.MaxValue;
            }

            if (ray.Direction.Z > 0)
            {
                stepZ = 1;
                tDeltaZ = 1 / ray.Direction.Z;
                tMaxZ = (currentBlockPosition.Z + 1 - ray.Position.Z) * tDeltaZ;
            }
            else if (ray.Direction.Z < 0)
            {
                stepZ = -1;
                tDeltaZ = 1 / -ray.Direction.Z;
                tMaxZ = (ray.Position.Z - currentBlockPosition.Z) * tDeltaZ;
            }
            else
            {
                stepZ = 0;
                tDeltaZ = 0;
                tMaxZ = float.MaxValue;
            }

            float currentRayLength;
            do
            {
                Face intersectedFace;
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        currentRayLength = tMaxX;
                        intersectedFace = Face.X;
                        currentBlockPosition.X += stepX;
                        tMaxX = tMaxX + tDeltaX;
                    }
                    else
                    {
                        currentRayLength = tMaxZ;
                        intersectedFace = Face.Z;
                        currentBlockPosition.Z += stepZ;
                        tMaxZ = tMaxZ + tDeltaZ;
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        currentRayLength = tMaxY;
                        intersectedFace = Face.Y;
                        currentBlockPosition.Y += stepY;
                        tMaxY = tMaxY + tDeltaY;
                    }
                    else
                    {
                        currentRayLength = tMaxZ;
                        intersectedFace = Face.Z;
                        currentBlockPosition.Z += stepZ;
                        tMaxZ = tMaxZ + tDeltaZ;
                    }
                }

                if (currentRayLength < maximumRayLength)
                {
                    var block = world.GetBlockAt(currentBlockPosition);
                    if (block.CanBeSelected)
                    {
                        var selectedFaceNormal = GetNormalForIntersectedFace(intersectedFace, stepX, stepY, stepZ);
                        return new IntersectionResult
                        {
                            IntersectedBlock = block,
                            IntersectedFaceNormal = selectedFaceNormal
                        };
                    }
                }
            }
            while (currentRayLength < maximumRayLength);

            return null;
        }

        static Vector3 GetNormalForIntersectedFace(Face intersectedFace, int stepX, int stepY, int stepZ)
        {
            switch (intersectedFace)
            {
                case Face.X:
                    return new Vector3(-stepX, 0, 0);
                case Face.Y:
                    return new Vector3(0, -stepY, 0);
                default:
                    return new Vector3(0, 0, -stepZ);
            }
        }
    }

    public class IntersectionResult
    {
        public Block IntersectedBlock;
        public Vector3 IntersectedFaceNormal;
    }
}
