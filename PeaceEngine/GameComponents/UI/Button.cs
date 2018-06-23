using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plex.Engine.GraphicsSubsystem;

namespace Plex.Engine.GameComponents.UI
{
    /// <summary>
    /// A class representing a clickable button.
    /// </summary>
    public class Button : Control
    {
        private string _text = "";
        private Texture2D _texture = null;

        public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Default;

        public string Text { get; set; } = "";

        public bool ShowImage { get; set; } = false;

        private Rectangle _imageRect = Rectangle.Empty;
        private Rectangle _textRect = Rectangle.Empty;

        public bool AutoSize { get; set; } = true;
        public int AutoSizeMaxWidth { get; set; } = 0;

        private string _wrapped = null;

        /// <summary>
        /// Gets or sets an icon texture for the button.
        /// </summary>
        public Texture2D Image
        {
            get
            {
                return _texture;
            }
            set
            {
                if (_texture == value)
                    return;
                _texture = value;
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            if(AutoSize)
            {
                int imageMargin = Theme.ButtonImageMargin;
                if (string.IsNullOrWhiteSpace(Text) || ShowImage == false)
                    imageMargin = 0;
                int imageWithPadding = (Theme.ButtonPaddingX) + imageMargin + (ShowImage ? Theme.ButtonIconSize : 0);

                if (!string.IsNullOrWhiteSpace(Text))
                {
                    int availableWidth = 0;

                    if (AutoSizeMaxWidth > 0)
                    {
                        availableWidth = AutoSizeMaxWidth - imageWithPadding;
                    }

                    _wrapped = TextRenderer.WrapText(Theme.GetFont(Themes.TextStyle.Button), Text, availableWidth, TextRenderers.WrapMode.Words);

                    var measure = Theme.GetFont(Themes.TextStyle.Button).MeasureString(_wrapped);
                    Width = imageWithPadding + (int)measure.X;
                    Height = Theme.ButtonPaddingY + Math.Max(Theme.ButtonIconSize, (int)measure.Y);
                }
                else
                {
                    Width = imageWithPadding;
                    Height = (Theme.ButtonPaddingY) + Theme.ButtonIconSize;
                }
            }
            base.OnUpdate(time);
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            var state = UIButtonState.Idle;
            if (ContainsMouse)
                state = UIButtonState.Hover;
            if (LeftButtonPressed)
                state = UIButtonState.Pressed;
            Theme.DrawButtonBackground(gfx, state, ButtonStyle);


            int innerWidth = 0;
            int textHeight = 0;

            int imageMargin = Theme.ButtonImageMargin;
            if (string.IsNullOrWhiteSpace(_wrapped) || ShowImage == false)
                imageMargin = 0;
            int imageWithPadding = imageMargin + (ShowImage ? Theme.ButtonIconSize : 0);

            if (!string.IsNullOrWhiteSpace(_wrapped))
            {
                var measure = Theme.GetFont(Themes.TextStyle.Button).MeasureString(_wrapped);
                innerWidth = imageWithPadding + (int)measure.X;
                textHeight = (int)measure.Y;
            }
            else
            {
                innerWidth = imageWithPadding;
            }

            int x = (gfx.Width - innerWidth) / 2;

            if (ShowImage)
            {
                if (_texture != null)
                    gfx.FillRectangle(x, (gfx.Height - Theme.ButtonIconSize) / 2, Theme.ButtonIconSize, Theme.ButtonIconSize, _texture, Theme.GetButtonTextColor(state, ButtonStyle));

                gfx.DrawString(Theme.GetFont(Themes.TextStyle.Button), _wrapped, new Vector2(x + Theme.ButtonIconSize + Theme.ButtonImageMargin, (gfx.Height - textHeight) / 2), Theme.GetButtonTextColor(state, ButtonStyle));
            }
            else
            {
                gfx.DrawString(_wrapped, new Vector2(x, (gfx.Height - textHeight) / 2), Theme.GetButtonTextColor(state, ButtonStyle), Theme.GetFont(Themes.TextStyle.Button), TextAlignment.Center, innerWidth);
            }
        }
    }

    public enum ButtonStyle
    {
        Default,
        Primary,
        Success,
        Warning,
        Danger,
    }

    public enum UIButtonState
    {
        Idle,
        Hover,
        Pressed
    }
}
