using Microsoft.Xna.Framework;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.UI
{
    /// <summary>
    /// A control that skips the painting process. Thus, it is invisible. Good for handling mouse events in an area that your control is rendering on its own.
    /// </summary>
    public class Hitbox : Control
    {
        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
        }
    }
}
