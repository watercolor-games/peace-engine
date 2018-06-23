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
    /// Gets or sets a UI element which can host other UI elements and resize based on its contents.
    /// </summary>
    public class Panel : Control
    {
        private bool _autosize = false;

        public bool DrawBackground { get; set; } = true;

        public PanelStyles PanelStyle { get; set; } = PanelStyles.Default;

        /// <summary>
        /// Gets or sets whether the panel should auto-size based on its contents.
        /// </summary>
        public bool AutoSize
        {
            get
            {
                return _autosize;
            }
            set
            {
                if (_autosize == value)
                    return;
                _autosize = value;
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            if (_autosize)
            {
                if (Children.Count > 0)
                {
                    var last = Children.Where(x => x.Visible).OrderByDescending(x => x.Y).First();
                    Height = last.Y + last.Height;
                }
                else
                {
                    Height = 0;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            if (DrawBackground)
                Theme.DrawPanel(gfx, PanelStyle);
        }
    }

    public enum PanelStyles
    {
        Default,
        Light,
        Dark
    }
}
