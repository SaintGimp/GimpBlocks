using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GimpBlocks
{
    public class BlockPicker
        : IListener<CameraMoved>,
        IListener<ChunkRebuilt>
    {
        readonly BlockArray _blockArray;
        readonly ICamera _camera;
        readonly GraphicsDevice _graphicsDevice;

        public BlockPicker(BlockArray blockArray, ICamera camera, GraphicsDevice graphicsDevice)
        {
            _blockArray = blockArray;
            _camera = camera;
            _graphicsDevice = graphicsDevice;
        }

        public Block SelectedBlock;
        public BlockPosition SelectedPlacePosition;

        public void Handle(CameraMoved message)
        {
            PickBlock();
        }

        void PickBlock()
        {
            var ray = new Ray(_camera.Location, _camera.LookAt);

            var currentLocation = _camera.Location;
            var pickDistance = 4;
            var lowerBound = new BlockPosition((int)currentLocation.X - pickDistance, (int)currentLocation.Y - pickDistance,
                                               (int)currentLocation.Z - pickDistance);
            var upperBound = new BlockPosition((int)currentLocation.X + pickDistance, (int)currentLocation.Y + pickDistance,
                                               (int)currentLocation.Z + pickDistance);

            // TODO: potential optimizations: get only the blocks that could possibly be in view
            // (maybe plane/BB intersection tests, if they're faster?), test the nearest blocks first,
            // switch to imperative style
            float nearestIntersection = float.MaxValue;
            SelectedBlock = null;
            _blockArray.ForEachInVolume(lowerBound, upperBound, block =>
                {
                    if (block.Prototype.IsSolid)
                    {
                        var intersection = block.BoundingBox.Intersects(ray);
                        if (intersection != null && intersection < nearestIntersection)
                        {
                            nearestIntersection = intersection.Value;
                            SelectedBlock = block;
                        }
                    }
                });

            if (SelectedBlock != null)
            {
                // TODO: it would probably be more efficient to embed this calculation inside the ray/BB
                // intersection test, which uses three ray/slab tests (http://www.gamedev.net/topic/429443-obb-ray-and-obb-plane-intersection/)
                var selectedFaceNormal = CalculateSelectedFaceNormal(SelectedBlock.BoundingBox, ray, nearestIntersection);
                SelectedPlacePosition = SelectedBlock.Position + selectedFaceNormal;
            }

            //var testSelectedBlock = ray.Intersects(_blockArray);
            if (testSelectedBlock != null)
            {
                
            }
        }

        Vector3 CalculateSelectedFaceNormal(BoundingBox boundingBox, Ray ray, float nearestIntersection)
        {
            var intersectionPoint = ray.Position + (ray.Direction * nearestIntersection);

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

        bool IsClose(float first, float second)
        {
            return Math.Abs(first - second) < 0.00000001;
        }

        public void Handle(ChunkRebuilt message)
        {
            PickBlock();
        }
    }
}
