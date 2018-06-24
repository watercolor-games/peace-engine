using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Plex.Engine.GameComponents.UI
{
    public class VStacker : Panel
    {
        protected override void OnUpdate(GameTime time)
        {
            DrawBackground = false;
            int y = 0;
            int x = 0;
            int w = 0;
            foreach(var child in Children)
            {
                if (AutoSize)
                {
                    child.X = x;
                    child.Y = y;
                    y += child.Height;
                }
                else
                {
                    w = Math.Max(w, child.Width);
                    if(y + child.Height >= Height)
                    {
                        y = 0;
                        x += w;
                        child.X = x;
                        child.Y = y;
                    }
                    else
                    {
                        child.Y = y;
                        child.X = x;
                        y += child.Height;
                    }

                }
            }
            base.OnUpdate(time);
        }
    }
}
