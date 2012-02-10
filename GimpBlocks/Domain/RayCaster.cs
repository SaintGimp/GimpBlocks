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

        public static IntersectionResult Intersects(this Ray ray, BlockArray blockArray)
        {
            BlockPosition startingBlockPosition = ray.Position;
            int currentBlockX = startingBlockPosition.X;
            int currentBlockY = startingBlockPosition.Y;
            int currentBlockZ = startingBlockPosition.Z;
            int stepX, stepY, stepZ;
            float tMaxX, tMaxY, tMaxZ;
            float tDeltaX, tDeltaY, tDeltaZ;

            if (ray.Direction.X > 0)
            {
                stepX = 1;
                tDeltaX = 1 / ray.Direction.X;
                tMaxX = (startingBlockPosition.X + 1 - ray.Position.X) * tDeltaX;
            }
            else if (ray.Direction.X < 0)
            {
                stepX = -1;
                tDeltaX = 1 / -ray.Direction.X;
                tMaxX = (ray.Position.X - startingBlockPosition.X) * tDeltaX;
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
                tMaxY = (startingBlockPosition.Y + 1 - ray.Position.Y) * tDeltaY;
            }
            else if (ray.Direction.Y < 0)
            {
                stepY = -1;
                tDeltaY = 1 / -ray.Direction.Y;
                tMaxY = (ray.Position.Y - startingBlockPosition.Y) * tDeltaY;
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
                tMaxZ = (startingBlockPosition.Z + 1 - ray.Position.Z) * tDeltaZ;
            }
            else if (ray.Direction.Z < 0)
            {
                stepZ = -1;
                tDeltaZ = 1 / -ray.Direction.Z;
                tMaxZ = (ray.Position.Z - startingBlockPosition.Z) * tDeltaZ;
            }
            else
            {
                stepZ = 0;
                tDeltaZ = 0;
                tMaxZ = float.MaxValue;
            }

            float currentRayLength;
            Face intersectedFace;
            do
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        intersectedFace = Face.X;
                        currentBlockX = currentBlockX + stepX;
                        currentRayLength = tMaxX;
                        tMaxX = tMaxX + tDeltaX;
                    }
                    else
                    {
                        intersectedFace = Face.Z;
                        currentBlockZ = currentBlockZ + stepZ;
                        currentRayLength = tMaxZ;
                        tMaxZ = tMaxZ + tDeltaZ;
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        intersectedFace = Face.Y;
                        currentBlockY = currentBlockY + stepY;
                        currentRayLength = tMaxY;
                        tMaxY = tMaxY + tDeltaY;
                    }
                    else
                    {
                        intersectedFace = Face.Z;
                        currentBlockZ = currentBlockZ + stepZ;
                        currentRayLength = tMaxZ;
                        tMaxZ = tMaxZ + tDeltaZ;
                    }
                }

                if (currentRayLength < 4)
                {
                    var blockPosition = new BlockPosition(currentBlockX, currentBlockY, currentBlockZ);
                    if (blockArray.IsInBounds(blockPosition))
                    {
                        var blockPrototype = blockArray[currentBlockX, currentBlockY, currentBlockZ];
                        if (blockPrototype.IsSolid)
                        {
                            var selectedFaceNormal = GetNormalForIntersectedFace(intersectedFace, stepX, stepY, stepZ);
                            return new IntersectionResult
                            {
                                IntersectedBlock = blockArray.GetAt(blockPosition),
                                IntersectedFaceNormal = selectedFaceNormal
                            };
                        }
                    }
                }
            }
            while (currentRayLength < 4);

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
