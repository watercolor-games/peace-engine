using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Plex.Engine.GraphicsSubsystem
{
    public class NullRenderer : IRenderer
    {
        public void BeginRender()
        {
        }

        public void DrawBatch(GraphicsState state, VertexPositionColorTexture[] vertexBuffer, int[] indexBuffer, int startIndex, int indexCount, object batchUserData)
        {
        }

        public void EndRender()
        {
        }

        public Point GetTextureSize(Microsoft.Xna.Framework.Graphics.Texture2D texture)
        {
            return Point.Zero;
        }

        public Rectangle GetViewport()
        {
            return Rectangle.Empty;
        }
    }
}
