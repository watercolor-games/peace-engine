using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.TextRenderers;

namespace Plex.Engine.GUI
{
    /// <summary>
    /// Represents a user interface element that can display progress.
    /// </summary>
    public class ProgressBar : Control
    {
        private float _value = 0.0f;
        private string _text = null;

        /// <summary>
        /// The value of the progress bar (between 0.0 and 1.0).
        /// </summary>
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                value = MathHelper.Clamp(value, 0f, 1f);
                if (_value == value)
                    return;
                _value = value;
                Invalidate(true);
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        protected override void OnUpdate(GameTime time)
        {
            if(!string.IsNullOrWhiteSpace(_text))
            {
                Height = (int)TextRenderer.MeasureText(_text, Theme.GetFont(Themes.TextFontStyle.System), Width, WrapMode.Words).Y + 6;
            }
            base.OnUpdate(time);
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            gfx.Clear(Color.Black);
            gfx.DrawRectangle(0, 0, (int)MathHelper.Lerp(0, Width, _value), Height, Theme.GetAccentColor());
            if(!string.IsNullOrWhiteSpace(_text))
            {
                var f = Theme.GetFont(Themes.TextFontStyle.System);
                var c = Theme.GetFontColor(Themes.TextFontStyle.System);
                gfx.DrawString(_text, new Vector2(0, 3), c, f, TextAlignment.Center, Width, WrapMode.Words);
            }
        }
    }
}
