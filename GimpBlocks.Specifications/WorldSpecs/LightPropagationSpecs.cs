using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;

namespace GimpBlocks.Specifications.WorldSpecs
{
    [Subject("Propagating light")]
    public class when_sunlight_is_cast_on_flat_terrain : BasicWorldContext
    {
        static BlockVolume airVolume;
        static BlockVolume groundVolume;

        Establish context = () =>
        {
            CreateWorld(1);
            airVolume = new BlockVolume(world,
                new BlockPosition(0, groundBlockPosition.Y + 1, 0),
                new BlockPosition(Chunk.XDimension - 1, Chunk.YDimension - 1, Chunk.ZDimension - 1));
            groundVolume = new BlockVolume(world,
                new BlockPosition(0, 0, 0),
                new BlockPosition(Chunk.XDimension - 1, groundBlockPosition.Y, Chunk.ZDimension - 1));
        };

        Because of = () =>
            world.DoLighting();

        It should_have_maximum_light_in_the_air = () =>
            airVolume.ContainedBlocks().ShouldEachConformTo(block => block.LightLevel == World.MaximumLightLevel);

        It should_no_light_underground = () =>
            groundVolume.ContainedBlocks().ShouldEachConformTo(block => block.LightLevel == 0);
    }

    [Subject("Propagating light")]
    public class when_sunlight_is_cast_on_an_overhang : BasicWorldContext
    {
        static BlockVolume overhangVolume;
        static BlockVolume underneathOverhangVolume;

        Establish context = () =>
        {
            CreateWorld(1);
            overhangVolume = new BlockVolume(world,
                new BlockPosition(groundBlockPosition.X - 2, groundBlockPosition.Y + 10, groundBlockPosition.Z - 2),
                new BlockPosition(groundBlockPosition.X + 2, groundBlockPosition.Y + 10, groundBlockPosition.Z + 2));
            underneathOverhangVolume = new BlockVolume(world,
                new BlockPosition(groundBlockPosition.X - 2, groundBlockPosition.Y + 1, groundBlockPosition.Z - 2),
                new BlockPosition(groundBlockPosition.X + 2, groundBlockPosition.Y + 9, groundBlockPosition.Z + 2));

            overhangVolume.SetAllTo(BlockPrototype.StoneBlock);
        };

        Because of = () =>
            world.DoLighting();

        It should_not_have_maximum_light_under_the_overhang = () =>
            underneathOverhangVolume.ContainedBlocks().ShouldEachConformTo(block => block.LightLevel != World.MaximumLightLevel);

        It should_propagate_light_under_the_overhang = () =>
            underneathOverhangVolume.ContainedBlocks().ShouldEachConformTo(block => block.LightLevel > 0);

        It should_decrease_light_at_every_propagation_step = () =>
        {
            underneathOverhangVolume.ContainedBlocks().Count(block => block.LightLevel == World.MaximumLightLevel - 1).ShouldEqual(144);
            underneathOverhangVolume.ContainedBlocks().Count(block => block.LightLevel == World.MaximumLightLevel - 2).ShouldEqual(72);
            underneathOverhangVolume.ContainedBlocks().Count(block => block.LightLevel == World.MaximumLightLevel - 3).ShouldEqual(9);
        };
    }

    [Subject("Propagating light")]
    public class when_there_are_unlit_areas_with_no_connections_to_lit_areas : BasicWorldContext
    {
        static BlockVolume undergroundVolume;

        Establish context = () =>
        {
            CreateWorld(1);
            undergroundVolume = new BlockVolume(world,
                new BlockPosition(groundBlockPosition.X - 2, groundBlockPosition.Y - 10, groundBlockPosition.Z - 2),
                new BlockPosition(groundBlockPosition.X + 2, groundBlockPosition.Y - 20, groundBlockPosition.Z + 2));

            undergroundVolume.SetAllTo(BlockPrototype.AirBlock);
        };

        Because of = () =>
            world.DoLighting();

        It should_not_have_any_light_in_the_cavity = () =>
            undergroundVolume.ContainedBlocks().ShouldEachConformTo(block => block.LightLevel == 0);
    }

    [Subject("Propagating light")]
    public class when_there_are_unlit_areas_with_indirect_connections_to_lit_areas : BasicWorldContext
    {
        static BlockVolume downVolume;
        static BlockVolume sidewaysVolume;
        static BlockVolume upVolume;

        Establish context = () =>
        {
            CreateWorld(1);
            downVolume = new BlockVolume(world, groundBlockPosition, 0, -4, 0);
            sidewaysVolume = new BlockVolume(world, downVolume.Minimum, 4, 0, 0);
            upVolume = new BlockVolume(world, sidewaysVolume.Maximum, 0, 2, 0);

            downVolume.SetAllTo(BlockPrototype.AirBlock);
            sidewaysVolume.SetAllTo(BlockPrototype.AirBlock);
            upVolume.SetAllTo(BlockPrototype.AirBlock);
        };

        Because of = () =>
            world.DoLighting();

        It should_propagate_light_around_corners = () =>
            world.GetBlockAt(upVolume.Maximum).LightLevel.ShouldEqual((byte)9);
    }

    [Subject("Propagating light")]
    public class when_there_are_unlit_areas_a_long_way_away_from_light : BasicWorldContext
    {
        static BlockVolume downVolume;
        static BlockVolume sidewaysVolume;
        static BlockVolume upVolume;

        Establish context = () =>
        {
            CreateWorld(1);
            downVolume = new BlockVolume(world, groundBlockPosition, 0, -4, 0);
            sidewaysVolume = new BlockVolume(world, downVolume.Minimum, World.MaximumLightLevel, 0, 0);

            downVolume.SetAllTo(BlockPrototype.AirBlock);
            sidewaysVolume.SetAllTo(BlockPrototype.AirBlock);
        };

        Because of = () =>
            world.DoLighting();

        It should_propagate_light_until_it_runs_out = () =>
            world.GetBlockAt(sidewaysVolume.Maximum.Left).LightLevel.ShouldEqual((byte)1);

        It should_not_propagate_light_after_it_runs_out = () =>
            world.GetBlockAt(sidewaysVolume.Maximum).LightLevel.ShouldEqual((byte)0);
    }

    [Subject("Propagating light")]
    public class when_unlit_areas_cross_chunk_boundaries : BasicWorldContext
    {
        static BlockVolume downVolume;
        static BlockVolume sidewaysVolume;
        static BlockVolume upVolume;

        Establish context = () =>
        {
            CreateWorld(1);
            downVolume = new BlockVolume(world, new BlockPosition(Chunk.XDimension - 5, groundBlockPosition.Y, groundBlockPosition.Z), 0, -4, 0);
            sidewaysVolume = new BlockVolume(world, downVolume.Minimum, World.MaximumLightLevel, 0, 0);

            downVolume.SetAllTo(BlockPrototype.AirBlock);
            sidewaysVolume.SetAllTo(BlockPrototype.AirBlock);
        };

        Because of = () =>
            world.DoLighting();

        It should_propagate_light_across_chunk_boundaries = () =>
            world.GetBlockAt(sidewaysVolume.Maximum.Left).LightLevel.ShouldEqual((byte)1);
    }

    [Subject("Propagating light")]
    public class when_an_adjacent_chunk_has_higher_terrain : BasicWorldContext
    {
        static BlockVolume cubeVolume;
        static BlockPosition voidPosition;

        Establish context = () =>
        {
            // Set up a situation where there's an air block in an adjacent chunk that's cut off from the
            // light in the adjacent chunk but can be lit by the first chunk. Also, this air block is higher than
            // any terrain in the first block (so the propagation is in danger of being optimized away).
            CreateWorld(2);
            cubeVolume = new BlockVolume(world, new BlockPosition(Chunk.XDimension, groundBlockPosition.Y + 1, 0), 3, 3, 3);
            voidPosition = new BlockPosition(Chunk.XDimension, groundBlockPosition.Y + 2, 1);
            cubeVolume.SetAllTo(BlockPrototype.StoneBlock);
            world.SetBlockPrototype(voidPosition, BlockPrototype.AirBlock);
        };

        Because of = () =>
            world.DoLighting();

        It should_do_lighting_to_the_height_of_the_adjacent_terrain = () =>
            world.GetBlockAt(voidPosition).LightLevel.ShouldEqual((byte) (World.MaximumLightLevel - 1));
    }
}
