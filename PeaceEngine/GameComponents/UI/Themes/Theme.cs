using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.UI.Themes
{
    public abstract class Theme
    {
        public virtual int TooltipPaddingX { get; } = 7;
        public virtual int TooltipPaddingY { get; } = 7;

        public virtual int ButtonPaddingY { get; } = 4;
        public virtual int ButtonPaddingX { get; } = 7;
        public virtual int ButtonIconSize { get; } = 16;
        public virtual int ButtonImageMargin { get; } = 3;
        public virtual int CheckBoxSize => 16;
        public virtual int TextBoxVerticalPadding => 2;
        public virtual int ScrollbarSize => 17;

        public virtual double CaretBlinkMS => 250;

        public abstract Color GetButtonTextColor(UIButtonState state, ButtonStyle style);
        public abstract Color GetTextColor(TextStyle style);
        public abstract void DrawTextEditorBackground(GraphicsContext gfx);
        public abstract void DrawCheckBox(GraphicsContext gfx, bool check, bool containsMouse);
        public abstract void DrawTextBoxLabel(GraphicsContext gfx, string label);
        public abstract void DrawTextBoxText(GraphicsContext gfx, string text, int xOffset);
        public abstract void DrawTextBoxCaret(GraphicsContext gfx, int xOffset);
        public abstract void DrawTextBoxBackground(GraphicsContext gfx, UIButtonState state);
        public abstract void DrawBlankControlArea(GraphicsContext gfx);
        public abstract void DrawPanel(GraphicsContext gfx, PanelStyles style);
        public abstract SpriteFont GetFont(TextStyle style);
        public abstract void DrawToolTip(GraphicsContext gfx, string text);
        public abstract void DrawButtonBackground(GraphicsContext gfx, UIButtonState state, ButtonStyle style);
        public abstract void DrawHoveredHighlight(GraphicsContext gfx, Rectangle region);
        public abstract void DrawSelectedHighlight(GraphicsContext gfx, Rectangle region);
        public abstract void DrawProgressBar(GraphicsContext gfx, float value);
        public abstract void DrawScrollBar(GraphicsContext gfx, Hitbox up, Hitbox down, Hitbox nub);
    }
}
