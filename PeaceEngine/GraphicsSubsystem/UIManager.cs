using Plex.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
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

namespace Plex.Engine.GraphicsSubsystem
{
    /// <summary>
    /// Provides an advanced 2D user interface engine for the Peace engine.
    /// </summary>
    /// <remarks>
    ///     <para>The <see cref="UIManager"/> class is used as a way to simply add, remove and reorder top-level user interface elements in the Peace engine. Unless you need to directly access these abilities from a <see cref="IEngineComponent"/>, <see cref="IEntity"/> or <see cref="Window"/> object, you do not under any circumstances need to depend on this component.</para>
    ///     <para>This component is also not meant to be used for the opening and closing of <see cref="Window"/>s. This functionality is built directly into the <see cref="Window"/> class and available through the <see cref="WindowSystem"/> engine component.</para>
    ///     <para>In most cases, you shouldn't need to directly access the UIManager unless you are working inside the engine itself. Mods and games should use the <see cref="Window"/> and <see cref="WindowSystem"/> APIs for managing top-levels.</para>
    ///     <para>Also, <see cref="UIManager"/> is strictly meant for user interface entities. For other <see cref="IEntity"/> entities, use the <see cref="Plexgate"/> and <see cref="Layer"/> APIs.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso cref="Window"/>
    /// <seealso cref="Control"/>
    /// <seealso cref="WindowSystem"/> 
    /// <seealso cref="Plexgate"/>
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
            private Plexgate _plexgate = null;

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
                ctrl.Invalidate(true);
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

            [Dependency]
            private TextRenderer _textRenderer = null;

            public TextRenderer TextRenderer => _textRenderer;

            public bool IsFocused(Control ctrl)
            {
                return ctrl == _focused;
            }

            public void SetFocus(Control ctrl)
            {
                if (_focused == ctrl)
                    return;
                var alreadyFocused = _focused;
                _focused = ctrl;
                if (alreadyFocused != null)
                {
                    alreadyFocused.Invalidate();
                }
                ctrl.Invalidate();

            }
            
            public void Draw(GameTime time, GraphicsContext ctx)
            {
                ctx.TextScale = _ui.GUIScale;
                ctx.Opacity = 1f;
                ctx.Grayout = false;
                foreach (var ctrl in Controls)
                {
                    if (!ctrl.Visible)
                        continue;
                    if (ctrl.Opacity > 0)
                        ctrl.Draw(time, ctx);
                    if(ctrl._userfacingtarget != null)
                    {
                        ctx.BeginDraw();
                        ctx.Batch.Draw(ctrl._userfacingtarget, _ui.VirtualToScreen(new Vector2(ctrl.X, ctrl.Y)), Color.White * ctrl.Opacity);
                        ctx.EndDraw();
                    }
                    ctx.RenderOffsetX = 0;
                    ctx.RenderOffsetY = 0;

                }

                ctx.Opacity = 1f;
                ctx.Grayout = false;

                ctx.X = 0;
                ctx.Y = 0;
                ctx.Width = _plexgate.GameRenderTarget.Width;
                ctx.Height = _plexgate.GameRenderTarget.Height;

                if (ShowPerfCounters == false)
                    return;
                ctx.BeginDraw();
                _debugUpdTimer += time.ElapsedGameTime.TotalSeconds;
                if (_debugUpdTimer >= 1)
                {
                    _debug = $"{Math.Round(1 / time.ElapsedGameTime.TotalSeconds)} FPS | {GC.GetTotalMemory(false) / 1048576} MiB RAM | {Math.Round(_debugCpu.NextValue())}% CPU | Mouse scroll value: {_lastScrollValue} | Peace Engine resolution: {_plexgate.GameRenderTarget.Width}x{_plexgate.GameRenderTarget.Height} | UI virtual screen resolution: {_ui.ScreenWidth}x{_ui.ScreenHeight}";
                    _debugUpdTimer %= 1;
                }
                ctx.DrawString(_debug, Vector2.Zero, Color.White, _thememgr.Theme.GetFont(TextFontStyle.Muted), TextAlignment.Left, int.MaxValue, WrapMode.None);



                ctx.EndDraw();

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
                        _plexgate.GameRenderTarget.SaveAsPng(fstream, _plexgate.GameRenderTarget.Width, _plexgate.GameRenderTarget.Height);
                    }
                    return;
                }

                if (!DoInput)
                    return;
                if (_focused != null)
                    _focused.ProcessKeyboardEvent(e);
            }

            public void OnMouseUpdate(MouseState mouse)
            {
                if (!DoInput)
                    return;
                if (mouse == _lastMouseState)
                    return;
                _lastMouseState = mouse;
                //Propagate mouse events.
                var controls = Controls.OrderByDescending(x => Array.IndexOf(Controls, x)).ToArray();
                bool hasBeenHandled = false;
                var virtualMouseCoords = _ui.ScreenToVirtual(new Vector2(mouse.X, mouse.Y));
                mouse = new MouseState((int)virtualMouseCoords.X, (int)virtualMouseCoords.Y, mouse.ScrollWheelValue, mouse.LeftButton, mouse.MiddleButton, mouse.RightButton, mouse.XButton1, mouse.XButton2);
                foreach (var ctrl in controls)
                {
                    if (ctrl.Visible == false)
                        continue;
                     if (hasBeenHandled)
                    {
                        ctrl.ResetMouseState();
                    }
                    else
                    {
                        if (ctrl.PropagateMouseState(mouse))
                            hasBeenHandled = true;
                    }
                }

                if(mouse.ScrollWheelValue != _lastScrollValue)
                {
                    foreach (var ctrl in controls)
                        if (ctrl.PropagateScrollDelta(mouse.ScrollWheelValue - _lastScrollValue))
                            break;
                    _lastScrollValue = mouse.ScrollWheelValue;
                }
            }

            public void Update(GameTime time)
            {
                foreach (var ctrl in Controls)
                {
                    if (!ctrl.Visible)
                        continue;
                    ctrl.Update(time);
                }


            }

            public void Load(ContentManager content)
            {
                _screenshots = Path.Combine(_appdata.GamePath, "screenshots");
                if (!Directory.Exists(_screenshots))
                    Directory.CreateDirectory(_screenshots);
                _debugUpdTimer = 0;
                _debug = "";
                _debugCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");

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

        /// <summary>
        /// Retrieves a percentage scale factor for calculating the width and height of a UI element's render bounds.
        /// </summary>
        public float GUIScale
        {
            get
            {
                return (float)_plexgate.GameRenderTarget.Height / ScreenHeight;
            }
        }

        /// <summary>
        /// Maps coordinates within the UI's virtual screen space to Peace Engine's native screen space.
        /// </summary>
        /// <param name="virtualCoords">A <see cref="Vector2"/> containing coordinates in the virtual space.</param>
        /// <returns>The mapped real-space coordinates.</returns>
        public Vector2 VirtualToScreen(Vector2 virtualCoords)
        {
            return new Vector2(virtualCoords.X * ((float)_plexgate.GameRenderTarget.Width / ScreenWidth), virtualCoords.Y * ((float)_plexgate.GameRenderTarget.Height / ScreenHeight));
        }


        public TextRenderer TextRenderer
        {
            get
            {
                return _container.TextRenderer;
            }
        }

        /// <summary>
        /// Maps Peace Engine screen coordinates to the UI's virtual coordinate space.
        /// </summary>
        /// <param name="screenCoords">The screen coordinates to map.</param>
        /// <returns>The mapped virtual-space coordinates.</returns>
        public Vector2 ScreenToVirtual(Vector2 screenCoords)
        {
            if(_plexgate.GameRenderTarget == null)
                return new Vector2((screenCoords.X / 3840) * ScreenWidth, (screenCoords.Y / 2160) * ScreenHeight);
            return new Vector2((screenCoords.X / _plexgate.GameRenderTarget.Width) * ScreenWidth, (screenCoords.Y / _plexgate.GameRenderTarget.Height) * ScreenHeight);
        }

        [Dependency]
        private Plexgate _plexgate = null;


        /// <summary>
        /// Make the user interface visible.
        /// </summary>
        public void ShowUI()
        {
            _plexgate.GetLayer(LayerType.UserInterface).AddEntity(_container);
        }

        [Dependency]
        private ThemeManager _themeManager = null;

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
            _plexgate.GetLayer(LayerType.UserInterface).RemoveEntity(_container);
        }

        /// <summary>
        /// Gets the screen width available to user interface elements.
        /// </summary>
        public float ScreenWidth
        {
            get
            {
                if (_plexgate.GameRenderTarget == null)
                    return (1920 / 1080) * ScreenHeight;
                return ((float)_plexgate.GameRenderTarget.Width / (float)_plexgate.GameRenderTarget.Height) * ScreenHeight;
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
        public float ScreenHeight
        {
            get
            {
                return 2160;
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
                _container.SetFocus(ctrl);
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
            _container = _plexgate.New<UIContainer>();
            _plexgate.GetLayer(LayerType.UserInterface).AddEntity(_container);
            _startThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void _crossthreadInvoke(Action action)
        {
            if (action == null)
                return;
            if (Thread.CurrentThread.ManagedThreadId == _startThreadId)
                action.Invoke();
            else
                _plexgate.Invoke(action);
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
            _plexgate.graphicsDevice.IsFullScreen = fullscreen;
            _plexgate.graphicsDevice.ApplyChanges();
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
}
