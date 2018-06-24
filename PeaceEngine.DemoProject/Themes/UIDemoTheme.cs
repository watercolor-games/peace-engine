using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Plex.Engine;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GameComponents.UI.Themes;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaceEngine.DemoProject.Themes
{
    //Themes are a way to personalize the look and feel of the Peace Engine UI.
    //Themes are a collection of overridable layout properties and rendering functions.
    public class UIDemoTheme : Theme, ILoadable
    {
        private Texture2D _check = null;

        private Color _controlRegular = new Color(0x3F, 0x3F, 0x3F, 0xFF);
        private Color _controlPrimary = new Color(0x1B, 0xAA, 0xF7, 0xFF);
        private Color _controlDanger = new Color(0xF7, 0x1B, 0x1B, 0xFF);
        private Color _controlWarning = new Color(0xF7, 0x94, 0x1B, 0xFF);
        private Color _controlDark = new Color(0x22, 0x22, 0x22, 0xFF);
        private Color _controlLight = new Color(96, 96, 96, 0xFF);
        private Color _controlSuccess = new Color(0x2C, 0xD3, 0x1D, 0xFF);
        private Color _tooltipTextColor = Color.White;

        private SpriteFont _regularFont = null;
        private SpriteFont _highlightFont = null;
        private SpriteFont _muteFont = null;
        private SpriteFont _monoFont = null;
        private SpriteFont _head1Font = null;
        private SpriteFont _head2Font = null;
        private SpriteFont _head3Font = null;

        public override int ButtonPaddingX => 14;
        public override int ButtonPaddingY => 8;

        public override int TooltipPaddingY => 2;
        public override int TooltipPaddingX => 2;


        private Color _buttonText = Color.White;

        public override void DrawCheckBox(GraphicsContext gfx, bool check, bool containsMouse)
        {
            gfx.Clear(Color.White);
            gfx.FillRectangle(2, 2, gfx.Width - 4, gfx.Height - 4, (containsMouse) ? _controlLight : _controlDark);
            if (check)
            {
                gfx.FillRectangle(0, 0, gfx.Width, gfx.Height, _check, Color.White);
            }
        }

        public override void DrawBlankControlArea(GraphicsContext gfx)
        {
            gfx.Clear(_controlRegular);
        }

        public override void DrawButtonBackground(GraphicsContext gfx, UIButtonState state, ButtonStyle style)
        {
            int rounding = ButtonPaddingY / 2;
            var color = _controlLight;
            switch (style)
            {
                case ButtonStyle.Danger:
                    color = _controlDanger;
                    break;
                case ButtonStyle.Primary:
                    color = _controlPrimary;
                    break;
                case ButtonStyle.Success:
                    color = _controlSuccess;
                    break;
                case ButtonStyle.Warning:
                    color = _controlWarning;
                    break;
            }
            if (state == UIButtonState.Hover)
                color = color.Lighten(0.25F);
            else if (state == UIButtonState.Pressed)
                color = color.Darken(0.25F);

            gfx.FillRoundedRectangle(0, 0, gfx.Width, gfx.Height, rounding, color);
        }

        public override void DrawPanel(GraphicsContext gfx, PanelStyles style)
        {
            switch (style)
            {
                case PanelStyles.Dark:
                    gfx.Clear(_controlDark);
                    break;
                case PanelStyles.Default:
                    gfx.Clear(_controlRegular);
                    break;
                case PanelStyles.Light:
                    gfx.Clear(_controlLight);
                    break;
            }
        }

        public override void DrawToolTip(GraphicsContext gfx, string text)
        {
            gfx.FillRoundedRectangle(0, 0, gfx.Width, gfx.Height, TooltipPaddingY, _controlDark);
            gfx.DrawString(GetFont(TextStyle.ToolTip), text, new Vector2(TooltipPaddingX, TooltipPaddingY), _tooltipTextColor);
        }

        public override Color GetButtonTextColor(UIButtonState state, ButtonStyle style)
        {
            return _buttonText;
        }

        public override SpriteFont GetFont(TextStyle style)
        {
            switch(style)
            {
                case TextStyle.Button:
                    return _regularFont;
                case TextStyle.Heading1:
                    return _head1Font;
                case TextStyle.Heading2:
                    return _head2Font;
                case TextStyle.Heading3:
                    return _head3Font;
                case TextStyle.Highlight:
                    return _highlightFont;
                case TextStyle.ListItem:
                    return _regularFont;
                case TextStyle.Monospace:
                    return _monoFont;
                case TextStyle.Mute:
                    return _muteFont;
                case TextStyle.ToolTip:
                    return _regularFont;
                default:
                    return _regularFont;
            }
        }

        public override Color GetTextColor(TextStyle style)
        {
            return Color.White;
        }

        public void Load(ContentManager content)
        {
            _regularFont = content.Load<SpriteFont>("Fonts/Regular");
            _monoFont = content.Load<SpriteFont>("Fonts/Monospace");
            _highlightFont = content.Load<SpriteFont>("Fonts/Highlight");
            _muteFont = content.Load<SpriteFont>("Fonts/Mute");
            _head1Font = content.Load<SpriteFont>("Fonts/Heading1");
            _head2Font = content.Load<SpriteFont>("Fonts/Heading2");
            _head3Font = content.Load<SpriteFont>("Fonts/Heading3");

            _check = content.Load<Texture2D>("Textures/check");
        }

        public override void DrawTextBoxBackground(GraphicsContext gfx, UIButtonState state)
        {
            gfx.Clear(_controlDark);

            var barColor = _controlLight;
            if (state == UIButtonState.Hover)
                barColor = barColor.Lighten(0.25F);
            else if (state == UIButtonState.Pressed)
                barColor = _controlPrimary;

            gfx.FillRectangle(0, gfx.Height - 2, gfx.Width, 2, barColor);
        }

        public override void DrawTextBoxLabel(GraphicsContext gfx, string label)
        {
            gfx.DrawString(_regularFont, label, new Vector2(2, 2), _controlLight);
        }

        public override void DrawTextBoxText(GraphicsContext gfx, string text, int xOffset)
        {
            gfx.DrawString(_regularFont, text, new Vector2(2 - xOffset, 2), Color.White);
        }

        public override void DrawTextBoxCaret(GraphicsContext gfx, int xOffset)
        {
            int height = _regularFont.LineSpacing;
            int y = (gfx.Height - height) / 2;
            int x = 2 + xOffset;
            gfx.FillRectangle(x, y, 2, height, Color.White);
        }

        public override void DrawTextEditorBackground(GraphicsContext gfx)
        {
            gfx.Clear(_controlLight);
            gfx.FillRectangle(1, 1, gfx.Width - 2, gfx.Height - 2, _controlDark);
        }

        public override void DrawHoveredHighlight(GraphicsContext gfx, Rectangle region)
        {
            gfx.FillRoundedRectangle(region, 3, _controlPrimary * 0.5F);
        }

        public override void DrawSelectedHighlight(GraphicsContext gfx, Rectangle region)
        {
            gfx.FillRoundedRectangle(region, 3, _controlPrimary);
        }

        public override void DrawProgressBar(GraphicsContext gfx, float value)
        {
            int rounding = 4;
            gfx.FillRoundedRectangle(0, 0, gfx.Width, gfx.Height, rounding, _controlLight);
            gfx.FillRoundedRectangle(1, 1, gfx.Width - 2, gfx.Height - 2, rounding, Color.Black);

            int progressWidth = (int)MathHelper.Lerp(0, gfx.Width - 2, value);
            rounding = Math.Min(progressWidth/2, rounding);

            gfx.FillRoundedRectangle(1, 1, progressWidth, gfx.Height - 2, rounding, _controlPrimary);


        }

        public override void DrawScrollBar(GraphicsContext gfx, Hitbox upArrow, Hitbox downArrow, Hitbox scrollNub)
        {
            gfx.Clear(_controlDark);

            var accent = _controlPrimary;

            gfx.FillRectangle(upArrow.X, upArrow.Y, upArrow.Width, upArrow.Height, _controlLight);
            gfx.FillRectangle(downArrow.X, downArrow.Y, downArrow.Width, downArrow.Height, _controlLight);

            gfx.FillRectangle(scrollNub.X, scrollNub.Y, scrollNub.Width, scrollNub.Height, accent);

        }
    }
}
