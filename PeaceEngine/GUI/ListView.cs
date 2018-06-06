using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GUI
{
    public abstract class ListView : Control
    {
        private int _selected = -1;
        private int _tracked = -1;
        private readonly Dictionary<string, Texture2D> _images = new Dictionary<string, Texture2D>();

        public readonly List<ListViewItem> Items = new List<ListViewItem>();
        
        public ListViewItem SelectedItem => (_selected == -1) ? null : Items[_selected];

        public int TrackedIndex => _tracked;

        public int SelectedIndex
        {
            get
            {
                return _selected;
            }
            set
            {
                value = MathHelper.Clamp(value, -1, Items.Count - 1);
                if(_selected!=value)
                {
                    _selected = value;
                    OnSelectedIndexChanged(EventArgs.Empty);
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler SelectedIndexChanged;

        public bool AutoSize { get; set; } = false;


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var rects = GetItemRects();
            var rect = rects.FirstOrDefault(x => e.Position.X >= x.X && e.Position.Y >= x.Y && e.Position.X <= x.X + x.Width && e.Position.Y <= x.Y + x.Height);
            if (rect == Rectangle.Empty)
                _tracked = -1;
            else
                _tracked = Array.IndexOf(rects, rect);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            _tracked = -1;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            SelectedIndex = _tracked;
        }

        public void SetImage(string key, Texture2D texture)
        {
            if (key == null)
                return;

            if(texture == null)
            {
                if (_images.ContainsKey(key))
                    _images.Remove(key);
                return;
            }
            if (_images.ContainsKey(key))
                _images[key] = texture;
            else
                _images.Add(key, texture);
        }

        public Texture2D GetImage(string key)
        {
            if (key == null)
                return null;
            if (!_images.ContainsKey(key))
                return null;
            return _images[key];
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e) { }

        protected override void OnPaint(GameTime time, GraphicsContext gfx) { }

        protected abstract Rectangle[] GetItemRects();
        protected abstract void CalculateSize();

        protected override void OnUpdate(GameTime time)
        {
            if(AutoSize)
            {
                CalculateSize();
            }
            base.OnUpdate(time);
        }
    }

    public class ListViewItem
    {
        public string Text { get; set; }
        public object Tag { get; set; }
        public string ImageKey { get; set; }

        public ListViewItem(string text, string image, object tag)
        {
            Text = text;
            Tag = tag;
            ImageKey = image;
        }
    }
}
