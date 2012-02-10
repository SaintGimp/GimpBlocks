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

        public BlockPicker(BlockArray blockArray, ICamera camera)
        {
            _blockArray = blockArray;
            _camera = camera;
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

            var pickDistance = 4;
            var intersectionResult = ray.Intersects(_blockArray);
            if (intersectionResult != null)
            {
                SelectedBlock = intersectionResult.IntersectedBlock;
                SelectedPlacePosition = intersectionResult.IntersectedBlock.Position +
                                        intersectionResult.IntersectedFaceNormal;
            }
            else
            {
                SelectedBlock = null;
            }
        }

        public void Handle(ChunkRebuilt message)
        {
            PickBlock();
        }
    }
}
