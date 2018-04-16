using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Interfaces;

namespace Plex.Engine
{
    /// <summary>
    /// A class for rendering text.
    /// </summary>
    public class TextRenderer : IEngineComponent
    {
        [Dependency]
        private UIManager _ui = null;

        public void Initiate() { }

        private string WrapLine(SpriteFont font, string text, float maxLineWidth)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            if (font.MeasureString(text).X <= maxLineWidth)
                return text;
            if (font.MeasureString(font.Characters.OrderBy(x => font.MeasureString(x.ToString()).X).Last().ToString()).X >= maxLineWidth)
                throw new InvalidOperationException("Max line width must be greater than the width of the largest character in the font.");
            text = text.Trim();
            var sb = new StringBuilder();

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Perform text wrapping on a string of text using the specified <see cref="SpriteFont"/>.
        /// </summary>
        /// <param name="font">The <see cref="SpriteFont"/> representing the font to measure the text in.</param>
        /// <param name="text">The text to wrap.</param>
        /// <param name="maxLineWidth">The maximum width (in pixels) that a line of text can be.</param>
        /// <param name="mode">The type of text wrapping to apply.</param>
        /// <returns>The resulting wrapped text.</returns>
        public string WrapText(SpriteFont font, string text, float maxLineWidth, WrapMode mode)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            if (font.MeasureString(text).X <= maxLineWidth)
                return text;
            if (font.MeasureString(font.Characters.OrderBy(x => font.MeasureString(x.ToString()).X).Last().ToString()).X >= maxLineWidth)
                return text;
                if (mode == WrapMode.Words)
            {
                float spacewidth = font.MeasureString(" ").X;
                if (maxLineWidth < spacewidth)
                    return text;
                text = text.Trim().Replace("\r", "");
                string[] lines = text.Split('\n');
                float lineWidth = 0;
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    lineWidth = 0;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    var words = line.Split(' ').ToList();
                    for (int i = 0; i < words.Count; i++)
                    {
                        string word = words[i];

                        Vector2 size = font.MeasureString(word);

                        if (lineWidth + size.X <= maxLineWidth)
                        {

                            sb.Append(word + " ");
                            lineWidth += size.X + spacewidth;
                        }
                        else
                        {
                            if (size.X >= maxLineWidth)
                            {
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                int half = word.Length / 2;
                                string first = word.Substring(0, half);
                                string second = word.Substring(half);
                                words[i] = first;
                                words.Insert(i + 1, second);
                                i--;
                                continue;
                            }
                            else
                            {
                                sb.Append("\n" + word + " ");
                                lineWidth = size.X + spacewidth;
                            }
                        }
                    }

                }
                return sb.ToString().TrimEnd();
            }
            else
            {
                float lineWidth = 0;
                string newstr = "";
                foreach (char c in text)
                {
                    var measure = font.MeasureString(c.ToString());
                    if (lineWidth + measure.X > maxLineWidth)
                    {
                        newstr += "\n";
                        lineWidth = measure.X;
                    }
                    else
                    {
                        if (c == '\n')
                        {
                            lineWidth = 0;
                        }
                        else
                            lineWidth += measure.X;
                    }
                    newstr += c;
                }
                return newstr;
            }

        }

        /// <summary>
        /// Measure a string of text to get its width and height in pixels.
        /// </summary>
        /// <param name="text">The text to measure</param>
        /// <param name="font">The font to measure with</param>
        /// <param name="maxwidth">The maximum width text can be before it is wrapped</param>
        /// <param name="wrapMode">The wrap mode to use</param>
        /// <returns>The size in pixels of the text.</returns>
        public Vector2 MeasureText(string text, SpriteFont font, float maxwidth, WrapMode wrapMode)
        {
            if (string.IsNullOrEmpty(text))
                return Vector2.Zero;
            switch (wrapMode)
            {
                case WrapMode.None:
                    return font.MeasureString(text) * _ui.GUIScale;
                default:
                    return font.MeasureString(WrapText(font, text, maxwidth, wrapMode));
            }
        }

        /// <summary>
        /// Render a string of text.
        /// </summary>
        /// <param name="gfx">The graphics context to render the text to.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="x">The X coordinate of the text.</param>
        /// <param name="y">The Y coordinate of the text.</param>
        /// <param name="font">The font to render the text in.</param>
        /// <param name="maxwidth">The maximum width text can be before it is wrapped.</param>
        /// <param name="alignment">The alignment of the text.</param>
        /// <param name="wrapMode">The type of text wrapping to use.</param>
        /// <param name="color">The color of the text to render</param>
        public void DrawText(SpriteBatch batch, int x, int y, string text, SpriteFont font, Color color, float maxwidth, TextAlignment alignment, WrapMode wrapMode)
        {
            if (string.IsNullOrEmpty(text))
                return;
            string measured = (wrapMode == WrapMode.None) ? text : WrapText(font, text, maxwidth, wrapMode);
            string[] lines = measured.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var measure = font.MeasureString(line);
                switch (alignment)
                {
                    case TextAlignment.Left:
                        batch.DrawString(font, line, new Vector2(x, y + ((measure.Y * i)*_ui.GUIScale)), color, 0, Vector2.Zero, _ui.GUIScale, SpriteEffects.None, 0);
                        break;
                    case TextAlignment.Center:
                        batch.DrawString(font, line, new Vector2((x + ((maxwidth - measure.X) / 2)*_ui.GUIScale), y + ((measure.Y * i)*_ui.GUIScale)), color, 0, Vector2.Zero, _ui.GUIScale, SpriteEffects.None, 0);
                        break;
                    case TextAlignment.Right:
                        batch.DrawString(font, line, new Vector2(x + ((maxwidth - measure.X)/2), y + ((measure.Y * i)*_ui.GUIScale)), color, 0, Vector2.Zero, _ui.GUIScale, SpriteEffects.None, 0);
                        break;
                }
            }

        }


    }

    /// <summary>
    /// Describes how text should be aligned when rendered.
    /// </summary>
    public enum TextAlignment
    {
        /// <summary>
        /// Text should be aligned to the centre of the render bounds.
        /// </summary>
        Center = 0,
        /// <summary>
        /// Text should be aligned to the left.
        /// </summary>
        Left = 1,
        /// <summary>
        /// Text should be rendered to the right.
        /// </summary>
        Right = 2,
    }

    public enum WrapMode
    {
        None,
        Letters,
        Words
    }
}
