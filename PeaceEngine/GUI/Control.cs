using Microsoft.Xna.Framework;
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
    /// <summary>
    /// A class representing a Peacenet GUI element.
    /// </summary>
    public class Control : IDisposable
    {
        private int _x = 0;
        private int _y = 0;
        private int _width = 1;
        private int _height = 1;
        private List<Control> _children = null;
        internal RenderTarget2D _userfacingtarget = null;
        private Control _parent = null;
        private bool _isVisible = true;
        private bool _disposed = false;
        private float _opacity = 1;
        private bool _enabled = true;

        /// <summary>
        /// Gets or sets whether this control is enabled. If not, the control will not receive mouse or keyboard events, and will be visually grayed out.
        /// </summary>
        public virtual bool Enabled
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
                
            }
        }

        private int _minWidth = 1;
        private int _minHeight = 1;
        private int _maxWidth = 0;
        private int _maxHeight = 0;

        /// <summary>
        /// Retrieves whether this control is focused.
        /// </summary>
        public bool IsFocused
        {
            get
            {
                return Manager.IsFocused(this);
            }
        }

        /// <summary>
        /// Retrieves whether this control is focused or contains a child/descendent UI element which is in focus.
        /// </summary>
        public bool HasFocused
        {
            get
            {
                if (IsFocused)
                    return true;
                return Children.Any(x => x.HasFocused);
            }
        }
        
        private float _computeOpacity()
        {
            var ctrl = this;
            float opac = 1f;
            while(ctrl!=null)
            {
                opac *= ctrl.Opacity;
                ctrl = ctrl.Parent;
            }
            return opac;
        }

        /// <summary>
        /// Retrieves the back buffer for the control.
        /// </summary>
        public RenderTarget2D BackBuffer
        {
            get
            {
                return _userfacingtarget ?? Parent?.BackBuffer ?? GameLoop.GetInstance().GameRenderTarget;
            }
        }

        /// <summary>
        /// Occurs when a keyboard event is fired by the engine to this control.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyEvent;
        
        /// <summary>
        /// Occurs when the control's width is changed.
        /// </summary>
        public event EventHandler WidthChanged;
        /// <summary>
        /// Occurs when the control's height is changed.
        /// </summary>
        public event EventHandler HeightChanged;
        /// <summary>
        /// Occurs when the control's X coordinate is changed.
        /// </summary>
        public event EventHandler XChanged;
        /// <summary>
        /// Occurs when the control's Y coordinate is changed.
        /// </summary>
        public event EventHandler YChanged;
        /// <summary>
        /// Occurs when the control's "Visible" property is changed.
        /// </summary>
        public event EventHandler VisibleChanged;

        /// <summary>
        /// Gets or sets the minimum width of the control.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the minimum height of the control.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the maximum width of the control.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the maximum height of the control.
        /// </summary>
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

        /// <summary>
        /// Clears all child elements from the control.
        /// </summary>
        public void Clear()
        {
            while(_children.Count > 0)
            {
                RemoveChild(_children[0]);
            }
        }

        /// <summary>
        /// Gets or sets the opacity of the control.
        /// </summary>
        public float Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                value = MathHelper.Clamp(value, 0, 1);
                if (value >= 1)
                    _userfacingtarget = null;
                if (value == _opacity)
                    return;
                _opacity = value;
                
            }
        }

        /// <summary>
        /// Gets whether the control has been disposed.
        /// </summary>
        public bool Disposed
        {
            get
            {
                return _disposed;
            }
        }

        /// <summary>
        /// Gets or sets whether the control should be rendered on-screen.
        /// </summary>
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
                
            }
        }

        [Dependency]
        private UIManager _manager = null;

        /// <summary>
        /// Retrieves the control's associated <see cref="UIManager"/> instance.
        /// </summary>
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

        /// <summary>
        /// When overriden in a derived class, this method handles any keyboard events from the engine.
        /// </summary>
        /// <param name="e">A <see cref="KeyboardEventArgs"/> object containing information about the event.</param>
        protected virtual void OnKeyEvent(KeyboardEventArgs e)
        {

        }

        private Themes.Theme _theme = null;

        /// <summary>
        /// Retrieves the control's associated <see cref="Themes.Theme"/>. 
        /// </summary>
        public Theme Theme
        {
            get
            {
                if (Parent != null)
                    return Parent.Theme; //this will walk up the ui tree to the toplevel and grab the theme.
                return Manager.Theme;
            }
        }

        internal void SetTheme(Theme theme)
        {
            _theme = theme;
        }

        /// <summary>
        /// Adds a child to this control.
        /// </summary>
        /// <param name="child">The child control to add.</param>
        public virtual void AddChild(Control child)
        {
            if (_children == null)
                return;
            if (_children.Contains(child))
                return;
            _children.Add(child);
            child._parent = this;
            
        }

        /// <summary>
        /// Retrieves the parent of this control. Returns null if the control is a top-level.
        /// </summary>
        public Control Parent
        {
            get
            {
                return _parent;
            }
        }

        /// <summary>
        /// Gets or sets the width of the control.
        /// </summary>
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
                
                WidthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the height of the control.
        /// </summary>
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
                
                HeightChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the X coordinate of the control.
        /// </summary>
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
            }
        }

        /// <summary>
        /// Gets or sets the Y coordinate of the control.
        /// </summary>
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

            }
        }

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
                if (Manager.HoveredControl == this) return true;
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

        /// <summary>
        /// Creates a new instance of the <see cref="Control"/> class. 
        /// </summary>
        public Control()
        {
            GameLoop.GetInstance().Inject(this);
            _children = new List<Control>();
        }

        /// <summary>
        /// Updates the control's layout.
        /// </summary>
        /// <param name="time">The time since the last frame update.</param>
        protected virtual void OnUpdate(GameTime time)
        {

        }

        /// <summary>
        /// Removes a child from this control.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        public virtual void RemoveChild(Control child)
        {
            if (!_children.Contains(child))
                return;
            _children.Remove(child);
            
        }
        
        /// <summary>
        /// Recursively invalidate ALL child UI elements.
        /// </summary>
        public void InvalidateAll()
        {
            
            foreach (var child in Children)
                child.InvalidateAll();
        }

        private bool? _lastFocus = null;

        /// <summary>
        /// Occurs when the control gains or loses focus.
        /// </summary>
        public event EventHandler HasFocusedChanged;

        /// <summary>
        /// Retrieves an array containing all child controls.
        /// </summary>
        public Control[] Children
        {
            get
            {
                if (_children == null)
                    return new Control[0];
                return _children.ToArray();
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
            if (_userfacingtarget == null)
            {
                
            }

            if (_isVisible == false)
                return;
            if (_disposed)
                return;

            OnUpdate(time);
            if (_children == null)
                return;
            foreach (var child in Children)
            {
                child.Update(time);
            }
        }



        /// <summary>
        /// Paint the control onto its front surface. 
        /// </summary>
        /// <param name="time">The time since the last frame.</param>
        /// <param name="gfx">The graphics context used to render to the back buffer.</param>
        protected virtual void OnPaint(GameTime time, GraphicsContext gfx)
        {
            Theme.DrawControlBG(gfx, 0, 0, Width, Height);
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

        private Rectangle GetScissorRectangle()
        {
            Rectangle bounds = Bounds;
            var parent = this;
            while(parent != null)
            {
                bounds = Rectangle.Intersect(bounds, parent.Bounds);
                if (parent._userfacingtarget != null)
                    break;
                parent = parent.Parent;
            }
            return bounds;
        }

        /// <summary>
        /// Fire a render event.
        /// </summary>
        /// <param name="time">The time since the last frame.</param>
        /// <param name="gfx">The graphics context to render the control to.</param>
        public void Draw(GameTime time, GraphicsContext gfx)
        {
            if (Visible == false)
                return;
            if (Opacity <= 0)
                return;
            //If we're disabled, set the Grayout property.
            
            if (Opacity < 1 && !Manager.IgnoreControlOpacity && _userfacingtarget == null)
                _userfacingtarget = gfx.CreateRenderTarget(Width, Height);
            else if(Opacity == 1)
            {
                if(_userfacingtarget!=null)
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

            foreach(var child in Children)
            {
                child.Draw(time, gfx);
                if (child._userfacingtarget != null)
                {
                    var s = ToScreen(child.X, child.Y);
                    gfx.FillRectangle(s.X, s.Y, child.Width, child.Height, Color.White * child.Opacity, child._userfacingtarget);
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
}
