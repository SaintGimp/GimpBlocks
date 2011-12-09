using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GimpBlocks
{
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColorLighting : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Lighting;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
          new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
          new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
          new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
