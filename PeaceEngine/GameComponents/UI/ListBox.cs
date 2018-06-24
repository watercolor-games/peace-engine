using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.UI
{
    public class ListBox : Control
    {
        private const int _hpad = 7;
        private const int _vpad = 2;
        private const int _itemPad = 2;
        private const int _highlightPad = 4;

        private int _hovered = -1;
        private int _selected = -1;

        public event EventHandler SelectedIndexChanged;

        public readonly List<object> Items = new List<object>();

        public bool AutoSize { get; set; } = false;

        public int SelectedIndex
        {
            get
            {
                return _selected;
            }
            set
            {
                value = MathHelper.Clamp(value, -1, Items.Count - 1);
                if (value != _selected)
                {
                    _selected = value;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public object SelectedItem
        {
            get
            {
                if (_selected == -1)
                    return null;
                return Items[_selected];
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _hovered = -1;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int itemHeight = (_itemPad * 2) + (int)Theme.GetFont(Themes.TextStyle.ListItem).MeasureString("#").Y;
            int width = Width - (_highlightPad * 2);

            if (e.Position.X < _highlightPad || e.Position.X >= _highlightPad + width || e.Position.Y < _vpad)
            {
                _hovered = -1;
                return;
            }

            for (int i = 0; i < Items.Count; i++)
            {
                int y = _vpad + (itemHeight * i);
                if (e.Position.Y >= y && e.Position.Y <= y + itemHeight)
                {
                    _hovered = i;
                    return;
                }
            }

            _hovered = -1;

            base.OnMouseMove(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            SelectedIndex = _hovered;
            base.OnClick(e);
        }

        protected override void OnUpdate(GameTime time)
        {
            if (AutoSize)
            {
                var font = Theme.GetFont(Themes.TextStyle.ListItem);
                int itemHeight = (_itemPad * 2) + (int)Theme.GetFont(Themes.TextStyle.ListItem).MeasureString("#").Y;
                var totalItemHeight = (itemHeight * Items.Count);
                Height = (_vpad * 2) + totalItemHeight;
            }

            base.OnUpdate(time);
        }

        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            int itemHeight = (_itemPad * 2) + (int)Theme.GetFont(Themes.TextStyle.ListItem).MeasureString("#").Y;
            int width = Width - (_highlightPad * 2);
            for (int i = 0; i < Items.Count; i++)
            {
                string itemText = Items[i].ToString();
                int x = _hpad;
                int y = _vpad + (itemHeight * i);
                bool selected = i == _selected;
                bool hovered = i == _hovered;

                var foreground = Theme.GetTextColor(Themes.TextStyle.ListItem);
                
                if (selected)
                {
                    Theme.DrawSelectedHighlight(gfx, new Rectangle(0, y, Width, itemHeight));
                }
                else if (hovered)
                {
                    Theme.DrawHoveredHighlight(gfx, new Rectangle(0, y, Width, itemHeight));
                }

                gfx.DrawString(Theme.GetFont(Themes.TextStyle.ListItem), itemText, new Vector2(x, y + _itemPad), foreground);
            }
        }

    }
}
