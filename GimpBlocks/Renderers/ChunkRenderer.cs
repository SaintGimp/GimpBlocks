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

    public class ChunkRenderer : IChunkRenderer
    {
        readonly GraphicsDevice _graphicsDevice;
        readonly Effect _effect;
        readonly VertexBuffer[] _vertexBuffers = new VertexBuffer[6];
        readonly IndexBuffer[] _indexBuffers = new IndexBuffer[6];
        Vector3 _worldLocation;

        public ChunkRenderer(GraphicsDevice graphicsDevice, Effect effect)
        {
            _graphicsDevice = graphicsDevice;
            _effect = effect;
        }

        public void Initialize(Vector3 worldLocation, IEnumerable<VertexPositionColorLighting>[] vertices, IEnumerable<short>[] indices)
        {
            _worldLocation = worldLocation;
            CreateVertexBuffers(vertices);
            CreateIndexBuffers(indices);
        }

        void CreateVertexBuffers(IEnumerable<VertexPositionColorLighting>[] vertices)
        {
            for (int x = 0; x < 6; x++)
            {
                // TODO: need to handle case where there are no vertices at all
                VertexPositionColorLighting[] vertexArray = vertices[x].ToArray();
                if (vertexArray.Length > 0)
                {
                    _vertexBuffers[x] = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColorLighting), vertexArray.Length,
                        BufferUsage.WriteOnly);
                    _vertexBuffers[x].SetData(vertexArray);
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
                    _indexBuffers[x] = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, indexArray.Length,
                        BufferUsage.WriteOnly);
                    _indexBuffers[x].SetData(indexArray);
                }
            }
        }

        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            _effect.Parameters["View"].SetValue(originBasedViewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["World"].SetValue(GetWorldMatrix(cameraLocation));

            SetFillMode();

            // TODO: we can skip drawing certain face lists if we know that we're not in a position
            // to be able to see them, i.e. if the camera is higher than the entire chunk then we
            // don't have to draw the bottom face list.

            // TODO: when we get to textures, take a look at spritesheets

            // TODO: we need to generate the meshes for each chunk centered at the origin, then translate them
            // at render time to where they need to be.  Same technique as in GenesisEngine.

            for (int x = 0; x < 6; x++)
            {
                if (_vertexBuffers[x] != null && _indexBuffers[x] != null)
                {
                    foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        _graphicsDevice.Indices = _indexBuffers[x];
                        _graphicsDevice.SetVertexBuffer(_vertexBuffers[x]);
                        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffers[x].VertexCount, 0,
                            _indexBuffers[x].IndexCount / 3);
                    }
                }
            }
        }

        Matrix GetWorldMatrix(Vector3 cameraLocation)
        {
            var locationRelativeToCamera = _worldLocation - cameraLocation;

            Matrix scaleMatrix = Matrix.Identity;
            Matrix translationMatrix = Matrix.CreateTranslation(locationRelativeToCamera);

            return scaleMatrix * translationMatrix;
        }

        void SetFillMode()
        {
            if (Settings.Instance.ShouldDrawWireframe && _graphicsDevice.RasterizerState.FillMode != FillMode.WireFrame)
            {
                _graphicsDevice.RasterizerState = new RasterizerState() { FillMode = FillMode.WireFrame };
            }
            else if (!Settings.Instance.ShouldDrawWireframe && _graphicsDevice.RasterizerState.FillMode != FillMode.Solid)
            {
                _graphicsDevice.RasterizerState = new RasterizerState() { FillMode = FillMode.Solid };
            }
        }

    }
}
