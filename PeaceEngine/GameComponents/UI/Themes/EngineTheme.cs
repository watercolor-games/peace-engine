using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Interfaces;

namespace Plex.Engine.GameComponents.UI.Themes
{
    public class EngineTheme : Theme, ILoadable
    {
        private SpriteFont _font = null;

        public override Color GetButtonTextColor(UIButtonState state, ButtonStyle style)
        {
            return Color.White;
        }

        public override void DrawBlankControlArea(GraphicsContext gfx)
        {
            gfx.FillRectangle(0, 0, gfx.Width, gfx.Height, Color.GhostWhite);
        }

        public override void DrawButtonBackground(GraphicsContext gfx, UIButtonState state, ButtonStyle style)
        {
            gfx.Clear(Color.Gray);
        }

        public override void DrawPanel(GraphicsContext gfx, PanelStyles style)
        {
            var color = Color.GhostWhite;
            switch (style)
            {
                case PanelStyles.Dark:
                    color = color.Darken(0.5F);
                    break;
                case PanelStyles.Light:
                    color = Color.White;
                    break;
            }

            gfx.FillRectangle(0, 0, gfx.Width, gfx.Height, color);
        }

        public override void DrawToolTip(GraphicsContext gfx, string text)
        {
            gfx.Clear(Color.Black);
            gfx.FillRectangle(2, 2, gfx.Width - 4, gfx.Height - 4, Color.Gray);
            gfx.DrawString(_font, text, new Vector2(TooltipPaddingX, TooltipPaddingY), Color.White);
        }

        public override SpriteFont GetFont(TextStyle style)
        {
            return _font;
        }

        public void Load(ContentManager content)
        {
            _font = content.Load<SpriteFont>("EngineFont");
        }
    }
}
