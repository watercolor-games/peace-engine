using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.GameComponents.UI.Themes;

namespace Plex.Engine.GameComponents.UI
{
    public abstract class Control : IDisposable
    {
        private Control _parent = null;
        private UserInterface _ui = null;
        private RenderTarget2D _userfacingtarget = null;
        private bool _disposed = false;

        public readonly ControlCollection Children = null;
        public UserInterface UserInterface
        {
            get
            {
                if (Parent != null)
                    return Parent.UserInterface;
                return _ui;
            }
            private set
            {
                if (Parent != null)
                    return;
                _ui = value;
            }
        }

        public Control Parent => _parent;
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;

        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;


        public Control()
        {
            Children = new ControlCollection(this);
        }

        public RenderTarget2D BackBuffer => _userfacingtarget;

        public float Opacity { get; set; } = 1;

        public string ToolTip { get; set; } = "";

        public event EventHandler<KeyboardEventArgs> KeyEvent;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> Click;
        public event EventHandler<MouseEventArgs> DoubleClick;
        public event EventHandler<MouseEventArgs> MouseMove;
        public event EventHandler<MouseEventArgs> MouseEnter;
        public event EventHandler<MouseEventArgs> MouseLeave;
        public event EventHandler<MouseEventArgs> MouseDragStart;
        public event EventHandler<MouseEventArgs> MouseDragEnd;
        public event EventHandler<MouseEventArgs> MouseDrag;

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

        public bool LeftButtonPressed { get; private set; } = false;
        public bool MiddleButtonPressed { get; private set; } = false;
        public bool RightButtonPressed { get; private set; } = false;

        public event EventHandler HasFocusedChanged;

        internal void FireKeyEvent(KeyboardEventArgs e)
        {
            OnKeyEvent(e);
            KeyEvent?.Invoke(this, e);
        }

        internal void ResetButtonStates()
        {
            LeftButtonPressed = false;
            MiddleButtonPressed = false;
            RightButtonPressed = false;
        }

        public bool ContainsMouse
        {
            get
            {
                if (UserInterface.HoveredControl == this) return true;
                return Children.Any(x => x.ContainsMouse);
            }
        }

        internal void FireMouseDragEnd(MouseEventArgs e)
        {
            OnMouseDragEnd(e);
            MouseDragEnd?.Invoke(this, e);
        }

        internal void FireMouseDrag(MouseEventArgs e)
        {
            OnMouseDrag(e);
            MouseDrag?.Invoke(this, e);
        }

        internal void FireMouseDragStart(MouseEventArgs e)
        {
            OnMouseDragStart(e);
            MouseDragStart?.Invoke(this, e);
        }

        public Theme Theme => UserInterface.Theme;

        internal void FireMouseDown(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    LeftButtonPressed = true;
                    break;
                case MouseButton.Middle:
                    MiddleButtonPressed = true;
                    break;
                case MouseButton.Right:
                    RightButtonPressed = true;
                    break;
            }
            OnMouseUp(e);
            MouseUp?.Invoke(this, e);
        }

        internal void FireMouseDoubleClick(MouseEventArgs e)
        {
            OnDoubleClick(e);
            DoubleClick?.Invoke(this, e);
        }

        internal void FireMouseLeave(MouseEventArgs e)
        {
            OnMouseLeave(e);
            MouseLeave?.Invoke(this, e);
        }

        internal void FireMouseEnter(MouseEventArgs e)
        {
            OnMouseEnter(e);
            MouseEnter?.Invoke(this, e);
        }

        internal void FireMouseMove(MouseEventArgs e)
        {
            OnMouseMove(e);
            MouseMove?.Invoke(this, e);

            foreach (var child in Children)
                child.FireMouseMove(e.OffsetPosition(new Vector2(child.X, child.Y)));
        }


        internal void FireMouseClick(MouseEventArgs e)
        {
            OnClick(e);
            Click?.Invoke(this, e);
        }

        internal void FireMouseUp(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    LeftButtonPressed = false;
                    break;
                case MouseButton.Middle:
                    MiddleButtonPressed = false;
                    break;
                case MouseButton.Right:
                    RightButtonPressed = false;
                    break;
            }
            OnMouseUp(e);
            MouseUp?.Invoke(this, e);
        }

        public Vector2 ToToplevel(int x, int y)
        {
            var parent = this;
            while (parent != null)
            {
                x += parent.X;
                y += parent.Y;
                parent = parent.Parent;
            }
            return new Vector2(x, y);
        }

        public Vector2 ToScreen(int x, int y)
        {
            var parent = this;
            while (parent != null && parent._userfacingtarget == null)
            {
                x += parent.X;
                y += parent.Y;
                parent = parent.Parent;
            }
            return new Vector2(x, y);
        }

        public Rectangle Bounds
        {
            get
            {
                var screenPos = ToScreen(0, 0);
                return new Rectangle((int)screenPos.X, (int)screenPos.Y, Width, Height);
            }
        }

        private bool _lastFocus = false;

        public bool IsFocused => UserInterface.IsFocused(this);

        public bool HasFocused
        {
            get
            {
                if (IsFocused) return true;
                return Children.Any(x => x.HasFocused);
            }
        }

        /// <summary>
        /// Fire an update event.
        /// </summary>
        /// <param name="time">The time since the last frame. Used for animation.</param>
        public void Update(GameTime time)
        {
            if (_lastFocus != HasFocused)
            {
                _lastFocus = HasFocused;
                HasFocusedChanged?.Invoke(this, EventArgs.Empty);
            }

            if (Visible == false)
                return;
            if (_disposed)
                return;

            OnUpdate(time);
            foreach (var child in Children)
            {
                child.Update(time);
            }
        }

        private Rectangle GetScissorRectangle()
        {
            Rectangle bounds = Bounds;
            var parent = this;
            while (parent != null)
            {
                bounds = Rectangle.Intersect(bounds, parent.Bounds);
                if (parent._userfacingtarget != null)
                    break;
                parent = parent.Parent;
            }
            return bounds;
        }

        protected virtual void OnUpdate(GameTime time)
        {

        }

        protected virtual void OnPaint(GameTime time, GraphicsContext gfx)
        {

        }

        /// <summary>
        /// Fire a render event.
        /// </summary>
        /// <param name="time">The time since the last frame.</param>
        /// <param name="gfx">The graphics context to render the control to.</param>
        public void Draw(GameTime time, GraphicsContext gfx)
        {
            if (Visible == false || Width==0 || Height==0)
                return;
            if (Opacity <= 0)
                return;
            //If we're disabled, set the Grayout property.

            if (((Opacity < 1) || Enabled == false) && _userfacingtarget == null)
                _userfacingtarget = gfx.CreateRenderTarget(Width, Height);
            else if (Opacity == 1 || Enabled == true)
            {
                if (_userfacingtarget != null)
                {
                    _userfacingtarget.Dispose();
                    _userfacingtarget = null;
                }
            }
            gfx.ScissorRectangle = GetScissorRectangle();

            if (gfx.ScissorRectangle == Rectangle.Empty)
                return;

            var screenPos = ToScreen(0, 0);
            gfx.RenderOffsetX = -(gfx.X - (int)screenPos.X);
            gfx.RenderOffsetY = -(gfx.Y - (int)screenPos.Y);


            if (_userfacingtarget != null)
            {
                gfx.SetRenderTarget(BackBuffer);
                gfx.Clear(Color.Transparent);
            }

            OnPaint(time, gfx);

            foreach (var child in Children)
            {
                child.Draw(time, gfx);
                if (child._userfacingtarget != null)
                {
                    var s = ToScreen(child.X, child.Y);
                    var tint = child.Enabled ? Color.White : Color.Gray;
                    gfx.FillRectangle(new RectangleF(s.X, s.Y, child.Width, child.Height), child._userfacingtarget, tint * child.Opacity);
                }
            }
            if (_userfacingtarget != null)
                gfx.SetRenderTarget(Parent?.BackBuffer ?? GameLoop.GetInstance().GameRenderTarget);

        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_userfacingtarget != null)
            {
                _userfacingtarget.Dispose();
                _userfacingtarget = null;
            }
            if (Children != null)
            {
                while (Children.Count > 0)
                {
                    var first = Children.First();
                    Children.Remove(first);
                    first.Dispose();
                }
            }
            _disposed = true;
        }

        public class TopLevelCollection : ICollection<Control>
        {
            private List<Control> _controls = new List<Control>();
            private UserInterface _owner = null;

            internal TopLevelCollection(UserInterface owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public int Count => _controls.Count;

            public bool IsReadOnly => false;

            public void Add(Control item)
            {
                if (item.UserInterface == _owner)
                    return;
                item.UserInterface = _owner;
                _controls.Add(item);
            }

            public void Clear()
            {
                while (_controls.Count > 0)
                    Remove(_controls[0]);
            }

            public bool Contains(Control item)
            {
                return _controls.Contains(item);
            }

            public void CopyTo(Control[] array, int arrayIndex)
            {
                _controls.CopyTo(array, arrayIndex);
            }

            public IEnumerator<Control> GetEnumerator()
            {
                return _controls.GetEnumerator();
            }

            public bool Remove(Control item)
            {
                if (item.Parent != null || item.UserInterface!=_owner)
                    return false;
                item.UserInterface = null;
                return _controls.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _controls.GetEnumerator();
            }

        }

        public class ControlCollection : ICollection<Control>
        {
            private List<Control> _controls = new List<Control>();
            private Control _owner = null;

            internal ControlCollection(Control owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public int Count => _controls.Count;

            public bool IsReadOnly => false;

            public void Add(Control item)
            {
                if (item.Parent == _owner)
                    return;
                if(item.Parent != null)
                {
                    item.Parent.Children.Remove(item);
                }
                item._parent = _owner;
                _controls.Add(item);
            }

            public void Clear()
            {
                while (_controls.Count > 0)
                    Remove(_controls[0]);
            }

            public bool Contains(Control item)
            {
                return _controls.Contains(item);
            }

            public void CopyTo(Control[] array, int arrayIndex)
            {
                _controls.CopyTo(array, arrayIndex);
            }

            public IEnumerator<Control> GetEnumerator()
            {
                return _controls.GetEnumerator();
            }

            public bool Remove(Control item)
            {
                if (item.Parent != _owner)
                    return false;
                item._parent = null;
                return _controls.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _controls.GetEnumerator();
            }
        }
    }
}
