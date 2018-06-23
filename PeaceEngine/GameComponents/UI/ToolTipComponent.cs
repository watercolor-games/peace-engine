using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Plex.Engine.GraphicsSubsystem;

namespace Plex.Engine.GameComponents.UI
{
    public sealed class ToolTipComponent : GameComponent
    {
        public string Text { get; set; } = "";

        private string _wrapped = "";

        public Themes.Theme Theme { get; set; }

        protected override void OnDraw(GameTime time, GraphicsContext gfx)
        {
            if (string.IsNullOrWhiteSpace(_wrapped))
                return;

            Theme.DrawToolTip(gfx, _wrapped);
        }

        protected override void OnUpdate(GameTime time)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return;

            _wrapped = TextRenderer.WrapText(Theme.GetFont(Themes.TextStyle.ToolTip), Text, Scene.Width / 2, TextRenderers.WrapMode.Words);
            var measure = Theme.GetFont(Themes.TextStyle.ToolTip).MeasureString(_wrapped);
            Width = (int)measure.X + (Theme.TooltipPaddingX * 2);
            Height = (int)measure.Y + (Theme.TooltipPaddingY * 2);

            X = MathHelper.Clamp(X, 0, Scene.Width - Width);
            Y = MathHelper.Clamp(Y, 0, Scene.Height - Height);


        }
    }
}
