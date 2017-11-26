﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GUI
{
    public class Control : IDisposable
    {
        private int _x = 0;
        private int _y = 0;
        private int _width = 350;
        private int _height = 100;
        private List<TopLevel> _children = null;
        private bool _invalidated = true;
        private RenderTarget2D _rendertarget = null;
        private bool _resized = false;
        private Control _parent = null;
        private int _mousex = 0;
        private int _mousey = 0;
        private ButtonState _left;
        private ButtonState _middle;
        private ButtonState _right;
        private bool _isVisible = true;

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
            }
        }

        public bool ContainsMouse
        {
            get
            {
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

        public void AddChild(Control child)
        {
            if (_children.FirstOrDefault(x => x.Control == child)!=null)
                return;
            _children.Add(new TopLevel
            {
                Control = child
            });
            child._parent = this;
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
                _invalidated = true;
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
                _invalidated = true;
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
                _x = value;
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
                _y = value;
            }
        }


        public Control()
        {
            _children = new List<TopLevel>();
        }

        protected virtual void OnUpdate(GameTime time)
        {

        }

        private ButtonState _lastLeft;
        private ButtonState _lastRight;
        private ButtonState _lastMiddle;


        internal bool PropagateMouseState(ButtonState left, ButtonState middle, ButtonState right)
        {
            foreach(var child in _children)
            {
                if (child.Control.PropagateMouseState(left, middle, right))
                    return true;
            }
            if (_isVisible == false)
                return false;
            bool isInCtrl = (_mousex >= 0 && _mousex <= Width && _mousey >= 0 && _mousey <= Height);
            if (_lastLeft == left && _lastRight == right && _lastMiddle == middle)
                return isInCtrl;
            _lastLeft = left;
            _lastRight = right;
            _lastMiddle = middle;

            if (isInCtrl)
            {
                bool fireLeft = (_left == ButtonState.Pressed && left == ButtonState.Released);
                bool fireRight = (_right == ButtonState.Pressed && right == ButtonState.Released);
                bool fireMiddle = (_middle == ButtonState.Pressed && middle == ButtonState.Released);
                if (_left != left || _right != right || _middle != middle)
                    _invalidated = true;
                if (_left != left && left == ButtonState.Pressed)
                    MouseLeftDown?.Invoke(this, EventArgs.Empty);
                if (_right != right && right == ButtonState.Pressed)
                    MouseRightDown?.Invoke(this, EventArgs.Empty);
                if (_middle != middle && middle == ButtonState.Pressed)
                    MouseMiddleDown?.Invoke(this, EventArgs.Empty);
                if (_left != left && left == ButtonState.Released)
                    MouseLeftUp?.Invoke(this, EventArgs.Empty);
                if (_right != right && right == ButtonState.Released)
                    MouseRightUp?.Invoke(this, EventArgs.Empty);
                if (_middle != middle && middle == ButtonState.Released)
                    MouseMiddleUp?.Invoke(this, EventArgs.Empty);

                _left = left;
                _middle = middle;
                _right = right;
                if (fireLeft)
                {
                    Click?.Invoke(this, EventArgs.Empty);
                }
                if (fireRight)
                {
                    RightClick?.Invoke(this, EventArgs.Empty);

                }
                if (fireMiddle)
                {
                    MiddleClick?.Invoke(this, EventArgs.Empty);
                    
                }
            }
            else
            {
                _left = ButtonState.Released;
                _middle = _left;
                _right = _left;
            }
            return isInCtrl;
        }

        public void Update(GameTime time)
        {
            if (_rendertarget == null)
            {
                _invalidated = true;
                _resized = true;
            }
            if (_isVisible == false)
                return;
            //Poll the mouse state.
            var mouse = Mouse.GetState();
            //For toplevels, set mouse input loc directly.
            int _newmousex = 0;
            int _newmousey = 0;
            if (Parent == null)
            {
                _newmousex = mouse.X - X;
                _newmousey = mouse.Y - Y;
            }
            //For controls with parents, poll mouse information from the parent.
            else
            {
                _newmousex = Parent._mousex - X;
                _newmousey = Parent._mousey - Y;
            }
            if(_newmousex != _mousex || _newmousey != _mousey)
            {
                _mousex = _newmousex;
                _mousey = _newmousey;
                MouseMove?.Invoke(this, new Vector2(_newmousex, _newmousey));
                _invalidated = true;
            }
            OnUpdate(time);
            foreach (var child in _children)
            {
                bool makeTarget = false;
                if (child.RenderTarget == null)
                    makeTarget = true;
                else
                {
                    if (child.RenderTarget.Width != child.Control.Width || child.RenderTarget.Height != child.Control.Height)
                        makeTarget = true;
                }
                if (makeTarget)
                {
                    if(_rendertarget != null)
                    {
                        child.RenderTarget = new RenderTarget2D(_rendertarget.GraphicsDevice, child.Control.Width, child.Control.Height, false, _rendertarget.Format, _rendertarget.DepthStencilFormat, 1, RenderTargetUsage.PreserveContents);
                        child.Control.Invalidate();
                    }
                }
                child.Control.Update(time);
            }
        }

        protected virtual void OnPaint(GameTime time, GraphicsContext gfx, RenderTarget2D currentTarget)
        {
            Theme.DrawControlBG(gfx, 0, 0, Width, Height);
        }

        public void Invalidate()
        {
            _invalidated = true;
        }

        public void Draw(GameTime time, GraphicsContext gfx, RenderTarget2D currentTarget)
        {
            if (_isVisible == false)
                return;
            if (_invalidated)
            {
                if (_resized)
                {
                    if (_rendertarget != null)
                        _rendertarget.Dispose();
                    _rendertarget = new RenderTarget2D(gfx.Device, _width, _height, false, gfx.Device.PresentationParameters.BackBufferFormat, gfx.Device.PresentationParameters.DepthStencilFormat, 1, RenderTargetUsage.PreserveContents);
                    _resized = false;
                }
                int lastw = gfx.Width;
                int lasth = gfx.Height;
                gfx.Width = _width;
                gfx.Height = _height;
                gfx.Device.SetRenderTarget(_rendertarget);
                gfx.Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
                    SamplerState.LinearWrap, DepthStencilState.Default,
                    RasterizerState.CullNone);
                OnPaint(time, gfx, _rendertarget);
                gfx.Batch.End();
                gfx.Width = lastw;
                gfx.Height = lasth;
                _invalidated = false;
            }
            foreach (var ctrl in _children)
            {
                int lastw = gfx.Width;
                int lasth = gfx.Height;
                int lastx = gfx.X;
                int lasty = gfx.Y;
                gfx.Width = ctrl.Control._width;
                gfx.Height = ctrl.Control._height;
                gfx.Device.SetRenderTarget(ctrl.RenderTarget);
                ctrl.Control.Draw(time, gfx, ctrl.RenderTarget);
                gfx.Device.SetRenderTarget(_rendertarget);
                gfx.Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
    SamplerState.LinearWrap, DepthStencilState.Default,
    RasterizerState.CullNone);
                gfx.DrawRectangle(ctrl.Control.X, ctrl.Control.Y, ctrl.Control.Width, ctrl.Control.Height, ctrl.RenderTarget, Color.White);
                gfx.Batch.End();

                gfx.Width = lastw;
                gfx.Height = lasth;
                gfx.X = lastx;
                gfx.Y = lasty;
            }
            gfx.Device.SetRenderTarget(currentTarget);
            gfx.Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
SamplerState.LinearWrap, DepthStencilState.Default,
RasterizerState.CullNone);

            gfx.DrawRectangle(0, 0, _rendertarget.Width, _rendertarget.Height, _rendertarget, Color.White);
            gfx.Batch.End();

        }

        public void Dispose()
        {
            if (_rendertarget != null)
            {
                _rendertarget.Dispose();
                _rendertarget = null;
            }
            foreach(var rinfo in _children)
            {
                rinfo.Control.Dispose();
                rinfo.RenderTarget.Dispose();
                rinfo.Control = null;
                rinfo.RenderTarget = null;
            }
            _children.Clear();
            _children = null;
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

        protected override void OnPaint(GameTime time, GraphicsContext gfx, RenderTarget2D currentTarget)
        {
            gfx.Clear(Color.Gray);
            gfx.DrawString(((int)time.TotalGameTime.TotalSeconds).ToString(), 0, 0, Color.White, new System.Drawing.Font("Lucida Console", 12F), TextAlignment.Middle, Width, TextRenderers.WrapMode.Words);
        }
    }
}
