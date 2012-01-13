using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public static class RayExtensions
    {
        public static IntersectionResult Intersects(this Ray ray, BlockArray blockArray)
        {

            BlockPosition startingBlockPosition = ray.Position;
            var exitResult = GetExitPoint(ray, startingBlockPosition.BoundingBox);
            if (exitResult == null)
            {
                return null;
            }

            var blockPositionToTest = startingBlockPosition + exitResult.ExitFaceNormal;
            var intersectionDistance = blockPositionToTest.BoundingBox.Intersects(ray);

            while (intersectionDistance != null)
            {
                if (blockArray.IsInBounds(blockPositionToTest) && blockArray[blockPositionToTest].IsSolid)
                {
                    return new IntersectionResult() { IntersectedBlock = blockArray.GetAt(blockPositionToTest), IntersectionDistance = intersectionDistance.Value };
                }

                intersectionDistance = blockPositionToTest.BoundingBox.Intersects(ray);
                if (intersectionDistance != null)
                {
                    var faceNormal = CalculateIntersectedFaceNormal(blockPositionToTest.BoundingBox, ray, intersectionDistance.Value);
                    blockPositionToTest = blockPositionToTest + faceNormal;
                }
            }

            return null;
        }

        static ExitResult GetExitPoint(Ray ray, BoundingBox boundingBox)
        {
            var corners = boundingBox.GetCorners();

            var top = new Plane(corners[0], corners[1], corners[4]);
            var intersection = ray.Intersects(top);
            if (intersection != null)
            {
                return new ExitResult {ExitFaceNormal = Vector3.Up, IntersectionDistance = intersection.Value};
            }

            var bottom = new Plane(corners[2], corners[3], corners[6]);
            intersection = ray.Intersects(bottom);
            if (intersection != null)
            {
                return new ExitResult { ExitFaceNormal = Vector3.Down, IntersectionDistance = intersection.Value };
            }

            var left = new Plane(corners[0], corners[3], corners[4]);
            intersection = ray.Intersects(left);
            if (intersection != null)
            {
                return new ExitResult { ExitFaceNormal = Vector3.Left, IntersectionDistance = intersection.Value };
            }

            var right = new Plane(corners[1], corners[2], corners[5]);
            intersection = ray.Intersects(right);
            if (intersection != null)
            {
                return new ExitResult { ExitFaceNormal = Vector3.Right, IntersectionDistance = intersection.Value };
            }

            var back = new Plane(corners[4], corners[5], corners[6]);
            intersection = ray.Intersects(back);
            if (intersection != null)
            {
                return new ExitResult { ExitFaceNormal = Vector3.Forward, IntersectionDistance = intersection.Value };
            }

            var front = new Plane(corners[0], corners[1], corners[2]);
            intersection = ray.Intersects(front);
            if (intersection != null)
            {
                return new ExitResult { ExitFaceNormal = Vector3.Backward, IntersectionDistance = intersection.Value };
            }

            return null;
        }

        static Vector3 CalculateIntersectedFaceNormal(BoundingBox boundingBox, Ray ray, float intersectionDistance)
        {
            var intersectionPoint = ray.Position + (ray.Direction * intersectionDistance);

            if (IsClose(intersectionPoint.X, boundingBox.Min.X))
            {
                return Vector3.Left;
            }
            else if (IsClose(intersectionPoint.Y, boundingBox.Min.Y))
            {
                return Vector3.Down;
            }
            else if (IsClose(intersectionPoint.Z, boundingBox.Min.Z))
            {
                return Vector3.Forward;
            }
            else if (IsClose(intersectionPoint.X, boundingBox.Max.X))
            {
                return Vector3.Right;
            }
            else if (IsClose(intersectionPoint.Y, boundingBox.Max.Y))
            {
                return Vector3.Up;
            }
            else
            {
                return Vector3.Backward;
            }
        }

        static bool IsClose(float first, float second)
        {
            return Math.Abs(first - second) < 0.00000001;
        }
    }

    public class IntersectionResult
    {
        public Block IntersectedBlock;
        public float IntersectionDistance;
    }

    public class ExitResult
    {
        public BlockPosition ExitFaceNormal;
        public float IntersectionDistance;
    }
}
