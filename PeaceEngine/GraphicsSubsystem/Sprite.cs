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
    /// A sprite value type, represented with a texture identifier and a source rectangle (in pixels) into the texture.
    /// </summary>
    public struct Sprite
    {
        /// <summary>
        /// Texture identifier.
        /// </summary>
        public readonly Texture2D Texture;

        /// <summary>
        /// Source rectangle of the sprite in the texture.
        /// </summary>
        public readonly Rectangle SrcRect;

        /// <summary>
        /// Create a new Sprite.
        /// </summary>
        /// <param name="texture">The texture identifier.</param>
        /// <param name="srcRect">The source rectangle of the sprite.</param>
        public Sprite(Texture2D texture, Rectangle srcRect)
        {
            Texture = texture;
            SrcRect = srcRect;
        }

        public override string ToString()
        {
            var t = $"{nameof(Texture)}: {Texture}";
            if (SrcRect != Rectangle.Empty)
                t = t + $", {nameof(SrcRect)}: {SrcRect}";
            return t;
        }
    }
}
