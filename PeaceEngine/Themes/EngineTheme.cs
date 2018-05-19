using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.GUI;

namespace Plex.Engine.Themes
{
    public class EngineTheme : Theme
    {
        private Color _bg = Color.Black;
        private SpriteFont _font = null;
        private Color _fg1 = Color.White;
        private Color _fg2 = new Color(0x1B, 0xAA, 0xF7);

        private Color _close = Color.Red;
        
        public override Color ControlBG
        {
            get
            {
                return _bg;
            }
        }

        public override int ScrollbarSize => 17;

        public override int WindowTitleHeight => 24;

        public override int WindowBorderWidth => 1;

        public override void DrawArrow(GraphicsContext gfx, int x, int y, int width, int height, UIButtonState state, ArrowDirection direction)
        {
        }

        public override void DrawButton(GraphicsContext gfx, string text, Texture2D image, UIButtonState state, bool showImage, Rectangle imageRect, Rectangle textRect)
        {
        }

        public override void DrawCheckbox(GraphicsContext gfx, int x, int y, int width, int height, bool isChecked, bool isMouseOver)
        {
        }

        public override void DrawControlBG(GraphicsContext graphics, int x, int y, int width, int height)
        {
        }

        public override void DrawControlDarkBG(GraphicsContext graphics, int x, int y, int width, int height)
        {
        }

        public override void DrawControlLightBG(GraphicsContext graphics, int x, int y, int width, int height)
        {
        }

        public override void DrawDisabledString(GraphicsContext graphics, string text, int x, int y, int width, int height, TextFontStyle style)
        {
        }

        public override void DrawScrollbar(GraphicsContext gfx, Hitbox upArrow, Hitbox downArrow, Hitbox scrollNub)
        {
        }

        public override void DrawStatedString(GraphicsContext graphics, string text, int x, int y, int width, int height, TextFontStyle style, UIButtonState state)
        {
        }

        public override void DrawString(GraphicsContext graphics, string text, int x, int y, int width, int height, TextFontStyle style)
        {
        }

        public override void DrawTextCaret(GraphicsContext graphics, int x, int y, int width, int height)
        {
        }

        public override void DrawWindowBorder(GraphicsContext graphics, string titletext, Hitbox leftBorder, Hitbox rightBorder, Hitbox bottomBorder, Hitbox leftCorner, Hitbox rightCorner, Hitbox title, Hitbox close, Hitbox minimize, Hitbox maximize, bool isFocused)
        {
        }

        public override Color GetAccentColor()
        {
            return _fg2;
        }

        public override SpriteFont GetFont(TextFontStyle style)
        {
            return _font;
        }

        public override Color GetFontColor(TextFontStyle style)
        {
            return _fg1;
        }

        public override Rectangle GetTitleButtonRectangle(TitleButton button, int windowWidth, int windowHeight)
        {
            return Rectangle.Empty;
        }

        public override void LoadThemeData(GraphicsDevice device, ContentManager content)
        {
            _font = content.Load<SpriteFont>("EngineFont");
        }

        public override void UnloadThemeData()
        {
        }
    }
}
