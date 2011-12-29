using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GimpBlocks
{
    public interface IBoundingBoxRenderer
    {
        void Draw(BoundingBox boundingBox, Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix);
    }

    public class BoundingBoxRenderer : IBoundingBoxRenderer
    {
        readonly GraphicsDevice _graphicsDevice;
        readonly BasicEffect _effect;
        readonly short[] _indices;

        public BoundingBoxRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _effect = new BasicEffect(_graphicsDevice);
            _effect.VertexColorEnabled = true;
            _effect.TextureEnabled = false;
            _effect.LightingEnabled = false;
            _effect.DiffuseColor = Vector3.One;
            _effect.Alpha = 1f;

            _indices = new short[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0,
                0, 4,
                1, 5,
                2, 6,
                3, 7,
                4, 5,
                5, 6,
                6, 7,
                7, 4,
            };
        }

        public void Draw(BoundingBox boundingBox, Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            _effect.View = originBasedViewMatrix;
            _effect.Projection = projectionMatrix;
            _effect.World = GetWorldMatrix(Vector3.Zero, cameraLocation);

            var vertices = CreateVertices(boundingBox);

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, 8, _indices, 0, _indices.Length / 2);
            }
        }

        VertexPositionColor[] CreateVertices(BoundingBox boundingBox)
        {
            var vertices = new VertexPositionColor[8];

            Vector3[] corners = boundingBox.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                vertices[i].Position = corners[i];
                vertices[i].Color = Color.Red;
            }

            return vertices;
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
