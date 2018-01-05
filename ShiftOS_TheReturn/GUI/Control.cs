﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GUI
{
    public class Control : IDisposable
    {
        private int _x = 0;
        private int _y = 0;
        private int _width = 1;
        private int _height = 1;
        private List<Control> _children = null;
        private bool _invalidated = true;
        protected RenderTarget2D _rendertarget = null;
        protected RenderTarget2D _userfacingtarget = null;
        private bool _resized = false;
        private Control _parent = null;
        private int _mousex = -1;
        private int _mousey = -1;
        private ButtonState _left;
        private ButtonState _middle;
        private ButtonState _right;
        private bool _isVisible = true;
        private bool _disposed = false;
        private float _opacity = 1;

        private bool _enabled = true;

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;
                Invalidate();
            }
        }

        private bool _doDoubleClick = false;
        private double _doubleClickCooldown = 0;

        private int _minWidth = 1;
        private int _minHeight = 1;
        private int _maxWidth = 0;
        private int _maxHeight = 0;

        public bool IsFocused
        {
            get
            {
                return Manager.IsFocused(this);
            }
        }

        public bool HasFocused
        {
            get
            {
                if (IsFocused)
                    return true;
                foreach(var child in Children)
                {
                    if (child.HasFocused)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        
        public RenderTarget2D BackBuffer
        {
            get
            {
                return _rendertarget;
            }
        }

        public event EventHandler Click;
        public event EventHandler RightClick;
        public event EventHandler MiddleClick;
        public event Action<object, Vector2> MouseMove;
        public event EventHandler MouseLeftDown;
        public event EventHandler MouseRightDown;
        public event EventHandler MouseMiddleDown;
        public event EventHandler MouseLeftUp;
        public event EventHandler MouseRightUp;
        public event EventHandler MouseMiddleUp;
        public event EventHandler<KeyboardEventArgs> KeyEvent;
        public event EventHandler DoubleClick;


        public event EventHandler WidthChanged;
        public event EventHandler HeightChanged;
        public event EventHandler XChanged;
        public event EventHandler YChanged;
        public event EventHandler VisibleChanged;


        public int MinWidth
        {
            get
            {
                return _minWidth;
            }
            set
            {
                value = Math.Max(1, value);
                if (value == _minWidth)
                    return;
                _minWidth = value;
                Width = Width;
            }
        }

        public int MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                value = Math.Max(1, value);
                if (value == _minHeight)
                    return;
                _minHeight = value;
                Height = Height;
            }
        }

        public int MaxWidth
        {
            get
            {
                return _maxWidth;
            }
            set
            {
                value = Math.Max(_minWidth, value);
                if (value == _maxWidth)
                    return;
                _maxWidth = value;
                Width = Width;
            }
        }

        public int MaxHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                value = Math.Max(1, value);
                if (value == _maxHeight)
                    return;
                _maxHeight = value;
                Height = Height;
            }
        }

        public void Clear()
        {
            while(_children.Count > 0)
            {
                RemoveChild(_children[0]);
            }
        }

        public float Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                value = MathHelper.Clamp(value, 0, 1);
                if (value == _opacity)
                    return;
                _opacity = value;
                Invalidate();
            }
        }

        public bool Disposed
        {
            get
            {
                return _disposed;
            }
        }

        public bool Visible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (_isVisible == value)
                    return;
                _isVisible = value;
                VisibleChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        public bool ContainsMouse
        {
            get
            {
                if (_enabled == false)
                    return false;
                return (_mousex >= 0 && _mousex <= Width && _mousey >= 0 && _mousey <= Height);
            }
        }

        private UIManager _manager = null;

        public UIManager Manager
        {
            get
            {
                if (Parent != null)
                    return Parent.Manager;
                return _manager;
            }
        }

        internal void SetManager(UIManager manager)
        {
            _manager = manager;
        }

        internal void ProcessKeyboardEvent(KeyboardEventArgs e)
        {
            OnKeyEvent(e);
            KeyEvent?.Invoke(this, e);
        }

        protected virtual void OnKeyEvent(KeyboardEventArgs e)
        {

        }

        private Themes.Theme _theme = null;

        public Theme Theme
        {
            get
            {
                if (Parent != null)
                    return Parent.Theme; //this will walk up the ui tree to the toplevel and grab the theme.
                return _theme;
            }
        }

        internal void SetTheme(Theme theme)
        {
            _theme = theme;
        }

        public virtual void AddChild(Control child)
        {
            if (_children.Contains(child))
                return;
            _children.Add(child);
            child._parent = this;
            Invalidate();
        }

        public int MouseX
        {
            get
            {
                return _mousex;
            }
        }

        public int MouseY
        {
            get
            {
                return _mousey;
            }
        }

        public Control Parent
        {
            get
            {
                return _parent;
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (value < 1)
                    value = 1;
                if (_width == value)
                    return;
                _width = value;
                _resized = true;
                Invalidate(true);
                WidthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (value < 1)
                    value = 1;
                if (_height == value)
                    return;
                _height = value;
                _resized = true;
                Invalidate(true);
                HeightChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int X
        {
            get
            {
                return _x;
            }
            set
            {
                if (_x == value)
                    return;
                _x = value;
                XChanged?.Invoke(this, EventArgs.Empty);
                if (_parent != null)
                    Invalidate();
            }
        }

        public int Y
        {
            get
            {
                return _y;
            }
            set
            {
                if (_y == value)
                    return;
                _y = value;
                YChanged?.Invoke(this, EventArgs.Empty);
                if(_parent!=null)
                    Invalidate();
            }
        }


        public Control()
        {
            _children = new List<Control>();
        }

        protected virtual void OnUpdate(GameTime time)
        {

        }

        private ButtonState _lastLeft;
        private ButtonState _lastRight;
        private ButtonState _lastMiddle;

        public virtual void RemoveChild(Control child)
        {
            if (!_children.Contains(child))
                return;
            _children.Remove(child);
            Invalidate();
        }

        private int _lastScrollValue = 0;

        protected virtual void OnMouseScroll(int delta)
        {

        }

        public event Action<int> MouseScroll;

        public virtual bool PropagateMouseState(MouseState state, bool skipEvents = false)
        {
            if (_enabled == false || _isVisible == false)
                return false;
            int x = 0;
            int y = 0;

            if (Parent == null)
            {
                x = state.X - X;
                y = state.Y - Y;
            }
            //For controls with parents, poll mouse information from the parent.
            else
            {
                x = Parent._mousex - X;
                y = Parent._mousey - Y;
            }
            if(_mousex != x || _mousey != y)
            {
                bool hasMouse = ContainsMouse;
                _mousex = x;
                _mousey = y;
                MouseMove?.Invoke(this, new Vector2(x, y));
                if(ContainsMouse != hasMouse)
                {
                    Invalidate(true);
                }
            }

            bool doEvents = !skipEvents;
            foreach(var child in Children.OrderByDescending(z=>Array.IndexOf(Children, z)))
            {
                bool res = child.PropagateMouseState(state, !doEvents);
                if (doEvents == true && res == true)
                    doEvents = false;
            }
            if (doEvents)
            {
                if (ContainsMouse)
                {
                    bool left = LeftMouseState == ButtonState.Pressed;
                    bool right = RightMouseState == ButtonState.Pressed;
                    bool middle = MiddleMouseState == ButtonState.Pressed;

                    if(LeftMouseState != state.LeftButton)
                    {
                        if(left)
                        {
                            Click?.Invoke(this, EventArgs.Empty);
                            MouseLeftUp?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            MouseLeftDown?.Invoke(this, EventArgs.Empty);
                        }
                        _lastLeft = state.LeftButton;
                        Invalidate(true);
                    }
                    if (RightMouseState != state.RightButton)
                    {
                        if (right)
                        {
                            MouseRightUp?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            MouseRightDown?.Invoke(this, EventArgs.Empty);
                        }
                        _lastRight = state.RightButton;
                        Invalidate(true);
                    }

                    return true;
                }
                else
                {
                    if (_lastLeft != ButtonState.Released || _lastRight != ButtonState.Released || _lastMiddle != ButtonState.Released)
                    {
                        _lastLeft = ButtonState.Released;
                        _lastRight = ButtonState.Released;
                        _lastMiddle = ButtonState.Released;
                        Invalidate(true);
                    }

                }
            }
            return false;
        }

        private bool? _lastFocus = null;

        public event EventHandler HasFocusedChanged;

        public Control[] Children
        {
            get
            {
                if (_children == null)
                    return new Control[0];
                return _children.ToArray();
            }
        }

        private MouseState _lastState;

        public void Update(GameTime time)
        {
            if (_lastFocus != HasFocused)
            {
                _lastFocus = HasFocused;
                HasFocusedChanged?.Invoke(this, EventArgs.Empty);
            }
            if (_rendertarget == null)
            {
                Invalidate(true);
                _resized = true;
            }
            if (_userfacingtarget == null)
            {
                Invalidate(true);
                _resized = true;
            }

            if (_isVisible == false)
                return;
            OnUpdate(time);
            if (_children == null)
                return;
            if (_disposed)
                return;
            try
            {
                foreach (var child in _children)
                {
                    child.Update(time);
                }
            }
            catch { }
        }



        protected virtual void OnPaint(GameTime time, GraphicsContext gfx)
        {
            Theme.DrawControlBG(gfx, 0, 0, Width, Height);
        }

        private bool _needsRerender = true;

        public void Invalidate(bool needsRepaint = false)
        {
            if (needsRepaint)
            {
                _invalidated = true;
            }
            _needsRerender = true;
            Parent?.Invalidate();
        }

        


        public void Draw(GameTime time, GraphicsContext gfx)
        {
            bool makeBack = false; //normally I'd let this be false but I thought I'd try making the backbuffer reset if the control's invalidated. This seemed to help, but right after restarting the game and doing the same thing, the bug was back. So this only works intermitently.
            if (_rendertarget == null)
                makeBack = true;
            else
            {
                if (_rendertarget.Width != Width || _rendertarget.Height != Height)
                {
                    _rendertarget.Dispose();
                    _rendertarget = null;
                    makeBack = true;
                }
            }
            if (makeBack)
            {
                _rendertarget = new RenderTarget2D(gfx.Device, Width, Height, false, gfx.Device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
            }

            if (_needsRerender)
            {
                if (_invalidated)
                {
                    if (_resized)
                    {
                        _userfacingtarget?.Dispose();
                        _userfacingtarget = new RenderTarget2D(gfx.Device, Width, Height, false, gfx.Device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
                        _resized = false;
                    }
                    gfx.Device.SetRenderTarget(_userfacingtarget);
                    gfx.Device.Clear(Color.Transparent);
                    gfx.BeginDraw();
                    OnPaint(time, gfx);
                    gfx.EndDraw();
                    _invalidated = false;
                }
                foreach (var child in _children)
                {
                    if (!child.Visible)
                        continue;
                    if(child.Opacity>0)
                        child.Draw(time, gfx);
                }

                gfx.Device.SetRenderTarget(_rendertarget);
                gfx.Device.Clear(Color.Transparent);
                if (_userfacingtarget != null)
                {
                    gfx.BeginDraw();
                    gfx.Batch.Draw(_userfacingtarget, new Rectangle(0, 0, Width, Height), Color.White);
                    foreach (var child in _children)
                    {
                        if (!child.Visible)
                            continue;
                        if (child.Opacity > 0)
                        {
                            var tint = (child.Enabled) ? Color.White : Color.Gray;
                            if (Manager.IgnoreControlOpacity)
                            {
                                gfx.Batch.Draw(child.BackBuffer, new Rectangle(child.X, child.Y, child.Width, child.Height), tint);
                            }
                            else
                            {
                                gfx.Batch.Draw(child.BackBuffer, new Rectangle(child.X, child.Y, child.Width, child.Height), tint * child.Opacity);
                            }
                        }
                    }
                    gfx.EndDraw();
                }
                else
                {
                    Invalidate(true);
                    _resized = true;
                    
                }
                _needsRerender = false;

            }
        }

        public ButtonState LeftMouseState
        {
            get
            {
                return _lastLeft;
            }
        }

        public ButtonState RightMouseState
        {
            get
            {
                return _lastRight;
            }
        }
        public ButtonState MiddleMouseState
        {
            get
            {
                return _lastMiddle;
            }
        }

        public void Dispose()
        {
            if (_rendertarget != null)
            {
                _rendertarget.Dispose();
                _rendertarget = null;
            }
            if (_userfacingtarget != null)
            {
                _userfacingtarget.Dispose();
                _userfacingtarget = null;
            }
            if (_children != null)
            {
                while (_children.Count > 0)
                {
                    _children[0].Dispose();
                    _children.RemoveAt(0);
                }
                _children = null;
            }
            _disposed = true;
        }
    }

    public class TestChild : Control
    {
        
        protected override void OnUpdate(GameTime time)
        {
            var measure = TextRenderer.MeasureText(((int)time.TotalGameTime.TotalSeconds).ToString(), new System.Drawing.Font("Lucida Console", 12F), Parent.Width, TextAlignment.Middle, TextRenderers.WrapMode.Words);
            Width = (int)measure.X;
            Height = (int)measure.Y;
            X = (Parent.Width - Width) / 2;
            Y = (Parent.Height - Height) / 2;
            Invalidate();
            base.OnUpdate(time);
        }

        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            gfx.Clear(Color.Gray);
            gfx.DrawString(((int)time.TotalGameTime.TotalSeconds).ToString(), 0, 0, Color.White, new System.Drawing.Font("Lucida Console", 12F), TextAlignment.Middle, Width, TextRenderers.WrapMode.Words);
        }
    }
}
