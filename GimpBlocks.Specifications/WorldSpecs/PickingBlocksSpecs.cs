using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Microsoft.Xna.Framework;

namespace GimpBlocks.Specifications.WorldSpecs
{
    [Subject("Picking blocks")]
    public class when_a_block_is_in_line_of_sight_and_in_range : BasicWorldContext
    {
        static Camera camera;
        static BlockPickerListener listener;
        static BlockPicker blockPicker;

        Establish context = () =>
        {
            CreateWorld(1);
            camera = new Camera();
            var cameraLocation = new Vector3(groundBlockPosition.X + 2, groundBlockPosition.Y + 2, groundBlockPosition.Z + 2);
            var lookAt = new Vector3(groundBlockPosition.X + .5f, groundBlockPosition.Y + 1, groundBlockPosition.Z + .5f);
            camera.SetViewParameters(cameraLocation, lookAt);
            listener = new BlockPickerListener();
            EventAggregator.Instance.AddListener(listener);
            blockPicker = new BlockPicker(world, camera);
        };

        Because of = () =>
            blockPicker.Handle(new CameraMoved());

        It should_notify_that_the_block_selection_changed = () =>
            listener.ShouldReceive<BlockSelectionChanged>();

        It should_pick_the_first_block_along_line_of_sight = () =>
            listener.FirstMessage<BlockSelectionChanged>().SelectedBlock.Position.ShouldEqual(groundBlockPosition);

        It should_select_the_place_position_along_the_line_of_sight = () =>
            listener.FirstMessage<BlockSelectionChanged>().SelectedPlacePosition.ShouldEqual(groundBlockPosition.Up);
    }

    public class BlockPickerListener : FakeEventListener,
        IListener<BlockSelectionChanged>
    {
        public void Handle(BlockSelectionChanged message)
        {
            RecordMessage(message);
        }
    }
}
