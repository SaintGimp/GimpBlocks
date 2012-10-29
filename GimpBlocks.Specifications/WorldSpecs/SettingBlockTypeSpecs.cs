using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using NSubstitute;

namespace GimpBlocks.Specifications.WorldSpecs
{
    [Subject("Setting block types")]
    public class when_blocks_are_set_to_stone : BasicWorldContext
    {
        static BlockVolume testVolume;

        Establish context = () =>
        {
            CreateWorld(1);
            var minimum = new BlockPosition(0, 0, 0);
            var maximum = new BlockPosition(Chunk.XDimension - 1, Chunk.YDimension - 1, Chunk.ZDimension - 1);
            testVolume = new BlockVolume(world, minimum, maximum);
        };

        Because of = () =>
            testVolume.SetAllTo(BlockPrototype.StoneBlock);

        It should_set_all_blocks_to_stone = () =>
            testVolume.ContainedBlocks().ShouldEachConformTo(block => block.Prototype == BlockPrototype.StoneBlock);
    }

    [Subject("Setting block types")]
    public class when_blocks_are_set_to_air : BasicWorldContext
    {
        static BlockVolume testVolume;

        Establish context = () =>
        {
            CreateWorld(1);
            var minimum = new BlockPosition(0, 0, 0);
            var maximum = new BlockPosition(Chunk.XDimension - 1, Chunk.YDimension - 1, Chunk.ZDimension - 1);
            testVolume = new BlockVolume(world, minimum, maximum);
        };

        Because of = () =>
            testVolume.SetAllTo(BlockPrototype.AirBlock);

        It should_set_all_blocks_to_air = () =>
            testVolume.ContainedBlocks().ShouldEachConformTo(block => block.Prototype == BlockPrototype.AirBlock);
    }
}
