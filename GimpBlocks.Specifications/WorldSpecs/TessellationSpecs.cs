using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;

namespace GimpBlocks.Specifications.WorldSpecs
{
    [Subject("Tessellation")]
    public class when_a_block_is_placed : BasicWorldContext
    {
        Establish context = () =>
        {
            CreateEmptyWorld(1);
            world.SetBlockPrototype(centerBlockPosition, BlockPrototype.StoneBlock);
        };

        Because of = () =>
            world.RebuildAllChunks();

        It should_generate_vertices_for_the_new_block = () =>
            chunkRenderer.Vertices.Count.ShouldEqual(24);

        It should_generate_indices_for_the_new_block = () =>
            chunkRenderer.Indices.Count.ShouldEqual(36);
    }

    [Subject("Tessellation")]
    public class when_a_block_is_placed_then_destroyed : BasicWorldContext
    {
        Establish context = () =>
        {
            CreateEmptyWorld(1);
            world.SetBlockPrototype(centerBlockPosition, BlockPrototype.StoneBlock);
            world.RebuildAllChunks();
            world.SetBlockPrototype(centerBlockPosition, BlockPrototype.AirBlock);
            chunkRenderer.Reset();
        };

        Because of = () =>
        {
            world.RebuildAllChunks();
        };

        It should_not_generate_any_vertices = () =>
            chunkRenderer.Vertices.ShouldBeEmpty();

        It should_not_generate_any_indices = () =>
            chunkRenderer.Indices.ShouldBeEmpty();
    }
}
