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

        It should_light_the_top_face = () =>
            chunkRenderer.Vertices.Count(v => v.Lighting.X == Tessellator.TopFaceLightingLimit).ShouldEqual(4);

        It should_light_the_side_faces = () =>
            chunkRenderer.Vertices.Count(v => v.Lighting.X == Tessellator.SideFaceLightingLimit).ShouldEqual(16);

        It should_light_the_bottom_face = () =>
            chunkRenderer.Vertices.Count(v => v.Lighting.X == Tessellator.BottomFaceLightingLimit).ShouldEqual(4);
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
            world.RebuildAllChunks();

        It should_not_generate_any_vertices = () =>
            chunkRenderer.Vertices.ShouldBeEmpty();

        It should_not_generate_any_indices = () =>
            chunkRenderer.Indices.ShouldBeEmpty();
    }

    [Subject("Tessellation")]
    public class when_blocks_are_adjacent_to_each_other : BasicWorldContext
    {
        Establish context = () =>
        {
            CreateEmptyWorld(1);
            world.SetBlockPrototype(centerBlockPosition, BlockPrototype.StoneBlock);
            world.SetBlockPrototype(centerBlockPosition.Up, BlockPrototype.StoneBlock);
        };

        Because of = () =>
            world.RebuildAllChunks();

        It should_not_generate_verticies_for_hidden_faces = () =>
            chunkRenderer.Vertices.Count.ShouldEqual(40);

        It should_not_generate_indices_for_hidden_faces = () =>
            chunkRenderer.Indices.Count.ShouldEqual(60);
    }

    [Subject("Tessellation")]
    public class when_block_vertices_are_adjacent_to_solid_blocks_in_their_plane : BasicWorldContext
    {
        Establish context = () =>
        {
            CreateEmptyWorld(1);
            world.SetBlockPrototype(centerBlockPosition, BlockPrototype.StoneBlock);
            world.SetBlockPrototype(centerBlockPosition.Up.Back, BlockPrototype.StoneBlock);
            world.SetBlockPrototype(centerBlockPosition.Up.Right, BlockPrototype.StoneBlock);
        };

        Because of = () =>
            world.RebuildAllChunks();

        // This could break if the light limits for bottom faces is raised
        It should_have_some_light_occluded = () =>
            chunkRenderer.Vertices.Count(v => v.Lighting.X == 3f / 4f).ShouldEqual(8);

        It should__not_get_light_from_diagonal_blocks_screened_by_solid_blocks = () =>
            chunkRenderer.Vertices.Count(v => v.Lighting.X == 1f / 3f).ShouldEqual(3);
    }

    [Subject("Tessellation")]
    public class when_blocks_are_adjacent_to_each_other_across_a_chunk_boundary : BasicWorldContext
    {
        Establish context = () =>
        {
            CreateEmptyWorld(2);
            world.SetBlockPrototype(new BlockPosition(Chunk.XDimension - 1, 0, 0), BlockPrototype.StoneBlock);
            world.SetBlockPrototype(new BlockPosition(Chunk.XDimension, 0, 0), BlockPrototype.StoneBlock);
        };

        Because of = () =>
            world.RebuildAllChunks();

        It should_not_generate_vertices_for_hidden_faces = () =>
            chunkRenderer.Vertices.Count.ShouldEqual(40);

        It should_not_generate_indices_for_hidden_faces = () =>
            chunkRenderer.Indices.Count.ShouldEqual(60);
    }
}
