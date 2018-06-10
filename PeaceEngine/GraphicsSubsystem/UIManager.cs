using Plex.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.TextRenderers;
using Plex.Engine.GUI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Plex.Engine.Themes;
using Plex.Engine.Config;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using System.Threading;
using Plex.Objects;
using RectangleF = OpenWheels.RectangleF;

namespace Plex.Engine.GraphicsSubsystem
{
    /// <summary>
    /// Provides an advanced 2D user interface engine for the Peace engine.
    /// </summary>
    /// <remarks>
    ///     <para>The <see cref="UIManager"/> class is used as a way to simply add, remove and reorder top-level user interface elements in the Peace engine. Unless you need to directly access these abilities from a <see cref="IEngineComponent"/>, <see cref="IEntity"/> or <see cref="Window"/> object, you do not under any circumstances need to depend on this component.</para>
    ///     <para>This component is also not meant to be used for the opening and closing of <see cref="Window"/>s. This functionality is built directly into the <see cref="Window"/> class and available through the <see cref="WindowSystem"/> engine component.</para>
    ///     <para>In most cases, you shouldn't need to directly access the UIManager unless you are working inside the engine itself. Mods and games should use the <see cref="Window"/> and <see cref="WindowSystem"/> APIs for managing top-levels.</para>
    ///     <para>Also, <see cref="UIManager"/> is strictly meant for user interface entities. For other <see cref="IEntity"/> entities, use the <see cref="GameLoop"/> and <see cref="Layer"/> APIs.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso cref="Window"/>
    /// <seealso cref="Control"/>
    /// <seealso cref="WindowSystem"/> 
    /// <seealso cref="GameLoop"/>
    /// <seealso cref="Layer"/>
    /// <seealso cref="IEngineComponent"/>
    /// <seealso cref="IEntity"/>
    public class UIManager : IEngineComponent, IConfigurable
    {
        public bool DoInput
        {
            get
            {
                lock(_container)
                {
                    return _container.DoInput;
                }
            }
            set
            {
                lock(_container)
                {
                    _container.DoInput = value;
                }
            }
        }

        private class UIContainer : IEntity, ILoadable, IDisposable
        {
            private bool _doInput = true;
            private int _lastScrollValue = 0;

            public bool DoInput
            {
                get
                {
                    return _doInput;
                }
                set
                {
                    _doInput = value;
                }
            }

            /// <inheritdoc/>
            public void OnGameExit()
            {

            }

            [Dependency]
            private ThemeManager _thememgr = null;

            [Dependency]
            private AppDataManager _appdata = null;

            [Dependency]
            private UIManager _ui = null;

            [Dependency]
            private ConfigManager _config = null;

            [Dependency]
            private GameLoop _GameLoop = null;

            private List<Control> _toplevels = new List<Control>();

            private Control _focused = null;
            private SpriteFont _monospace = null;
            private string _screenshots = null;
            private double _debugUpdTimer;
            private string _debug = "";
            private PerformanceCounter _debugCpu;
            private bool ShowPerfCounters = true;
            private MouseState _lastMouseState;

            public Control[] Controls
            {
                get
                {
                    return _toplevels.ToArray();
                }
            }

            public void AddControl(Control ctrl)
            {
                if (ctrl == null)
                    return;
                if (_toplevels.Contains(ctrl))
                    return;
                _toplevels.Add(ctrl);
                ctrl.SetManager(_ui);
                ctrl.SetTheme(_thememgr.Theme);
            }

            public void RemoveControl(Control ctrl, bool dispose)
            {
                if (ctrl == null)
                    return;
                if (!_toplevels.Contains(ctrl))
                    return;
                _toplevels.Remove(ctrl);
                if (dispose)
                    ctrl.Dispose();
            }

            public bool IsFocused(Control ctrl)
            {
                return ctrl == _focused;
            }

            public void SetFocus(Control ctrl)
            {
                if (_focused == ctrl)
                    return;
                _focused = ctrl;
            }

            public Control GetHovered(Vector2 mousePosition)
            {
                //The last control in the search iteration.
                Control last = null;
                //Find a top-level that the mouse is in.
                Control current = Controls.OrderByDescending(x => Array.IndexOf(Controls, x)).FirstOrDefault(x => x.Visible && MouseInBounds(x, mousePosition));
                //Walk down the control's children using the same search query until the current control is null.
                while(current != null)
                {
                    //Set the last control to the current
                    last = current;
                    //Search for a child.
                    current = current.Children.OrderByDescending(x => Array.IndexOf(current.Children, x)).FirstOrDefault(x => x.Visible && MouseInBounds(x, mousePosition));
                }
                //Return the last control
                return last;
            }

            private bool MouseInBounds(Control ctrl, Vector2 pos)
            {
                var controlScreen = ctrl.ToToplevel(0, 0);
                return (pos.X >= controlScreen.X && pos.Y >= controlScreen.Y && pos.X <= controlScreen.X + ctrl.Width && pos.Y <= controlScreen.Y + ctrl.Height);
            }

            public Scrollable GetHoveredScrollable(Vector2 mousePosition)
            {
                var hoveredControl = GetHovered(mousePosition);
                if (hoveredControl == null)
                    return null;

                var parent = hoveredControl;
                while(parent != null)
                {
                    if (parent is Scrollable)
                        return parent as Scrollable;
                    parent = parent.Parent;
                }

                return null;
            }


            public void Draw(GameTime time, GraphicsContext ctx)
            {
                foreach (var ctrl in Controls)
                {
                    if (!ctrl.Visible)
                        continue;
                    if (ctrl.Opacity == 0)
                        continue;
                    ctrl.Draw(time, ctx);

                    if(ctrl._userfacingtarget != null)
                    {
                        ctx.FillRectangle(new RectangleF(ctrl.X, ctrl.Y, ctrl.Width, ctrl.Height), ctrl._userfacingtarget, Color.White * ctrl.Opacity);
                    }
                }

                ctx.RenderOffsetX = 0;
                ctx.RenderOffsetY = 0;

                ctx.ScissorRectangle = Rectangle.Empty;

                if (ShowPerfCounters == false)
                    return;
#if !DEBUG
                ctx.DrawString(_thememgr.Theme.GetFont(TextFontStyle.Muted), $"{Math.Round(1 / time.ElapsedGameTime.TotalSeconds)} FPS", Vector2.Zero, Color.White);
                return;
#else
                _debugUpdTimer += time.ElapsedGameTime.TotalSeconds;
                if (_debugUpdTimer >= 1)
                {
                    if (_debugCpu != null)
                        _debug = $"{Math.Round(1 / time.ElapsedGameTime.TotalSeconds)} FPS | {GC.GetTotalMemory(false) / 1048576} MiB RAM | {Math.Round(_debugCpu.NextValue())}% CPU | Mouse scroll value: {_lastScrollValue}";
                    else
                        _debug = $"{Math.Round(1 / time.ElapsedGameTime.TotalSeconds)} FPS | {GC.GetTotalMemory(false) / 1048576} MiB RAM | {int.MinValue}% CPU | Mouse scroll value: {_lastScrollValue}";
                    _debugUpdTimer %= 1;
                }
                ctx.DrawString(_thememgr.Theme.GetFont(TextFontStyle.Muted), _debug, Vector2.Zero, Color.White);
#endif

            }

            public void OnKeyEvent(KeyboardEventArgs e)
            {
                if (e.Key == Keys.F4)
                {
                    ShowPerfCounters = !ShowPerfCounters;
                    return;
                }
                if (e.Key == Keys.F11)
                {
                    bool fullscreen = (bool)_config.GetValue("uiFullscreen", true);
                    fullscreen = !fullscreen;
                    _config.SetValue("uiFullscreen", fullscreen);
                    _ui.ApplyConfig();
                    return;
                }
                if (e.Key == Keys.F3)
                {
                    string fname = DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss") + ".png";
                    using (var fstream = File.OpenWrite(Path.Combine(_screenshots, fname)))
                    {
                        _GameLoop.GameRenderTarget.SaveAsPng(fstream, _GameLoop.GameRenderTarget.Width, _GameLoop.GameRenderTarget.Height);
                    }
                    return;
                }

                if (!DoInput)
                    return;
                if (_focused != null)
                    _focused.ProcessKeyboardEvent(e);
            }

            public void Update(GameTime time)
            {
                if (_smoothScrollUIElement != null)
                {
                    _smoothScrollProgress = MathHelper.Clamp(_smoothScrollProgress + ((float)time.ElapsedGameTime.TotalSeconds * 4), 0, 1);
                    int curr = (int)MathHelper.Lerp(_smoothScrollPrevious, _smoothScrollDest, _smoothScrollProgress);
                    _smoothScrollUIElement.FireScroll(new MouseEventArgs(_GameLoop.ViewportAdapter, time.ElapsedGameTime, new MouseState(0, 0, _smoothScrollCurr, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released), new MouseState(0, 0, curr, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released), MouseButton.None));
                    if (_smoothScrollProgress == 1)
                        _smoothScrollUIElement = null;
                }

                foreach (var ctrl in Controls)
                {
                    if (!ctrl.Visible)
                        continue;
                    ctrl.Update(time);
                }
            }

            public void Load(ContentManager content)
            {
                _GameLoop.MouseDragStart += _GameLoop_MouseDragStart;
                _GameLoop.MouseDrag += _GameLoop_MouseDrag;
                _GameLoop.MouseDragEnd += _GameLoop_MouseDragEnd;
                _GameLoop.MouseDown += _GameLoop_MouseDown;
                _GameLoop.MouseUp += _GameLoop_MouseUp;
                _GameLoop.MouseMove += _GameLoop_MouseMove;
                _GameLoop.MouseClicked += _GameLoop_MouseClicked;
                _GameLoop.MouseDoubleClicked += _GameLoop_MouseDoubleClicked;
                _GameLoop.MouseWheelMoved += _GameLoop_MouseWheelMoved;

                _config.GetValue("ui.smoothScrolling", true); //turn smooth scrolling on by default.

                _screenshots = Path.Combine(_appdata.GamePath, "screenshots");
                if (!Directory.Exists(_screenshots))
                    Directory.CreateDirectory(_screenshots);
                _debugUpdTimer = 0;
                _debug = "";
                try
                {
                    _debugCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                }
                catch
                {
                    Logger.Log("Cannot poll CPU usage stats because Windows is a garbage operating system and this specific install has a corrupt registry.");
                }
            }

            private Control _dragging = null;

            private void _GameLoop_MouseDragEnd(object sender, MouseEventArgs e)
            {
                if (_dragging != null)
                {
                    _dragging.FireMouseDragEnd(e.OffsetPosition(_dragging.ToToplevel(0, 0)));
                    _dragging = null;
                }
            }

            private void _GameLoop_MouseDrag(object sender, MouseEventArgs e)
            {
                if (_dragging != null)
                    _dragging.FireMouseDrag(e.OffsetPosition(_dragging.ToToplevel(0, 0)));
            }

            private void _GameLoop_MouseDragStart(object sender, MouseEventArgs e)
            {
                var hovered = GetHovered(e.Position.ToVector2());
                if(hovered != null)
                {
                    _dragging = hovered;
                    hovered.FireMouseDragStart(e.OffsetPosition(_dragging.ToToplevel(0, 0)));
                }
            }

            private float _smoothScrollProgress = 0f;
            private int _smoothScrollPrevious = 0;
            private int _smoothScrollDest = 0;
            private int _smoothScrollCurr = 0;
            private Scrollable _smoothScrollUIElement = null;
            

            private void _GameLoop_MouseWheelMoved(object sender, MouseEventArgs e)
            {
                bool doSmoothScroll = _config.GetValue("ui.smoothScrolling", true);

                var scrollable = GetHoveredScrollable(e.Position.ToVector2());

                if (scrollable == null)
                    return;

                if (doSmoothScroll)
                {
                    _smoothScrollPrevious = _smoothScrollCurr;
                    _smoothScrollDest = _smoothScrollPrevious + (e.ScrollWheelDelta/4);
                    _smoothScrollProgress = 0;
                    _smoothScrollUIElement = scrollable;
                }
                else
                {
                    scrollable.FireScroll(e.OffsetPosition(scrollable.ToToplevel(0, 0)));
                }
            }

            public Control HoveredControl { get; private set; }

            private void _GameLoop_MouseMove(object sender, MouseEventArgs e)
            {
                //Traverse the control hierarchy and find a control that the mouse is hovering over.
                var hovered = GetHovered(e.Position.ToVector2());
                if (HoveredControl != hovered)
                {
                    //If HoveredControl isn't null, fire a mouse leave event on it.
                    if (HoveredControl != null)
                        HoveredControl.FireMouseLeave(e.OffsetPosition(HoveredControl.ToToplevel(0, 0)));
                    //If we're not null, fire a mouse enter event.
                    if (hovered != null)
                        hovered.FireMouseEnter(e.OffsetPosition(hovered.ToToplevel(0, 0)));
                }
                //Make it the "Hovered Control" so it renders as hovered.
                HoveredControl = hovered;

                //Now we propagate the mouse move event if the control isn't null.
                if (hovered != null)
                    hovered.FireMouseMove(e.OffsetPosition(hovered.ToToplevel(0, 0)));
            }

            private void _GameLoop_MouseDoubleClicked(object sender, MouseEventArgs e)
            {
                //Traverse the control hierarchy and find a control that the mouse is hovering over.
                var hovered = GetHovered(e.Position.ToVector2());
                //Is the mouse on a UI element?
                if (hovered != null)
                {
                    //Is it the focused UI element?
                    if (hovered == _focused)
                    {
                        //Fire "mouse up" event.
                        hovered.FireMouseDoubleClick(e.OffsetPosition(hovered.ToToplevel(0, 0)));
                    }
                }
            }


            private void _GameLoop_MouseClicked(object sender, MouseEventArgs e)
            {
                //Traverse the control hierarchy and find a control that the mouse is hovering over.
                var hovered = GetHovered(e.Position.ToVector2());
                //Is the mouse on a UI element?
                if (hovered != null)
                {
                    //Is it the focused UI element?
                    if (hovered == _focused)
                    {
                        //Fire "mouse up" event.
                        hovered.FireMouseClick(e.OffsetPosition(hovered.ToToplevel(0, 0)));
                    }
                }
            }

            private Control _mousedown = null;

                private void _GameLoop_MouseUp(object sender, MouseEventArgs e)
            {
                //Traverse the control hierarchy and find a control that the mouse is hovering over.
                var hovered = GetHovered(e.Position.ToVector2());
                //Is the mouse on a UI element?
                if (hovered != null)
                {
                    //Is it the focused UI element?
                    if (hovered == _focused)
                    {
                        //Fire "mouse up" event.
                        hovered.FireMouseUp(e.OffsetPosition(hovered.ToToplevel(0, 0)));
                    }
                }

                //If we do have a focused ui element, now would be a good time to make sure it's not rendering as "pressed"
                _mousedown?.ResetButtonStates();
                _mousedown = null;
            }

            private void _GameLoop_MouseDown(object sender, MouseEventArgs e)
            {
                //Traverse the control hierarchy and find a control that the mouse is hovering over.
                var hovered = GetHovered(e.Position.ToVector2());
                //Set the control as the current focus. If we didn't find any, this will cause control focus to be lost - i.e, the player clicked somewhere other than in the UI.
                SetFocus(hovered);
                //If we DO have a new focused control, fire a click event on it.
                if (hovered != null)
                    hovered.FireMouseDown(e.OffsetPosition(hovered.ToToplevel(0, 0)));
                _mousedown = hovered;
            }

            public void InvalidateAll()
            {
                foreach(var ctrl in _toplevels.ToArray())
                {
                    ctrl.InvalidateAll();
                }
            }

            public void SendToBack(Control ctrl)
            {
                if (_toplevels.Contains(ctrl))
                {
                    _toplevels.Remove(ctrl);
                    _toplevels.Insert(0, ctrl);
                }
            }


            public void BringToFront(Control ctrl)
            {
                if (_toplevels.Contains(ctrl))
                {
                    _toplevels.Remove(ctrl);
                    _toplevels.Add(ctrl);
                }
            }

            public void Dispose()
            {
                Logger.Log("Clearing out ui controls...");
                while(Controls.Length>0)
                {
                    var ctrl = Controls[0];
                    RemoveControl(ctrl, true);
                }
                _debug = "";
                _debugCpu = null;
                Logger.Log("UI system is shutdown.");

            }
        }

        private UIContainer _container = null;

        [Dependency]
        private GameLoop _GameLoop = null;

        public event Action FocusGained;

        /// <summary>
        /// Make the user interface visible.
        /// </summary>
        public void ShowUI()
        {
            _GameLoop.GetLayer(LayerType.UserInterface).AddEntity(_container);
        }

        [Dependency]
        private ThemeManager _themeManager = null;

        public Control HoveredControl => _container.HoveredControl;

        public Theme Theme
        {
            get
            {
                return this._themeManager.Theme;
            }
        }

        /// <summary>
        /// Make the user interface invisible.
        /// </summary>
        public void HideUI()
        {
            _GameLoop.GetLayer(LayerType.UserInterface).RemoveEntity(_container);
        }

        /// <summary>
        /// Gets the screen width available to user interface elements.
        /// </summary>
        public int ScreenWidth
        {
            get
            {
                if (_GameLoop.GameRenderTarget == null)
                    return 1;
                return _GameLoop.GameRenderTarget.Width;
            }
        }

        /// <summary>
        /// Recursively invalidate ALL UI elements.
        /// </summary>
        public void InvalidateAll()
        {
            _crossthreadInvoke(() =>
            {
                this._container.InvalidateAll();
            });
        }

        /// <summary>
        /// Gets the screen height available to user interface elements.
        /// </summary>
        public int ScreenHeight
        {
            get
            {
                if (_GameLoop.GameRenderTarget == null)
                    return 1;
                return _GameLoop.GameRenderTarget.Height;
            }
        }

        /// <summary>
        /// Set a control to be the focused control.
        /// </summary>
        /// <param name="ctrl">The control to focus.</param>
        public void SetFocus(Control ctrl)
        {
            _crossthreadInvoke(() =>
            {
                if (IsFocused(ctrl))
                    return;
                _container.SetFocus(ctrl);
                if (ctrl != null)
                    FocusGained?.Invoke();
            });
        }

        /// <summary>
        /// Determines whether a control is in focus.
        /// </summary>
        /// <param name="ctrl">The control to check</param>
        /// <returns>Whether the control is in focus.</returns>
        public bool IsFocused(Control ctrl)
        {
            if (ctrl == null)
                return false;
            return _container.IsFocused(ctrl);
        }

        /// <summary>
        /// Add a control as a top-level.
        /// </summary>
        /// <param name="ctrl">The control to add.</param>
        public void Add(Control ctrl)
        {
            _crossthreadInvoke(() =>
            {
                if (_container.Controls.Contains(ctrl))
                    return;
                _container.AddControl(ctrl);
            });
        }

        /// <summary>
        /// Remove a control from the top-level list.
        /// </summary>
        /// <param name="ctrl">The control to remove.</param>
        /// <param name="dispose">Whether the control should be disposed.</param>
        public void Remove(Control ctrl, bool dispose = true)
        {
            _crossthreadInvoke(() =>
            {
                if (!_container.Controls.Contains(ctrl))
                    return;
                _container.RemoveControl(ctrl, dispose);
            });
        }

        /// <summary>
        /// Retrieve the text in the system clipboard if any.
        /// </summary>
        /// <returns>The text found in the clipboard. Returns null if no text is in the clipboard.</returns>
        public string GetClipboardText()
        {
            if (System.Windows.Forms.Clipboard.ContainsText() == false)
                return null;
            return System.Windows.Forms.Clipboard.GetText();
        }

        private int _startThreadId = -1;

        /// <inheritdoc/>
        public void Initiate()
        {
            _container = _GameLoop.New<UIContainer>();
            _GameLoop.GetLayer(LayerType.UserInterface).AddEntity(_container);
            _startThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void _crossthreadInvoke(Action action)
        {
            if (action == null)
                return;
            if (Thread.CurrentThread.ManagedThreadId == _startThreadId)
                action.Invoke();
            else
                _GameLoop.Invoke(action);
        }

        private bool _ignoreControlOpacity = false;

        /// <summary>
        /// Retrieves whether the engine is configured to ignore the value of any control's <see cref="Control.Opacity"/> value and instead render the control as opaque. 
        /// </summary>
        public bool IgnoreControlOpacity
        {
            get
            {
                return _ignoreControlOpacity;
            }
        }

        [Dependency]
        private ConfigManager _config = null;

        /// <inheritdoc/>
        public void ApplyConfig()
        {
            bool fullscreen = (bool)_config.GetValue("uiFullscreen", true);
            _GameLoop.IsFullScreen = fullscreen;
            _ignoreControlOpacity = (bool)_config.GetValue("uiIgnoreControlOpacity", false);
        }

        /// <summary>
        /// Bring a control to the front of the UI.
        /// </summary>
        /// <param name="_tutorialLabel">The control to move.</param>
        public void BringToFront(Control _tutorialLabel)
        { 
            _crossthreadInvoke(() =>
            {
                _container.BringToFront(_tutorialLabel);
            });
        }
    }

    public static class MouseEventArgsExtensions
    {
        public static MouseEventArgs OffsetPosition(this MouseEventArgs e, Vector2 offset)
        {
            var gl = GameLoop.GetInstance();


            offset = new Vector2((offset.X / gl.ViewportAdapter.VirtualWidth) * gl.GraphicsDevice.PresentationParameters.BackBufferWidth, (offset.Y / gl.ViewportAdapter.VirtualHeight) * gl.GraphicsDevice.PresentationParameters.BackBufferHeight);

            var prevState = new MouseState(e.PreviousState.X - (int)offset.X, e.PreviousState.Y - (int)offset.Y, e.PreviousState.ScrollWheelValue, e.PreviousState.LeftButton, e.PreviousState.MiddleButton, e.PreviousState.RightButton, e.PreviousState.XButton1, e.PreviousState.XButton2);
            var currState = new MouseState(e.CurrentState.X - (int)offset.X, e.CurrentState.Y - (int)offset.Y, e.CurrentState.ScrollWheelValue, e.CurrentState.LeftButton, e.CurrentState.MiddleButton, e.CurrentState.RightButton, e.CurrentState.XButton1, e.CurrentState.XButton2);

            var res = new MouseEventArgs(GameLoop.GetInstance().ViewportAdapter, e.Time, prevState, currState, e.Button);

            return res;
        }
    }
}
