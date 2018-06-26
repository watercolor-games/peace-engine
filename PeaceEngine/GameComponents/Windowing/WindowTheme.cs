using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.Windowing
{
    public abstract class WindowTheme
    {
        public virtual int TitleHeight => 30;
        public virtual int BorderSize => 2;

        public abstract Rectangle GetButtonRect(TitleButton button, int windowWidth);
        public abstract void DrawWindowButton(GraphicsContext gfx, TitleButton button, Hitbox hitbox);
        public abstract void DrawWindowFrame(GraphicsContext gfx, string titleText);
    }

    public enum TitleButton
    {
        Close,
        Minimize,
        Rollup,
        Maximize
    }

    public class EngineWindowTheme : WindowTheme, ILoadable
    {
        private SpriteFont _engineFont = null;

        public override void DrawWindowButton(GraphicsContext gfx, TitleButton button, Hitbox hitbox)
        {
            gfx.FillRectangle(new Rectangle(hitbox.X, hitbox.Y, hitbox.Width, hitbox.Height), Color.Black);
        }

        public override void DrawWindowFrame(GraphicsContext gfx, string titleText)
        {
            gfx.FillRectangle(0, 0, gfx.Width, TitleHeight, Color.Gray);
            gfx.FillRectangle(0, TitleHeight, BorderSize, gfx.Height - TitleHeight, Color.Gray);
            gfx.FillRectangle(gfx.Width - BorderSize, TitleHeight, BorderSize, gfx.Height - TitleHeight, Color.Gray);
            gfx.FillRectangle(BorderSize, gfx.Height - BorderSize, gfx.Width - (BorderSize * 2), BorderSize, Color.Gray);

            if(!string.IsNullOrWhiteSpace(titleText))
            {
                var measure = _engineFont.MeasureString(titleText);
                var titleX = BorderSize*4;
                var titleY = (TitleHeight - measure.Y) / 2;
                gfx.DrawString(_engineFont, titleText, new Vector2(titleX, titleY), Color.White);

            }
        }

        public override Rectangle GetButtonRect(TitleButton button, int windowWidth)
        {
            int buttonSize = 24;
            int buttonY = (TitleHeight - buttonSize) / 2;
            int paddingFromRight = BorderSize;
            switch (button)
            {
                case TitleButton.Close:
                    return new Rectangle(windowWidth - (paddingFromRight + buttonSize), buttonY, buttonSize, buttonSize);
                case TitleButton.Maximize:
                    return new Rectangle(windowWidth - ((paddingFromRight + buttonSize) * 2), buttonY, buttonSize, buttonSize);
                case TitleButton.Minimize:
                    return new Rectangle(windowWidth - ((paddingFromRight + buttonSize) * 3), buttonY, buttonSize, buttonSize);
                default:
                    return Rectangle.Empty;
            }
        }

        public void Load(ContentManager content)
        {
            _engineFont = content.Load<SpriteFont>("EngineFont");
        }
    }
}
