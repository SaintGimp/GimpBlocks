using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Microsoft.Xna.Framework;

namespace GimpBlocks.Specifications.WorldSpecs
{
    [Subject("Picking blocks")]
    public class when_a_block_is_in_line_of_sight_and_but_not_in_range_and_the_camera_moves : BlockPickingContext
    {
        Establish context = () =>
        {
            var cameraLocation = new Vector3(groundBlockPosition.X + 10, groundBlockPosition.Y + 10, groundBlockPosition.Z + 10);
            var lookAt = new Vector3(groundBlockPosition.X + .5f, groundBlockPosition.Y + 1, groundBlockPosition.Z + .5f);
            camera.SetViewParameters(cameraLocation, lookAt);
        };

        Because of = () =>
            blockPicker.Handle(new CameraMoved());

        It should_notify_that_the_block_selection_changed = () =>
            listener.ShouldReceive<BlockSelectionChanged>();

        It should_not_pick_a_block = () =>
            listener.FirstMessage<BlockSelectionChanged>().SelectedBlock.ShouldBeNull();

        It should_not_choose_a_place_position = () =>
            listener.FirstMessage<BlockSelectionChanged>().SelectedPlacePosition.ShouldEqual(new BlockPosition(0, 0, 0));
    }

    [Subject("Picking blocks")]
    public class when_a_block_is_in_line_of_sight_but_not_in_range_and_a_chunk_is_rebuilt : BlockPickingContext
    {
        // TODO: MSpec is supposed to have features that allow us to reduce duplication here but
        // I couldn't get them to work with a few minutes effort

        Establish context = () =>
        {
            var cameraLocation = new Vector3(groundBlockPosition.X + 10, groundBlockPosition.Y + 10, groundBlockPosition.Z + 10);
            var lookAt = new Vector3(groundBlockPosition.X + .5f, groundBlockPosition.Y + 1, groundBlockPosition.Z + .5f);
            camera.SetViewParameters(cameraLocation, lookAt);
        };

        Because of = () =>
            blockPicker.Handle(new ChunkRebuilt());

        It should_notify_that_the_block_selection_changed = () =>
            listener.ShouldReceive<BlockSelectionChanged>();

        It should_not_pick_a_block = () =>
            listener.FirstMessage<BlockSelectionChanged>().SelectedBlock.ShouldBeNull();

        It should_not_choose_a_place_position = () =>
            listener.FirstMessage<BlockSelectionChanged>().SelectedPlacePosition.ShouldEqual(new BlockPosition(0, 0, 0));
    }

    [Subject("Picking blocks")]
    public class when_a_block_is_in_line_of_sight_and_in_range_and_the_camera_moves : BlockPickingContext
    {
        Establish context = () =>
        {
            var cameraLocation = new Vector3(groundBlockPosition.X + 2, groundBlockPosition.Y + 2, groundBlockPosition.Z + 2);
            var lookAt = new Vector3(groundBlockPosition.X + .5f, groundBlockPosition.Y + 1, groundBlockPosition.Z + .5f);
            camera.SetViewParameters(cameraLocation, lookAt);
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

    [Subject("Picking blocks")]
    public class when_a_block_is_in_line_of_sight_and_in_range_and_a_chunk_is_rebuilt : BlockPickingContext
    {
        Establish context = () =>
        {
            var cameraLocation = new Vector3(groundBlockPosition.X + 2, groundBlockPosition.Y + 2, groundBlockPosition.Z + 2);
            var lookAt = new Vector3(groundBlockPosition.X + .5f, groundBlockPosition.Y + 1, groundBlockPosition.Z + .5f);
            camera.SetViewParameters(cameraLocation, lookAt);
        };

        Because of = () =>
            blockPicker.Handle(new ChunkRebuilt());

        It should_notify_that_the_block_selection_changed = () =>
            listener.ShouldReceive<BlockSelectionChanged>();

        It should_pick_the_first_block_along_line_of_sight = () =>
            listener.FirstMessage<BlockSelectionChanged>().SelectedBlock.Position.ShouldEqual(groundBlockPosition);

        It should_select_the_place_position_along_the_line_of_sight = () =>
            listener.FirstMessage<BlockSelectionChanged>().SelectedPlacePosition.ShouldEqual(groundBlockPosition.Up);
    }

    public class BlockPickingContext : BasicWorldContext
    {
        public static Camera camera;
        public static BlockPickerListener listener;
        public static BlockPicker blockPicker;

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
