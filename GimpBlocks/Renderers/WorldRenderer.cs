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
        void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix);
    }

    public class WorldRenderer : IWorldRenderer
    {
        readonly GraphicsDevice graphicsDevice;
        readonly Effect effect;

        public WorldRenderer(GraphicsDevice graphicsDevice, Effect effect)
        {
            this.graphicsDevice = graphicsDevice;
            this.effect = effect;
        }

        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            effect.Parameters["View"].SetValue(originBasedViewMatrix);
            effect.Parameters["Projection"].SetValue(projectionMatrix);
            effect.Parameters["World"].SetValue(GetWorldMatrix(cameraLocation));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }
        }

        Matrix GetWorldMatrix(Vector3 cameraLocation)
        {
            var locationRelativeToCamera = Vector3.Zero - cameraLocation;

            Matrix scaleMatrix = Matrix.Identity;
            Matrix translationMatrix = Matrix.CreateTranslation(locationRelativeToCamera);

            return scaleMatrix * translationMatrix;
        }
    }
}
