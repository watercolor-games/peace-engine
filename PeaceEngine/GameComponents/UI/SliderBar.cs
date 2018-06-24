using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.UI
{
    /// <summary>
    /// A <see cref="ProgressBar"/> whose progress value can be modified using the mouse. This is useful for allowing the player to adjust percentage values. 
    /// </summary>
    public class SliderBar : ProgressBar
    {
        private bool _dragging = false;

        /// <summary>
        /// Creates a new instance of the <see cref="SliderBar"/> class. 
        /// </summary>
        public SliderBar()
        {

        }

        protected override void OnMouseDragStart(MouseEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                _dragging = true;
            base.OnMouseDragStart(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            Value = (float)MathHelper.Clamp(e.Position.X, 0, Width) / (float)Width;
            base.OnClick(e);
        }

        protected override void OnMouseDrag(MouseEventArgs e)
        {
            if (_dragging == true)
            {
                float mouseX = MathHelper.Clamp(e.Position.X, 0, Width);
                Value = mouseX / (float)Width;
            }
            base.OnMouseDrag(e);
        }

        protected override void OnMouseDragEnd(MouseEventArgs e)
        {
            _dragging = false;
            base.OnMouseDragEnd(e);
        }
    }
}
