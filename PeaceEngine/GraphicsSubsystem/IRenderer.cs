using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GraphicsSubsystem
{
    public interface IRenderer
    {
        /// <summary>
        /// Get the size of a texture.
        /// </summary>
        /// <param name="texture">The identifier of the texture.</param>
        /// <returns>The size of the texture in pixels.</returns>
        Point GetTextureSize(Texture2D texture);

        /// <summary>
        /// Get the current viewport of the renderer.
        /// </summary>
        /// <returns>Bounds of the viewport.</returns>
        Rectangle GetViewport();

        /// <summary>
        /// Called right before calls to <see cref="DrawBatch"/> to indicate that batches will be drawn.
        /// </summary>
        void BeginRender();

        /// <summary>
        /// Draw a batch of vertices.
        /// </summary>
        void DrawBatch(GraphicsState state, VertexPositionColorTexture[] vertexBuffer, int[] indexBuffer, int startIndex, int indexCount, object batchUserData);

        /// <summary>
        /// Called after a set of batches is drawn.
        /// </summary>
        void EndRender();
    }
}
