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
        readonly GraphicsDevice graphicsDevice;
        readonly BasicEffect effect;
        readonly short[] indices;

        public BoundingBoxRenderer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            effect = new BasicEffect(this.graphicsDevice);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;
            effect.LightingEnabled = false;
            effect.DiffuseColor = Vector3.One;
            effect.Alpha = 1f;

            indices = new short[]
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
            effect.View = originBasedViewMatrix;
            effect.Projection = projectionMatrix;
            effect.World = GetWorldMatrix(Vector3.Zero, cameraLocation);

            var vertices = CreateVertices(boundingBox);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, 8, indices, 0, indices.Length / 2);
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
