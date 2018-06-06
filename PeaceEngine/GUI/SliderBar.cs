using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Plex.Engine.GUI
{
    /// <summary>
    /// A <see cref="ProgressBar"/> whose progress value can be modified using the mouse. This is useful for allowing the player to adjust percentage values. 
    /// </summary>
    public class SliderBar : ProgressBar
    {
        private bool _isMouseDown = false;

        /// <summary>
        /// Creates a new instance of the <see cref="SliderBar"/> class. 
        /// </summary>
        public SliderBar()
        {
            MouseDown += (o, a) =>
            {
                if(a.Button == MonoGame.Extended.Input.InputListeners.MouseButton.Left)
                    _isMouseDown = true;
            };
            MouseMove += (o, a) =>
            {
                if (_isMouseDown)
                {
                    float mousex = a.Position.X;
                    float width = Width;
                    Value = (mousex / width);
                }
            };
            MouseUp += (o, a) =>
            {
                if (a.Button == MonoGame.Extended.Input.InputListeners.MouseButton.Left)
                    _isMouseDown = false;
            };
        }

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            if (!LeftButtonPressed)
                _isMouseDown = false;
            base.OnUpdate(time);
        }
    }
}
