using Microsoft.Xna.Framework;
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


        public void Update(GameTime time)
        {
            OnUpdate(time);
            foreach (var component in Components.ToArray())
                component.Update(time);
        }

        private Rectangle GetScissorRectangle()
        {
            Rectangle bounds = Bounds;
            var parent = this;
            while (parent != null)
            {
                bounds = Rectangle.Intersect(bounds, parent.Bounds);
                parent = parent.Parent;
            }
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
