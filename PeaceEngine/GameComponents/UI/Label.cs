using Microsoft.Xna.Framework;
using Plex.Engine.GameComponents.UI.Themes;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.UI
{
    public class Label : Control
    {
        public bool AutoSize { get; set; } = true;
        public int AutoSizeMaxWidth { get; set; } = 0;
        public TextStyle TextStyle { get; set; } = TextStyle.Regular;
        public string Text { get; set; } = "";
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;

        private string _wrapped = null;

        protected override void OnUpdate(GameTime time)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return;

            var font = Theme.GetFont(TextStyle);


            if (AutoSize)
            {
                _wrapped = TextRenderer.WrapText(font, Text, AutoSizeMaxWidth, TextRenderers.WrapMode.Words);
                var measure = font.MeasureString(_wrapped);
                Width = (int)measure.X;
                Height = (int)measure.Y;
            }
            else
            {
                _wrapped = TextRenderer.WrapText(font, Text, Width, TextRenderers.WrapMode.Words);
            }
            base.OnUpdate(time);
        }

        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            if (string.IsNullOrWhiteSpace(_wrapped) || string.IsNullOrWhiteSpace(Text))
                return;

            var font = Theme.GetFont(TextStyle);

            gfx.DrawString(_wrapped, Vector2.Zero, Theme.GetTextColor(TextStyle), font, Alignment, Width, TextRenderers.WrapMode.None);
        }
    }
}
