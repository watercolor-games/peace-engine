using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;

namespace Plex.Engine.GameComponents.UI
{
    /// <summary>
    /// Represents a boolean value as a GUI element.
    /// </summary>
    public class CheckBox : Control
    {
        private bool _checked = false;

        public event EventHandler CheckedChanged;

        protected override void OnClick(MouseEventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        /// <summary>
        /// Gets or sets the value of the check box.
        /// </summary>
        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                if (_checked == value)
                    return;
                _checked = value;
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnUpdate(GameTime time)
        {
            Width = Theme.CheckBoxSize;
            Height = Width;
            base.OnUpdate(time);
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            Theme.DrawCheckBox(gfx, _checked, ContainsMouse);
        }
    }
}
