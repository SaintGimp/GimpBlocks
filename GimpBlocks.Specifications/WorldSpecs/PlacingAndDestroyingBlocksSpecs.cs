using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using NSubstitute;

namespace GimpBlocks.Specifications.WorldSpecs
{
    [Subject("Placing and destroying blocks")]
    public class when_placing_a_block : BasicWorldContext
    {
        Establish context = () =>
        {
            CreateWorld(1);
            world.Handle(new BlockSelectionChanged
            {
                SelectedBlock = world.GetBlockAt(groundBlockPosition),
                SelectedPlacePosition = groundBlockPosition.Up
            });
        };

        Because of = () =>
            world.Handle(new PlaceBlock());

        It should_place_a_new_block_in_the_selected_place = () =>
            world.GetBlockAt(groundBlockPosition.Up).Prototype.ShouldEqual(BlockPrototype.StoneBlock);

        It should_not_destroy_the_selected_block = () =>
            world.GetBlockAt(groundBlockPosition).Prototype.ShouldEqual(BlockPrototype.StoneBlock);
    }

    [Subject("Placing and destroying blocks")]
    public class when_destroying_a_block : BasicWorldContext
    {
        Establish context = () =>
        {
            CreateWorld(1);
            world.Handle(new BlockSelectionChanged
            {
                SelectedBlock = world.GetBlockAt(groundBlockPosition),
                SelectedPlacePosition = groundBlockPosition.Up
            });
        };

        Because of = () =>
            world.Handle(new DestroyBlock());

        It should_destroy_the_selected_block = () =>
            world.GetBlockAt(groundBlockPosition).Prototype.ShouldEqual(BlockPrototype.AirBlock);

        It should_not_place_a_new_block_in_the_selected_place = () =>
            world.GetBlockAt(groundBlockPosition.Up).Prototype.ShouldEqual(BlockPrototype.AirBlock);
    }
}
