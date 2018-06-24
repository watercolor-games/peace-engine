using Microsoft.Xna.Framework;
using Plex.Engine.GraphicsSubsystem;
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

        public abstract void DrawWindowFrame(GraphicsContext gfx, string titleText);
    }

    public class EngineWindowTheme : WindowTheme
    {
        public override void DrawWindowFrame(GraphicsContext gfx, string titleText)
        {
            gfx.FillRectangle(0, 0, gfx.Width, TitleHeight, Color.Gray);
            gfx.FillRectangle(0, TitleHeight, BorderSize, gfx.Height - TitleHeight, Color.Gray);
            gfx.FillRectangle(gfx.Width - BorderSize, TitleHeight, BorderSize, gfx.Height - TitleHeight, Color.Gray);
            gfx.FillRectangle(BorderSize, gfx.Height - BorderSize, gfx.Width - (BorderSize * 2), BorderSize, Color.Gray);
        }
    }
}
