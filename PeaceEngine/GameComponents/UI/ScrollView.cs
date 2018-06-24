using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.UI
{
    /// <summary>
    /// A GUI element which can only contain one child and allows the child to be scrolled up and down by the mouse.
    /// </summary>
    public class ScrollView : Scrollable
    {
        private int _scrollOffset = 0;
        private Control _host = null;
        private int _scrollHeight = 0;

        private ScrollBar _scrollBar = null;

        /// <inheritdoc/>
        protected override void OnMouseScroll(MouseEventArgs e)
        {
            int delta = e.ScrollWheelDelta;
            int offset = MathHelper.Clamp(_scrollOffset - delta, 0, _scrollHeight - Height);
            if (offset != _scrollOffset)
            {
                _scrollOffset = offset;
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            if (_host == null)
                return;

            if (_scrollHeight != _host.Height)
            {
                _scrollHeight = _host.Height;
                if (_scrollHeight < Height)
                {
                    _scrollOffset = 0;
                }
                else
                {
                    _scrollOffset = MathHelper.Clamp(_scrollOffset, 0, _scrollHeight - Height);
                }
            }

            _host.X = 0;
            _host.Y = 0 - _scrollOffset;
            Width = _host.Width;
            _scrollHeight = _host.Height;
            _scrollBar.PreferredScrollHeight = _scrollHeight;
            _scrollBar.ScrollOffset = _scrollOffset;
            base.OnUpdate(time);
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            Theme.DrawPanel(gfx, PanelStyles.Dark);
        }
    }
}
