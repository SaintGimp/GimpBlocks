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
        void Draw(Vector3 location, Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix);
    }

    public class WorldRenderer : IWorldRenderer
    {
        readonly GraphicsDevice _graphicsDevice;
        readonly Effect _effect;

        public WorldRenderer(GraphicsDevice graphicsDevice, Effect effect)
        {
            _graphicsDevice = graphicsDevice;
            _effect = effect;
        }

        public void Draw(Vector3 location, Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            _effect.Parameters["View"].SetValue(originBasedViewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["World"].SetValue(GetWorldMatrix(Vector3.Zero, cameraLocation));

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
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
