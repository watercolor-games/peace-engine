using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Plex.Engine.GraphicsSubsystem;

namespace Plex.Engine.GUI
{
    /// <summary>
    /// A class representing a clickable button.
    /// </summary>
    public class Button : Control
    {
        private string _text = "";
        private Texture2D _texture = null;
        private bool _showImage = false;

        private float _textureX = 0;
        private float _textureY = 0;
        private float _textureW = 0;
        private float _textureH = 0;

        private float _lX = 0;
        private float _lY = 0;
        private float _lW = 0;
        private float _lH = 0;

        private bool _requireLayout = true;

        /// <summary>
        /// Retrieves a rectangle where the button's icon is situated.
        /// </summary>
        public Rectangle ImageRect
        {
            get
            {
                var tLoc = Manager.VirtualToScreen(new Vector2(_textureX, _textureY));
                return new Rectangle((int)tLoc.X, (int)tLoc.Y, (int)(_textureW * Manager.GUIScale), (int)(_textureH * Manager.GUIScale));
            }
        }

        /// <summary>
        /// Retrieves a rectangle where the button's text is situated.
        /// </summary>
        public Rectangle TextRect
        {
            get
            {
                var lLoc = Manager.VirtualToScreen(new Vector2(_lX, _lY));
                return new Rectangle((int)lLoc.X, (int)lLoc.Y, (int)(_lW), (int)(_lH));
            }
        }

        /// <summary>
        /// Gets or sets whether an icon should be rendered on the button.
        /// </summary>
        public bool ShowImage
        {
            get
            {
                return _showImage;
            }
            set
            {
                if (_showImage == value)
                    return;
                _showImage = value;
                Invalidate(true);
                _requireLayout = true;
            }
        }

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
                Invalidate(true);
                _requireLayout = true;
            }
        }

        /// <summary>
        /// Gets or sets the text of the button.
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                Invalidate(true);
                _requireLayout = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            if (_requireLayout)
            {
                if (_showImage)
                {
                    _textureW = 16;
                    _textureH = 16;
                }
                else
                {
                    _textureW = 0;
                    _textureH = 0;
                }
                float _minimumHorizontal = 0;
                TextAlignment _alignment = TextAlignment.Center;
                if (_showImage)
                {
                    _minimumHorizontal += _textureW;
                    if (!string.IsNullOrEmpty(_text))
                        _minimumHorizontal += 3;
                    _alignment = TextAlignment.Left;
                }
                float realMax = MaxWidth == 0 ? int.MaxValue : MaxWidth;

                var font = Theme.GetFont(Themes.TextFontStyle.Highlight);
                var measure = TextRenderer.MeasureText(_text, font, realMax - _minimumHorizontal, WrapMode.Words);

                _lW = (int)measure.X;
                _lH = (int)measure.Y;

                float realHeight = 6 + (Math.Max(_textureH, _lH));
                Height = realHeight;
                float realWidth = 0;
                if (!string.IsNullOrEmpty(_text))
                    realWidth = (int)measure.X;
                if (_showImage)
                {
                    if (realWidth > 0)
                        realWidth += 3;
                    realWidth += _textureW;
                }
                if (!string.IsNullOrWhiteSpace(_text))
                    realWidth += 20;
                else
                    realWidth += 8;
                Width = realWidth;
                _lY = (Height - _lH) / 2;
                if (_showImage)
                {
                    _textureX = 4;
                    _textureY = (Height - _textureH) / 2;
                    _lX = _textureX + _textureW + 3;
                }
                else
                {
                    _lX = (Width - _lW) / 2;
                }
                _requireLayout = false;
            }

            base.OnUpdate(time);
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            var state = Themes.UIButtonState.Idle;
            if (ContainsMouse)
                state = Themes.UIButtonState.Hover;
            if (LeftMouseState == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                state = Themes.UIButtonState.Pressed;
            Theme.DrawButton(gfx, _text, _texture, state, _showImage, ImageRect, TextRect);

        }
    }
}
