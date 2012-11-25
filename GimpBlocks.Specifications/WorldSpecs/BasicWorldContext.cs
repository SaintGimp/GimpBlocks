using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Microsoft.Xna.Framework;
using NSubstitute;

namespace GimpBlocks.Specifications.WorldSpecs  
{
    public class BasicWorldContext
    {
        public static IBoundingBoxRenderer boundingBoxRenderer;
        public static FakeChunkRenderer chunkRenderer;
        public static ChunkFactory flatChunkFactory;
        public static ChunkFactory emptyChunkFactory;
        public static World world;
        public static BlockPosition groundBlockPosition = 
            new BlockPosition(Chunk.XDimension / 2, FlatEnvironmentGenerator.GroundLevel, Chunk.ZDimension / 2);
        public static BlockPosition centerBlockPosition =
            new BlockPosition(Chunk.XDimension / 2, Chunk.YDimension / 2, Chunk.ZDimension / 2);

        Establish context = () =>
        {
            boundingBoxRenderer = Substitute.For<IBoundingBoxRenderer>();
            chunkRenderer = new FakeChunkRenderer();
            flatChunkFactory = new ChunkFactory(new FlatEnvironmentGenerator(), () => chunkRenderer);
            emptyChunkFactory = new ChunkFactory(new EmptyEnvironmentGenerator(), () => chunkRenderer);
        };

        public static void CreateFlatWorld(int viewDistance)
        {
            world = new World(viewDistance, flatChunkFactory, boundingBoxRenderer);
            world.LoadChunks();
        }

        public static void CreateEmptyWorld(int viewDistance)
        {
            world = new World(viewDistance, emptyChunkFactory, boundingBoxRenderer);
            world.LoadChunks();
        }
    }

    public class FakeChunkRenderer : IChunkRenderer
    {
        public void Initialize(Vector3 worldLocation, IEnumerable<VertexPositionColorLighting>[] vertices, IEnumerable<short>[] indices)
        {
            foreach (var vertexSet in vertices)
            {
                Vertices.AddRange(vertexSet);
            }

            foreach (var indexSet in indices)
            {
                Indices.AddRange(indexSet);
            }
        }

        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            throw new NotImplementedException();
        }

        public List<VertexPositionColorLighting> Vertices = new List<VertexPositionColorLighting>();

        public List<short> Indices = new List<short>();

        public void Reset()
        {
            Vertices.Clear();
            Indices.Clear();
        }
    }
}
