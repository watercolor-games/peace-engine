using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents
{
    public abstract class GameComponent
    {
        public class SceneComponentCollection : ICollection<GameComponent>
        {
            private GameScene _scene = null;
            private List<GameComponent> _list = new List<GameComponent>();

            internal SceneComponentCollection(GameScene owner)
            {
                _scene = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public int Count => _list.Count;

            public bool IsReadOnly => true;

            public void Add(GameComponent item)
            {
                if (item.Scene == _scene)
                    return;
                if (item.Parent != null)
                    item.Parent.Components.Remove(item);
                if (item.Scene != null)
                    item.Scene.Components.Remove(item);
                item.Scene = _scene;
                item.Spawn();
                _list.Add(item);
            }

            public void Clear()
            {
                while (_list.Count > 0)
                    Remove(_list[0]);
            }

            public bool Contains(GameComponent item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(GameComponent[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public IEnumerator<GameComponent> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public bool Remove(GameComponent item)
            {
                if (item.Scene != _scene || item.Parent != null)
                    return false;
                item.Scene = null;
                item.Despawn();
                return _list.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }


        public class ComponentCollection : ICollection<GameComponent>
        {
            private GameComponent _owner = null;
            private List<GameComponent> _list = new List<GameComponent>();

            internal ComponentCollection(GameComponent owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            public void Add(GameComponent item)
            {
                if (item.Parent == this._owner)
                    return;
                if (item.Parent != null)
                    item.Parent.Components.Remove(item);
                item.Parent = _owner;
                item.Spawn();
                _list.Add(item);
            }

            public void Clear()
            {
                while (_list.Count > 0)
                {
                    Remove(_list[0]);
                }
            }

            public bool Contains(GameComponent item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(GameComponent[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public IEnumerator<GameComponent> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public bool Remove(GameComponent item)
            {
                if (item.Parent != _owner)
                    return false;
                item.Parent = null;
                item.Despawn();
                return _list.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        private Rectangle _bounds = Rectangle.Empty;
        private GameScene _scene = null;

        protected virtual void OnSpawn() { }
        protected virtual void OnDespawn() { }
        
        private void Spawn()
        {
            OnSpawn();
        }

        private void Despawn()
        {
            OnDespawn();
        }

        public GameScene Scene
        {
            get
            {
                if (Parent != null)
                    return Parent.Scene;
                return _scene;
            }
            private set
            {
                if (Parent != null)
                    return;
                _scene = value;
            }
        }

        public GameComponent()
        {
            Components = new ComponentCollection(this);
        }

        public Vector2 ToLocal(Vector2 coordinates)
        {
            var parent = this;
            while (parent != null)
            {
                coordinates -= new Vector2(parent.X, parent.Y);
                parent = parent.Parent;
            }
            return coordinates;
        }

        public Vector2 ToScreen(Vector2 coordinates)
        {
            var parent = this;
            while(parent != null)
            {
                coordinates += new Vector2(parent.X, parent.Y);
                parent = parent.Parent;
            }
            return coordinates;
        }

        public readonly ComponentCollection Components = null;

        public GameComponent Parent { get; private set; }

        public bool Visible { get; set; } = true;
        
        public Rectangle Bounds { get => _bounds; set => _bounds = value; }

        public int X { get => _bounds.X; set => _bounds.X = value; }
        public int Y { get => _bounds.Y; set => _bounds.Y = value; }
        public int Width { get => _bounds.Width; set => _bounds.Width = value; }
        public int Height { get => _bounds.Height; set => _bounds.Height = value; }

        protected virtual void OnKeyEvent(KeyboardEventArgs e) { }
        protected virtual void OnMouseDown(MouseEventArgs e) { }
        protected virtual void OnMouseUp(MouseEventArgs e) { }
        protected virtual void OnClick(MouseEventArgs e) { }
        protected virtual void OnDoubleClick(MouseEventArgs e) { }
        protected virtual void OnMouseMove(MouseEventArgs e) { }
        protected virtual void OnMouseEnter(MouseEventArgs e) { }
        protected virtual void OnMouseLeave(MouseEventArgs e) { }
        protected virtual void OnMouseDragStart(MouseEventArgs e) { }
        protected virtual void OnMouseDrag(MouseEventArgs e) { }
        protected virtual void OnMouseDragEnd(MouseEventArgs e) { }

        internal void FireMouseEnter(MouseEventArgs e)
        {
            OnMouseEnter(e);
        }

        internal void FireMouseLeave(MouseEventArgs e)
        {
            OnMouseLeave(e);
        }

        internal void FireKeyEvent(KeyboardEventArgs e)
        {
            OnKeyEvent(e);
        }

        internal void FireMouseDragEnd(MouseEventArgs e)
        {
            OnMouseDragEnd(e);
        }

        internal void FireMouseDrag(MouseEventArgs e)
        {
            OnMouseDrag(e);
        }

        internal void FireMouseDragStart(MouseEventArgs e)
        {
            OnMouseDragStart(e);
        }

        internal void FireMouseDown(MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        internal void FireMouseDoubleClick(MouseEventArgs e)
        {
            OnDoubleClick(e);
        }

        internal void FireMouseMove(MouseEventArgs e)
        {
            OnMouseMove(e);

            foreach (var child in Components)
                child.FireMouseMove(e.OffsetPosition(new Vector2(child.X, child.Y)));
        }


        internal void FireMouseClick(MouseEventArgs e)
        {
            OnClick(e);
        }

        internal void FireMouseUp(MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        internal void FireScroll(MouseEventArgs e)
        {
            OnMouseScroll(e);
        }

        protected virtual void OnMouseScroll(MouseEventArgs e) { }


        public void Update(GameTime time)
        {
            OnUpdate(time);
            foreach (var component in Components.ToArray())
                component.Update(time);
        }

        public Rectangle GetScissorRectangle()
        {
            var screen = ToScreen(Vector2.Zero);
            Rectangle bounds = new Rectangle((int)screen.X, (int)screen.Y, Width, Height);
            var parent = this;
            while (parent != null)
            {
                var pScreen = parent.ToScreen(Vector2.Zero);
                bounds = Rectangle.Intersect(bounds, new Rectangle((int)pScreen.X, (int)pScreen.Y, parent.Width, parent.Height));
                parent = parent.Parent;
            }
            if(Scene!=null)
                bounds = Rectangle.Intersect(bounds, new Rectangle(0, 0, Scene.Width, Scene.Height));
            return bounds;
        }

        public void Draw(GameTime time, GraphicsContext gfx)
        {
            if (!Visible || _bounds == Rectangle.Empty)
                return;

            var scissor = GetScissorRectangle();
            var start = ToScreen(new Vector2(0, 0));

            gfx.ScissorRectangle = scissor;
            gfx.RenderOffsetX = -(gfx.X - (int)start.X);
            gfx.RenderOffsetY = -(gfx.Y - (int)start.Y);

            OnDraw(time, gfx);
            foreach (var child in Components.ToArray())
                child.Draw(time, gfx);
        }

        protected abstract void OnUpdate(GameTime time);
        protected abstract void OnDraw(GameTime time, GraphicsContext gfx);
    }
}
