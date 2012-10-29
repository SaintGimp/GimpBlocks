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
        readonly World world;
        readonly ICamera camera;

        public BlockPicker(World world, ICamera camera)
        {
            this.world = world;
            this.camera = camera;
        }

        public void Handle(CameraMoved message)
        {
            PickBlock();
        }

        void PickBlock()
        {
            var ray = new Ray(camera.Location, camera.LookAt);

            var intersectionResult = ray.Intersects(world, 5);

            Block selectedBlock;
            BlockPosition selectedPlacePosition = new BlockPosition();
            if (intersectionResult != null)
            {
                selectedBlock = intersectionResult.IntersectedBlock;
                selectedPlacePosition = intersectionResult.IntersectedBlock.Position +
                    intersectionResult.IntersectedFaceNormal;
            }
            else
            {
                selectedBlock = null;
            }

            EventAggregator.Instance.SendMessage(new BlockSelectionChanged
            {
                SelectedBlock = selectedBlock,
                SelectedPlacePosition = selectedPlacePosition
            });
        }

        public void Handle(ChunkRebuilt message)
        {
            PickBlock();
        }
    }
}
