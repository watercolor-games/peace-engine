using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plex.Engine.GraphicsSubsystem;

namespace Plex.Engine.GameComponents.UI
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
        private double _caretTime = 0;
        private bool _showCaret = true;
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
                
            }
        }

        private SpriteFont _drawFont
        {
            get
            {
                return Theme.GetFont(Themes.TextStyle.Regular);
            }
        }

        private string _lastText = "";

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            if(_caretTime<Theme.CaretBlinkMS)
            {
                _caretTime += time.ElapsedGameTime.TotalMilliseconds;
            }
            else
            {
                _caretTime = 0;
                _showCaret = !_showCaret;
            }

            string displayText = (_hasPassword) ? "*".Repeat(_text.Length) : _text;

            if (_lastText != displayText)
            {
                _lastText = displayText;
                TextChanged?.Invoke(this, EventArgs.Empty);
            }

            var hashMeasure = _drawFont.MeasureString("#");
            Height = Math.Max((int)hashMeasure.Y + (Theme.TextBoxVerticalPadding * 2), Height);

            if (string.IsNullOrEmpty(displayText))
            {
                if (_drawOffset != 0)
                {
                    _drawOffset = 0;
                }
            }
            string toCaret = displayText.Substring(0, _index);
            var measure = _drawFont.MeasureString(toCaret);
            if (_caretX != (int)measure.X)
            {
                _caretX = (int)measure.X;
            }

            //calculate offset
            int realCaretX = _caretX - _drawOffset;
            if (realCaretX > Width - 4)
            {
                _drawOffset = _caretX - (Width - 4);
            }
            else if (realCaretX < 0)
            {
                _drawOffset = _caretX + (Width - 4);
            }
            base.OnUpdate(time);
        }

        /// <inheritdoc/>
        protected override void OnKeyEvent(KeyboardEventArgs e)
        {
            _caretTime = 0;
            _showCaret = true;
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
            {
                return;
            }
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Left)
            {
                if (_index > 0)
                {
                    _index--;
                }
                return;
            }
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Right)
            {
                if (_index < _text.Length)
                {
                    _index++;
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
                    }
                    return;
                }
                _text = _text.Insert(_index, e.Character.ToString());
                _index++;
            }
            base.OnKeyEvent(e);
        }

        /// <inheritdoc/>
        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            UIButtonState state = UIButtonState.Idle;
            if (HasFocused)
                state = UIButtonState.Pressed;
            else if (ContainsMouse)
                state = UIButtonState.Hover;
            Theme.DrawTextBoxBackground(gfx, state);

            if(state != UIButtonState.Pressed)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    if(!string.IsNullOrWhiteSpace(Label))
                    {
                        Theme.DrawTextBoxLabel(gfx, Label);
                    }
                }
            }

            if(!string.IsNullOrWhiteSpace(Text))
            {
                Theme.DrawTextBoxText(gfx, Text, _drawOffset);
            }

            if(HasFocused)
            {
                if(_showCaret)
                    Theme.DrawTextBoxCaret(gfx, _caretX - _drawOffset);
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
            for (int i = 0; i < amount; i++)
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
