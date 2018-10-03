using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GimpBlocks
{
    public interface IChunkRenderer
    {
        void Initialize(Vector3 worldLocation, IEnumerable<VertexPositionColorLighting>[] vertices, IEnumerable<short>[] indices);
        void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix);
    }

    public class ChunkRenderer : IChunkRenderer, IDisposable
    {
        readonly GraphicsDevice graphicsDevice;
        readonly Effect effect;
        readonly VertexBuffer[] vertexBuffers = new VertexBuffer[6];
        readonly IndexBuffer[] indexBuffers = new IndexBuffer[6];
        Vector3 worldLocation;

        public ChunkRenderer(GraphicsDevice graphicsDevice, Effect effect)
        {
            this.graphicsDevice = graphicsDevice;
            this.effect = effect;
        }

        public void Initialize(Vector3 worldLocation, IEnumerable<VertexPositionColorLighting>[] vertices, IEnumerable<short>[] indices)
        {
            this.worldLocation = worldLocation;
            
            DestroyBuffers();
            CreateVertexBuffers(vertices);
            CreateIndexBuffers(indices);
        }

        void CreateVertexBuffers(IEnumerable<VertexPositionColorLighting>[] vertices)
        {
            for (int x = 0; x < 6; x++)
            {
                VertexPositionColorLighting[] vertexArray = vertices[x].ToArray();
                if (vertexArray.Length > 0)
                {
                    vertexBuffers[x] = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorLighting), vertexArray.Length,
                        BufferUsage.WriteOnly);
                    vertexBuffers[x].SetData(vertexArray);
                }
            }
        }

        void CreateIndexBuffers(IEnumerable<short>[] indices)
        {
            for (int x = 0; x < 6; x++)
            {
                short[] indexArray = indices[x].ToArray();
                if (indexArray.Length > 0)
                {
                    indexBuffers[x] = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexArray.Length,
                        BufferUsage.WriteOnly);
                    indexBuffers[x].SetData(indexArray);
                }
            }
        }

        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            effect.Parameters["View"].SetValue(originBasedViewMatrix);
            effect.Parameters["Projection"].SetValue(projectionMatrix);
            effect.Parameters["World"].SetValue(GetWorldMatrix(cameraLocation));

            SetFillMode();

            // TODO: we can skip drawing certain face lists if we know that we're not in a position
            // to be able to see them, i.e. if the camera is higher than the entire chunk then we
            // don't have to draw the bottom face list.

            // TODO: when we get to textures, take a look at spritesheets

            // TODO: we need to generate the meshes for each chunk centered at the origin, then translate them
            // at render time to where they need to be.  Same technique as in GenesisEngine.

            for (int x = 0; x < 6; x++)
            {
                if (vertexBuffers[x] != null && indexBuffers[x] != null)
                {
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphicsDevice.Indices = indexBuffers[x];
                        graphicsDevice.SetVertexBuffer(vertexBuffers[x]);
                        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffers[x].IndexCount / 3);
                    }
                }
            }
        }

        Matrix GetWorldMatrix(Vector3 cameraLocation)
        {
            var locationRelativeToCamera = worldLocation - cameraLocation;

            Matrix scaleMatrix = Matrix.Identity;
            Matrix translationMatrix = Matrix.CreateTranslation(locationRelativeToCamera);

            return scaleMatrix * translationMatrix;
        }

        void SetFillMode()
        {
            if (Settings.Instance.ShouldDrawWireframe && graphicsDevice.RasterizerState.FillMode != FillMode.WireFrame)
            {
                graphicsDevice.RasterizerState = new RasterizerState() { FillMode = FillMode.WireFrame };
            }
            else if (!Settings.Instance.ShouldDrawWireframe && graphicsDevice.RasterizerState.FillMode != FillMode.Solid)
            {
                graphicsDevice.RasterizerState = new RasterizerState() { FillMode = FillMode.Solid };
            }
        }

        bool disposed;
        public void Dispose()
        {
            if (!disposed)
            {
                DestroyBuffers();

                disposed = true;
            }
        }

        void DestroyBuffers()
        {
            for (int x = 0; x < 6; x++)
            {
                if (vertexBuffers[x] != null)
                {
                    vertexBuffers[x].Dispose();
                }
                vertexBuffers[x] = null;

                if (indexBuffers[x] != null)
                {
                    indexBuffers[x].Dispose();
                }
                indexBuffers[x] = null;
            }
        }
    }
}
