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

        public override Color GetTextColor(TextStyle style)
        {
            return (style == TextStyle.Button) ? Color.White : Color.Black;
        }

        public override void DrawCheckBox(GraphicsContext gfx, bool check, bool containsMouse)
        {
            var fg = Color.Black;
            if (containsMouse)
                fg = Color.Gray;

            gfx.Clear(fg);
            gfx.FillRectangle(2, 2, gfx.Width - 4, gfx.Height - 4, Color.White);

            if (check)
            {
                gfx.DrawLine(Vector2.Zero, new Vector2(gfx.Width, gfx.Height), 2, fg);
                gfx.DrawLine(new Vector2(0, gfx.Height), new Vector2(gfx.Width, 0), 2, fg);
            }
        }

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

        public override void DrawTextBoxBackground(GraphicsContext gfx, UIButtonState state)
        {
            gfx.Clear(Color.Black);
            gfx.FillRectangle(2, 2, gfx.Width - 4, gfx.Height - 4, Color.White);
        }

        public override void DrawTextBoxLabel(GraphicsContext gfx, string label)
        {
            gfx.DrawString(_font, label, new Vector2(2, 2), Color.Gray);
        }

        public override void DrawTextBoxText(GraphicsContext gfx, string text, int xOffset)
        {
            gfx.DrawString(_font, text, new Vector2(2 - xOffset, 2), Color.Black);
        }

        public override void DrawTextBoxCaret(GraphicsContext gfx, int xOffset)
        {
            int height = _font.LineSpacing;
            int y = (gfx.Height - height) / 2;
            int x = 2 + xOffset;
            gfx.FillRectangle(x, y, 2, height, Color.Black);
        }

        public override void DrawTextEditorBackground(GraphicsContext gfx)
        {
            gfx.Clear(Color.Black);
            gfx.FillRectangle(1, 1, gfx.Width - 2, gfx.Height, Color.White);
        }

        public override void DrawHoveredHighlight(GraphicsContext gfx, Rectangle region)
        {
            gfx.FillRectangle(region, Color.Gray * 0.5F);
        }

        public override void DrawSelectedHighlight(GraphicsContext gfx, Rectangle region)
        {
            gfx.FillRectangle(region, Color.Gray);
        }

        public override void DrawProgressBar(GraphicsContext gfx, float value)
        {
            gfx.Clear(Color.Black);
            gfx.FillRectangle(2, 2, gfx.Width - 4, gfx.Height - 4, Color.White);
            gfx.FillRectangle(2, 2, (int)MathHelper.Lerp(0, gfx.Width - 4, value), gfx.Height - 4, Color.Gray);
        }

        public override void DrawScrollBar(GraphicsContext gfx, Hitbox up, Hitbox down, Hitbox nub)
        {
            gfx.Clear(Color.White);
            gfx.FillRectangle(up.Bounds, Color.Gray);
            gfx.FillRectangle(down.Bounds, Color.Gray);
            gfx.FillRectangle(nub.Bounds, Color.Gray);
        }
    }
}
