﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using Microsoft.Xna.Framework.Graphics;

namespace Plex.Engine.GUI
{
    /// <summary>
    /// A control which contains editable text.
    /// </summary>
    public class TextBox : Control
    {
        private string _text = "";
        private string _label = "This is a text box.";
        private int _index = 0;
        private int _drawOffset = 0;
        private int _caretX = 0;
        private bool _hasPassword = false;

        /// <summary>
        /// Occurs when the text in the text box is changed.
        /// </summary>
        public event EventHandler TextChanged;

        /// <summary>
        /// Gets or sets whether text should be masked as dots to protect over-the-shoulder snooping of passwords.
        /// </summary>
        public bool HasPassword
        {
            get
            {
                return _hasPassword;
            }
            set
            {
                if (_hasPassword == value)
                    return;
                _hasPassword = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the text of the text box.
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                _index = (int)MathHelper.Clamp(_index, 0, _text.Length);
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the text to be displayed in the text box when it is empty and out of focus.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                if (_label == value)
                    return;
                _label = value;
                Invalidate(true);
            }
        }

        private SpriteFont _drawFont
        {
            get
            {
                return Theme.GetFont(Themes.TextFontStyle.System);
            }
        }

        private string _lastText = "";

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            string displayText = (_hasPassword) ? "*".Repeat(_text.Length) : _text;

            if(_lastText != displayText)
            {
                _lastText = displayText;
                TextChanged?.Invoke(this, EventArgs.Empty);
            }

            var hashMeasure = TextRenderer.MeasureText("#", _drawFont, int.MaxValue, TextRenderers.WrapMode.None);
            Height = Math.Max((int)hashMeasure.Y + 4, Height);

            if (string.IsNullOrEmpty(displayText))
            {
                if (_drawOffset != 0)
                {
                    _drawOffset = 0;
                    Invalidate(true);
                }
            }
            string toCaret = displayText.Substring(0, _index);
            var measure = TextRenderer.MeasureText(toCaret, _drawFont, int.MaxValue, TextRenderers.WrapMode.None);
            if (_caretX != (int)measure.X)
            {
                _caretX = (int)measure.X;
                Invalidate(true);
            }

            //calculate offset
            int realCaretX = _caretX - _drawOffset;
            if(realCaretX > Width-4)
            {
                _drawOffset = _caretX - (Width-4);
            }
            else if(realCaretX < 0)
            {
                _drawOffset = _caretX + (Width - 4);
            }
            base.OnUpdate(time);
        }

        /// <inheritdoc/>
        protected override void OnKeyEvent(KeyboardEventArgs e)
        {
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
            {
                return;
            }
            if(e.Modifiers.HasFlag(KeyboardModifiers.Control) && e.Key == Microsoft.Xna.Framework.Input.Keys.V)
            {
                string cbText = Manager.GetClipboardText();
                cbText = cbText?.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                if (!string.IsNullOrEmpty(cbText))
                {
                    Text = Text.Insert(_index, cbText);
                    _index += cbText.Length;
                }
                return;
            }
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.Left)
            {
                if(_index > 0)
                {
                    _index--;
                    Invalidate(true);
                }
                return;
            }
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Right)
            {
                if (_index < _text.Length)
                {
                    _index++;
                    Invalidate(true);
                }
                return;
            }
            if (e.Character != null)
            {
                if (e.Character == '\b')
                {
                    if (_index > 0)
                    {
                        _text = _text.Remove(_index - 1, 1);
                        _index--;
                        Invalidate(true);
                    }
                    return;
                }
                _text = _text.Insert(_index, e.Character.ToString());
                _index++;
                Invalidate(true);
            }
            base.OnKeyEvent(e);
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            Theme.DrawControlDarkBG(gfx, 0, 0, Width, Height);
            var colorBar = Color.Gray;
            if (ContainsMouse)
            {
                colorBar = Color.White;
            }
            if (IsFocused)
            {
                colorBar = Theme.GetAccentColor();
            }
            gfx.DrawRectangle(0, Height - 2, Width, 2, colorBar);
            if (string.IsNullOrWhiteSpace(_text))
            {
                if (!IsFocused)
                {
                    gfx.DrawString(_label, 2, 2, Color.Gray, _drawFont, TextAlignment.Left, int.MaxValue, TextRenderers.WrapMode.None);
                }
            }
            else
            {
                gfx.DrawString(_lastText, 2 - _drawOffset, 2, Color.White, _drawFont, TextAlignment.Left, int.MaxValue, TextRenderers.WrapMode.None);
                
            }
            if (IsFocused)
            {
                gfx.DrawRectangle(2 + (_caretX - _drawOffset), 4, 2, Height - 8, Color.White);
            }
        }

    }

    /// <summary>
    /// Simple string extensions that should really be in .NET by default but aren't
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Repeat a string for a specified amount of times
        /// </summary>
        /// <param name="str">The string to repeat</param>
        /// <param name="amount">The amount of times the string should be repeated</param>
        /// <returns>The resulting string of the repeat operation</returns>
        public static string Repeat(this string str, int amount)
        {
            string nstr = "";
            for(int i = 0; i < amount;i++)
            {
                nstr += str;
            }
            return nstr;
        }

        /// <summary>
        /// Truncate a string to fit under a certain length.
        /// </summary>
        /// <param name="value">The input string to truncate</param>
        /// <param name="maxLength">The allowed length for the string</param>
        /// <returns>The truncated string</returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Substring(0, Math.Min(value.Length, maxLength));
        }
    }
}
