using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GimpBlocks
{
    public interface IWorldRenderer
    {
        void Initialize(IEnumerable<VertexPositionColorLighting> vertices, IEnumerable<short> indices);
        void Draw(Vector3 location, Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix);
    }

    public class WorldRenderer : IWorldRenderer
    {
        readonly GraphicsDevice _graphicsDevice;
        readonly Effect _effect;
        VertexBuffer _vertexBuffer;
        IndexBuffer _indexBuffer;

        public WorldRenderer(GraphicsDevice graphicsDevice, Effect effect)
        {
            _graphicsDevice = graphicsDevice;
            _effect = effect;
        }

        public void Initialize(IEnumerable<VertexPositionColorLighting> vertices, IEnumerable<short> indices)
        {
            CreateVertexBuffer(vertices);
            CreateIndexBuffer(indices);
        }

        void CreateVertexBuffer(IEnumerable<VertexPositionColorLighting> vertices)
        {
            VertexPositionColorLighting[] vertexArray = vertices.ToArray();
            _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColorLighting), vertexArray.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertexArray);
        }

        void CreateIndexBuffer(IEnumerable<short> indices)
        {
            short[] indexArray = indices.ToArray();
            _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, indexArray.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indexArray);
        }

        public void Draw(Vector3 location, Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            _effect.Parameters["View"].SetValue(originBasedViewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["World"].SetValue(GetWorldMatrix(Vector3.Zero, cameraLocation));

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphicsDevice.Indices = _indexBuffer;
                _graphicsDevice.SetVertexBuffer(_vertexBuffer);
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount, 0,
                                                      _indexBuffer.IndexCount / 3);
            }
        }

        Matrix GetWorldMatrix(Vector3 location, Vector3 cameraLocation)
        {
            var locationRelativeToCamera = location - cameraLocation;

            Matrix scaleMatrix = Matrix.Identity;
            Matrix translationMatrix = Matrix.CreateTranslation(locationRelativeToCamera);

            return scaleMatrix * translationMatrix;
        }
    }
}
