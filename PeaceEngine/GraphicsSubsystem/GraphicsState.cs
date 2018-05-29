using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GraphicsSubsystem
{
    /// <summary>
    /// Holds graphics state for rendering.
    /// </summary>
    public struct GraphicsState
    {
        /// <summary>
        /// The texture to render.
        /// </summary>
        public readonly Texture2D Texture;

        /// <summary>
        /// Blend state to apply.
        /// </summary>
        public readonly BlendState BlendState;

        /// <summary>
        /// Sampler state to apply.
        /// </summary>
        public readonly SamplerState SamplerState;

        /// <summary>
        /// Bounds of the scissor rectangle. Note that this scissor rectangle should only be applied
        /// if <see cref="UseScissorRect"/> is set to <code>true</code>.
        /// </summary>
        public readonly Rectangle ScissorRect;

        /// <summary>
        /// Indicates if the scissor rectangle should be applied.
        /// </summary>
        public bool UseScissorRect => ScissorRect != Rectangle.Empty;

        /// <summary>
        /// Create a new <see cref="GraphicsState"/> instance.
        /// </summary>
        /// <param name="texture">Id of the texture to render.</param>
        /// <param name="blendState">Blend state.</param>
        /// <param name="samplerState">Sampler state.</param>
        /// <param name="scissorRect">Scissor rectangle. <see cref="Rectangle.Empty"/> means no scissor rectangle is set.</param>
        public GraphicsState(Texture2D texture, BlendState blendState,
            SamplerState samplerState, Rectangle scissorRect)
        {
            Texture = texture;
            BlendState = blendState;
            SamplerState = samplerState;
            ScissorRect = scissorRect;
        }

        /// <summary>
        /// Get the default <see cref="GraphicsState"/>.
        /// </summary>
        public static GraphicsState Default => new GraphicsState(null, Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend, Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp, Rectangle.Empty);
    }
}
