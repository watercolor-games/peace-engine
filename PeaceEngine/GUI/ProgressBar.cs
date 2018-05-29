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

        private string gettext()
        {
            if (!string.IsNullOrWhiteSpace(_text))
                return _text;
            return $"{Math.Round(_value, 2) * 100}%";
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
            if(!string.IsNullOrWhiteSpace(gettext()))
            {
                Height = (int)TextRenderer.MeasureText(gettext(), Theme.GetFont(Themes.TextFontStyle.System), Width, WrapMode.Words).Y + 6;
            }
            base.OnUpdate(time);
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            gfx.Clear(Color.Black);
            gfx.FillRectangle(0, 0, (int)MathHelper.Lerp(0, Width, _value), Height, Theme.GetAccentColor());
            if(!string.IsNullOrWhiteSpace(gettext()))
            {
                var f = Theme.GetFont(Themes.TextFontStyle.System);
                var c = Theme.GetFontColor(Themes.TextFontStyle.System);
                gfx.DrawString(gettext(), new Vector2(0, 3), c, f, TextAlignment.Center, Width, WrapMode.Words);
            }
        }
    }
}
