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
    /// Represents a labeled check box.
    /// </summary>
    public class CheckLabel : Control
    {
        private CheckBox _check = null;
        private Label _label = null;

        public bool AutoWidth { get; set; } = false;
        public int AutoWidthMax { get; set; } = 0;

        /// <summary>
        /// Gets or sets the value of the check box.
        /// </summary>
        public bool Checked
        {
            get
            {
                return _check.Checked;
            }
            set
            {
                _check.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the text of the checkbox's label.
        /// </summary>
        public string Text
        {
            get
            {
                return _label.Text;
            }
            set
            {
                _label.Text = value;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CheckLabel"/> control. 
        /// </summary>
        public CheckLabel() : base()
        {
            _check = new CheckBox();
            _label = new Label();
            Children.Add(_check);
            Children.Add(_label);
            _label.AutoSize = true;
        }

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            _check.X = 2;

            _label.AutoSize = true;

            _label.Alignment = TextAlignment.Left;

            _label.X = _check.X + _check.Width + 6;

            if (AutoWidth)
            {
                if (AutoWidthMax > 0)
                {
                    _label.AutoSizeMaxWidth = ((AutoWidthMax) - (_label.X));
                }
                else
                {
                    _label.AutoSizeMaxWidth = 0;
                }

                Width = _label.X + _label.Width + 4;
            }
            else
            {
                _label.AutoSizeMaxWidth = ((Width) - (_label.X));
            }

            Height = Math.Max(_label.Height, _check.Height) + 4;
            _check.Y = 4;
            _label.Y = (Height - _label.Height) / 2;
        }

        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
        }
    }
}
