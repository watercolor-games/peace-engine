using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Interfaces;
using Plex.Engine.TextRenderers;
using Plex.Engine.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.Cutscene
{
    /// <summary>
    /// Represents a Peace engine cutscene.
    /// </summary>
    public abstract class Cutscene : EntityContainer, ILoadable
    {
        [Dependency]
        private ThemeManager _theme = null;

        [Dependency]
        private CutsceneManager _cutscene = null;

        private bool _hasFinished = false;

        /// <summary>
        /// Retrieves the name of the cutscene.
        /// </summary>
        public abstract string Name { get; }

        internal bool IsFinished
        {
            get
            {
                return _hasFinished;
            }
            set
            {
                _hasFinished = value;
            }
        }

        /// <summary>
        /// Notify the cutscene manager that the cutscene is finished, stopping the cutscene from being played.
        /// </summary>
        public void NotifyFinished()
        {
            _hasFinished = true;
            _cutscene.Stop();
        }

        /// <inheritdoc/>
        public virtual void Load(ContentManager content) { }
        /// <summary>
        /// Fire a cutscene finish event.
        /// </summary>
        public virtual void OnFinish() { }
        /// <summary>
        /// Fire a cutscene play event.
        /// </summary>
        public virtual void OnPlay() { }

        public override void Draw(GameTime time, GraphicsContext gfx)
        {
            string placeholder = "Placeholder";
            string desc = $"Coded cutscene: {Name}";

            var header = _theme.Theme.GetFont(TextFontStyle.Header1);
            var highlight = _theme.Theme.GetFont(TextFontStyle.Highlight);

            var headColor = _theme.Theme.GetFontColor(TextFontStyle.Header1);
            var highlightColor = _theme.Theme.GetFontColor(TextFontStyle.Highlight);


            var pMeasure = header.MeasureString(placeholder);
            var dMeasure = highlight.MeasureString(desc);

            float centerY = (gfx.Height - (pMeasure.Y + 10 + dMeasure.Y)) / 2;

            gfx.BeginDraw();

            _theme.Theme.DrawControlBG(gfx, 0, 0, gfx.Width, gfx.Height);

            gfx.DrawString(placeholder, new Vector2((gfx.Width - pMeasure.X) / 2, centerY), headColor, header, TextAlignment.Left, int.MaxValue, WrapMode.None);
            gfx.DrawString(desc, new Vector2((gfx.Width - dMeasure.X) / 2, centerY + pMeasure.Y + 10), highlightColor, highlight, TextAlignment.Left, int.MaxValue, WrapMode.None);

            gfx.EndDraw();
        }
    }
}
