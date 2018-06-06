
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Newtonsoft.Json;
using Plex.Engine;
using Plex.Engine.GraphicsSubsystem;
using Plex.Objects;
using Plex.Engine.Interfaces;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using MonoGame.Extended.ViewportAdapters;

namespace Plex.Engine
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public sealed class GameLoop : Game
    {
        #region Constants

        public const int                BaseRenderHeight = 1080;

        #endregion

        #region Static fields

        private static GameLoop         _instance = null;

        #endregion

        #region Private fields

        private Queue<Action>           _actions = new Queue<Action>();
        private List<ComponentInfo>     _components = new List<ComponentInfo>();
        private GraphicsContext         _ctx = null;
        private Layer[]                 _layers = null;
        private float                   _renderScale = 1.0f;
        private SpriteFont              _font =  null;
        private Texture2D               _logo =   null;
        private volatile string         _status = "";
        private volatile float          _percentage = 0f;
        private volatile bool           _loaded = false;
        private Task                    _loadTask = null;
        private ManualResetEvent        _componentLoaded = new ManualResetEvent(true);
        private MouseListener           _mouseListener = null;
        private ViewportAdapter         _viewport = null;
        private GraphicsDeviceManager   _graphicsDevice;
        private SpriteBatch             _spriteBatch;
        private KeyboardListener        _keyboardListener = new KeyboardListener();
        private int                     _width = 0;
        private int                     _height = 0;
        private Texture2D               _mouseTexture = null;

        #endregion

        #region Events

        public event EventHandler                   FrameDrawn;
        public event EventHandler<MouseEventArgs>   MouseMove;
        public event EventHandler<MouseEventArgs>   MouseDown;
        public event EventHandler<MouseEventArgs>   MouseUp;
        public event EventHandler<MouseEventArgs>   MouseWheelMoved;
        public event EventHandler<MouseEventArgs>   MouseClicked;
        public event EventHandler<MouseEventArgs>   MouseDoubleClicked;
        public event EventHandler<MouseEventArgs>   MouseDragStart;
        public event EventHandler<MouseEventArgs>   MouseDragEnd;
        public event EventHandler<MouseEventArgs>   MouseDrag;


        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="GameLoop"/> game loop. 
        /// </summary>
        public GameLoop(string[] args)
        {
            if (_instance != null)
                throw new InvalidOperationException("GameLoop is already running! You cannot create multiple instances of GameLoop at the same time in one process. Instead, please let the already-running instance shut down fully.");
            _graphicsDevice = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Content = new PlexContentManager.PlexContentManager(this);
            _graphicsDevice.PreferMultiSampling = false;
            //Make window borderless
            Window.IsBorderless = false;
            //Set the title
            Window.Title = "Peace Engine";
            int w = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int h = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphicsDevice.PreferredBackBufferWidth = w;
            _graphicsDevice.PreferredBackBufferHeight = h;

            //Fullscreen
            _graphicsDevice.IsFullScreen = false;

            // keyboard events
            _keyboardListener.KeyPressed += KeyboardListener_KeyPressed;

            IsFixedTimeStep = true;
            _graphicsDevice.SynchronizeWithVerticalRetrace = true;

            int layerCount = Enum.GetValues(typeof(LayerType)).Length;
            _layers = new Layer[layerCount];
            for (int i = 0; i < layerCount; i++)
            {
                _layers[i] = new Engine.Layer();
            }

            Args = args;
            QuietMode = args.Contains("-q");
        }

        #endregion

        #region Properties

        public ViewportAdapter          ViewportAdapter => _viewport;
        public int                      BackBufferWidth => GraphicsDevice.PresentationParameters.BackBufferWidth;
        public int                      BackBufferHeight => GraphicsDevice.PresentationParameters.BackBufferHeight;
        public float                    AspectRatio => (float)GraphicsDevice.PresentationParameters.BackBufferWidth / GraphicsDevice.PresentationParameters.BackBufferHeight;


        /// <summary>
        /// Gets or sets whether the game window is full-screen.
        /// </summary>
        public bool IsFullScreen
        {
            get
            {
                return _graphicsDevice.IsFullScreen;
            }
            set
            {
                _graphicsDevice.IsFullScreen = value;
                _graphicsDevice.ApplyChanges();
            }
        }


        public float RenderScale
        {
            get
            {
                return _renderScale;
            }
            set
            {
                if (_renderScale == value)
                    return;
                _renderScale = value;
                GameRenderTarget = null;
            }
        }

        /// <summary>
        /// The command-line arguments passed to the game executable.
        /// </summary>
        public string[] Args { get; private set; }

        /// <summary>
        /// The game will load the single-player desktop as quickly as possible.
        /// </summary>
        public bool QuietMode { get; private set; }

        /// <summary>
        /// The main menu has been shut down once.
        /// </summary>
        public bool GameStarted { get; set; } = false;

        /// <summary>
        /// The main render target for the game.
        /// </summary>
        public RenderTarget2D GameRenderTarget { get; private set; }

        #endregion

        #region Internal methods

        internal static GameLoop GetInstance()
        {
            if (_instance == null)
                throw new InvalidOperationException("GameLoop has not been initiated. Therefore, you can't access its context.");
            return _instance;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Create a new instance of the specified type, injecting all dependencies.
        /// </summary>
        /// <typeparam name="T">The type to instantiate</typeparam>
        /// <returns>The new instance.</returns>
        public T New<T>() where T : new()
        {
            var obj = (T)Inject(new T());
            if (obj is ILoadable)
                (obj as ILoadable).Load(Content);
            return obj;
        }
        /// <summary>
        /// Creates a new instance of the specified type and injects all dependencies.
        /// </summary>
        /// <param name="t">The type of the object to create.</param>
        /// <returns>The newly created object with all dependencies injected.</returns>
        /// <exception cref="ArgumentException">The specified type doesn't define a public parameterless constructor.</exception> 
        public object New(Type t)
        {
            if (t.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException($"{t.Name} does not provide a parameterless constructor.");
            var obj = Inject(Activator.CreateInstance(t, null));
            (obj as ILoadable)?.Load(Content);
            return obj;
        }


        /// <summary>
        /// Invoke an action on the next game update.
        /// </summary>
        /// <param name="act">The action to invoke.</param>
        public void Invoke(Action act)
        {
            if (act == null)
                return;
            _actions.Enqueue(act);
        }


        /// <summary>
        /// Retrieves all types on which the specified type depends.
        /// </summary>
        /// <typeparam name="T">A type to scan. The scanner will only check public and non-public instance fields.</typeparam>
        /// <returns>A list of all dependency types.</returns>
        public IEnumerable<Type> GetDependencyTypes<T>()
        {
            var type = typeof(T);
            foreach (var field in type.GetFields(BindingFlags.Instance))
            {
                if (field.GetCustomAttributes(true).Any(x => x is DependencyAttribute))
                    yield return field.FieldType;
            }
        }

        /// <summary>
        /// Retrieves all types on which the specified type depends.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> describing an object to scan.</param>
        /// <returns>A list of all dependency types.</returns>
        public IEnumerable<Type> GetDependencyTypes(Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance))
            {
                if (field.GetCustomAttributes(true).Any(x => x is DependencyAttribute))
                    yield return field.FieldType;
            }
        }


        public Vector2 GetRenderScreenSize()
        {
            var scaleHeight = BaseRenderHeight / _renderScale;

            return new Vector2(AspectRatio * scaleHeight, scaleHeight);
        }

        /// <summary>
        /// Get all available screen resolutions.
        /// </summary>
        /// <returns>A list of available screen resolutions.</returns>
        public string[] GetAvailableResolutions()
        {
            var modes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes;
            List<string> _resolutions = new List<string>();
            foreach (var mode in modes.OrderByDescending(x => x.Width * x.Height))
            {
                if (mode.Height < 600)
                    continue;
                string resString = $"{mode.Width}x{mode.Height}";
                if (_resolutions.Contains(resString))
                    continue;
                _resolutions.Add(resString);
            }
            return _resolutions.ToArray();
        }

        /// <summary>
        /// Determines whether object A depends on object B.
        /// </summary>
        /// <param name="a">The object to check for dependencies.</param>
        /// <param name="b">The object to check whether object A depends on it</param>
        /// <returns>The result of the dependency check</returns>
        public bool DependsOn(object a, object b)
        {
            if (a == null || b == null)
                return false;
            var atype = a.GetType();
            var btype = b.GetType();
            return atype.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.FieldType == btype) != null;
        }

        /// <summary>
        /// Get the engine component of the specified type
        /// </summary>
        /// <param name="t">The type to search for</param>
        /// <returns>The engine component instance.</returns>
        /// <exception cref="ArgumentException">No components were found.</exception> 
        public IEngineComponent GetEngineComponent(Type t)
        {
            if (!typeof(IEngineComponent).IsAssignableFrom(t) || t.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException($"{t.Name} is not an IEngineComponent, or does not provide a parameterless constructor.");
            return _components.First(x => t.IsAssignableFrom(x.Component.GetType())).Component;
        }

        /// <summary>
        /// Inject all dependencies into the specified object.
        /// </summary>
        /// <param name="client">The object to inject dependencies into</param>
        /// <returns>The now-injected object.</returns>
        public object Inject(object client)
        {
            Type clientType = client.GetType();
            while (clientType != null)
            {
                foreach (var field in clientType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(f => f.GetCustomAttributes(true).Any(t => t is DependencyAttribute)))
                {
                    if (field.FieldType == this.GetType())
                        field.SetValue(client, this);
                    else
                        field.SetValue(client, GetEngineComponent(field.FieldType));

                }
                clientType = clientType.BaseType;
            }
            return client;
        }

        /// <summary>
        /// Retrieve the system default resolution
        /// </summary>
        /// <returns>The system default resolution string.</returns>
        public string GetSystemResolution()
        {
            var res = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            return $"{res.Width}x{res.Height}";
        }

        /// <summary>
        /// Retrieve all loaded <see cref="IEngineComponent"/> objects. 
        /// </summary>
        /// <returns>Every loaded engine component.</returns>
        public IEngineComponent[] GetAllComponents()
        {
            List<IEngineComponent> cpts = new List<IEngineComponent>();
            foreach (var cpt in _components)
                cpts.Add(cpt.Component);
            return cpts.ToArray();
        }

        /// <summary>
        /// Gets the layer represented by the specified layer type.
        /// </summary>
        /// <param name="type">The type of the layer to find</param>
        /// <returns>The found layer</returns>
        public Layer GetLayer(LayerType type)
        {
            return _layers[(int)type];
        }

        /// <summary>
        /// Change the game resolution.
        /// </summary>
        /// <param name="resolution">A resolution string in the format "WIDTHxHEIGHT".</param>
        public void ApplyResolution(string resolution)
        {
            if (string.IsNullOrWhiteSpace(resolution))
                throw new FormatException("Resolution string invalid. Proper format is \"<width>x<height>\".");

            string[] split = resolution.Split('x');
            if (split.Length != 2)
                throw new FormatException("Resolution string invalid. Proper format is \"<width>x<height>\".");

            try
            {
                _width = Convert.ToInt32(split[0]);
                _height = Convert.ToInt32(split[1]);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    _graphicsDevice.PreferredBackBufferWidth = _width;
                    _graphicsDevice.PreferredBackBufferHeight = _height;
                    _graphicsDevice.ApplyChanges();
                }
                GameRenderTarget?.Dispose();
                GameRenderTarget = null;
            }
            catch
            {
                throw new FormatException("Resolution string invalid. Proper format is \"<width>x<height>\".");

            }
        }

        #endregion

        #region Private methods

        private void ResetMouseListener()
        {
            Logger.Log("Resetting viewport and mouse listener...");

            _viewport = new BoxingViewportAdapter(Window, GraphicsDevice, (int)GetRenderScreenSize().X, (int)GetRenderScreenSize().Y);

            _mouseListener = new MouseListener(_viewport);

            //We want to delegate all of Extended's mouse events into our own. This makes it so that mouse events added by components/entities/etc persist beyond resets.
            _mouseListener.MouseClicked += _mouseListener_MouseClicked;
            _mouseListener.MouseDoubleClicked += _mouseListener_MouseDoubleClicked;
            _mouseListener.MouseDown += _mouseListener_MouseDown;
            _mouseListener.MouseDrag += _mouseListener_MouseDrag;
            _mouseListener.MouseDragStart += _mouseListener_MouseDragStart;
            _mouseListener.MouseDragEnd += _mouseListener_MouseDragEnd;
            _mouseListener.MouseMoved += _mouseListener_MouseMoved;
            _mouseListener.MouseUp += _mouseListener_MouseUp;
            _mouseListener.MouseWheelMoved += _mouseListener_MouseWheelMoved;
        }

        private void _mouseListener_MouseWheelMoved(object sender, MouseEventArgs e)
        {
            MouseWheelMoved?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse wheel moved.");
#endif
        }

        private void _mouseListener_MouseUp(object sender, MouseEventArgs e)
        {
            MouseUp?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse up.");
#endif
        }

        private void _mouseListener_MouseMoved(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse moved.");
#endif
        }

        private void _mouseListener_MouseDragEnd(object sender, MouseEventArgs e)
        {
            MouseDragEnd?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse drag end.");
#endif
        }

        private void _mouseListener_MouseDragStart(object sender, MouseEventArgs e)
        {
            MouseDragStart?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse drag start.");
#endif
        }

        private void _mouseListener_MouseDrag(object sender, MouseEventArgs e)
        {
            MouseDrag?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse drag.");
#endif
        }

        private void _mouseListener_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDown?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse down.");
#endif
        }

        private void _mouseListener_MouseDoubleClicked(object sender, MouseEventArgs e)
        {
            MouseDoubleClicked?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse double-clicked.");
#endif
        }

        private void _mouseListener_MouseClicked(object sender, MouseEventArgs e)
        {
            MouseClicked?.Invoke(sender, e);
#if DEBUG
            Logger.Log("Mouse clicked.");
#endif
        }

        private void LoadGame()
        {
            _status = "Retrieving types to load...";
            this._percentage = 0f;
            Logger.Log("Peace Engine is now initializing your game.");
            List<Type> typesToInit = new List<Type>();
            foreach (var type in ReflectMan.Types.Where(x => x.Assembly != this.GetType().Assembly && x.GetInterfaces().Contains(typeof(IEngineComponent))))
            {
                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    Logger.Log($"Found {type.Name}, but it doesn't have a parameterless constructor, so it's ignored.  Probably a mistake.", System.ConsoleColor.Yellow);
                    continue;
                }
                Logger.Log($"Found {type.Name}", System.ConsoleColor.Yellow);
                typesToInit.Add(type);
                _status = "Retrieving types to load... [" + typesToInit.Count + "]";
            }
            _status = "Constructing components...";
            foreach (var type in typesToInit)
            {
                _componentLoaded.WaitOne();
                _componentLoaded.Reset();
                Invoke(() =>
                {
                    var componentInfo = new ComponentInfo
                    {
                        IsInitiated = false,
                        Component = (IEngineComponent)Activator.CreateInstance(type, null)
                    };
                    _components.Add(componentInfo);
                    _percentage = (float)typesToInit.IndexOf(type) / typesToInit.Count;
                    _componentLoaded.Set();
                });
            }
            _componentLoaded.WaitOne();
            _status = "Injecting dependencies...";
            var componentsToInject = _components.Where(x => x.IsInitiated == false).ToList();
            foreach (var component in componentsToInject)
            {
                _componentLoaded.Reset();
                Invoke(() =>
                {
                    Logger.Log($"{component.Component.GetType().Name}: Injecting dependencies...");
                    Inject(component.Component);
                    _percentage = (float)componentsToInject.IndexOf(component) / componentsToInject.Count;
                    _componentLoaded.Set();
                });
                _componentLoaded.WaitOne();
            }
            _status = "Initializing components...";
            //I know. This is redundant. I'm only doing this as a safety precaution, to prevent crashes with modules that try to access uninitiated modules as they're initiating.
            foreach (var component in componentsToInject)
            {
                _componentLoaded.WaitOne();
                _componentLoaded.Reset();
                Invoke(() =>
                {
                    RecursiveInit(component.Component);
                    _percentage = (float)componentsToInject.IndexOf(component) / componentsToInject.Count;
                    _componentLoaded.Set();
                });
            }
            _status = "Loading assets...";
            foreach (var component in componentsToInject)
            {
                _componentLoaded.WaitOne();
                _componentLoaded.Reset();
                Invoke(() =>
                {
                    if (component.Component is ILoadable)
                        (component.Component as ILoadable).Load(Content);
                    _percentage = (float)componentsToInject.IndexOf(component) / componentsToInject.Count;
                    _componentLoaded.Set();
                });
            }

            Logger.Log("Done initiating engine.");

        }


        private void RecursiveInit(IEngineComponent component)
        {
            foreach (var field in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(f => f.GetCustomAttributes(false).Any(t => t is DependencyAttribute)))
            {
                if (field.FieldType == this.GetType())
                    continue;
                else
                {
                    var c = GetEngineComponent(field.FieldType);
                    RecursiveInit(c);
                }
            }
            if (_components.FirstOrDefault(x => x.Component == component).IsInitiated == false)
            {
                component.Initiate();
                _components.FirstOrDefault(x => x.Component == component).IsInitiated = true;
            }
        }

        private void KeyboardListener_KeyPressed(object sender, KeyboardEventArgs e)
        {
            foreach(var layer in _layers.ToArray())
            {
                foreach(var entity in layer.Entities)
                {
                    entity.OnKeyEvent(e);
                }
            }
        }

#endregion

#region MonoGame overrides

        protected override void Initialize()
        {
            GraphicsAdapter.UseDriverType = GraphicsAdapter.DriverType.Hardware;
            //MSAA (Multi-sample Anti Aliasing as requested by lempamo)
            _graphicsDevice.GraphicsProfile = GraphicsProfile.HiDef;
            _graphicsDevice.PreferMultiSampling = true;
            GraphicsDevice.PresentationParameters.MultiSampleCount = 8; //8x MSAA, should be a configurable thing
            _graphicsDevice.ApplyChanges();

            _instance = this;

            Logger.Log("Peace Engine is now initializing core engine components...");
            List<Type> typesToInit = new List<Type>();
            foreach (var type in ReflectMan.Types.Where(x => x.Assembly == this.GetType().Assembly && x.GetInterfaces().Contains(typeof(IEngineComponent))))
            {
                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    Logger.Log($"Found {type.Name}, but it doesn't have a parameterless constructor, so it's ignored.  Probably a mistake.", System.ConsoleColor.Yellow);
                    continue;
                }
                Logger.Log($"Found {type.Name}", System.ConsoleColor.Yellow);
                typesToInit.Add(type);
            }
            foreach (var type in typesToInit)
            {
                var componentInfo = new ComponentInfo
                {
                    IsInitiated = false,
                    Component = (IEngineComponent)Activator.CreateInstance(type, null)
                };
                _components.Add(componentInfo);
            }
            foreach(var component in _components)
            {
                Logger.Log($"{component.Component.GetType().Name}: Injecting dependencies...");
                Inject(component.Component);
            }
            //I know. This is redundant. I'm only doing this as a safety precaution, to prevent crashes with modules that try to access uninitiated modules as they're initiating.
            foreach (var component in _components)
            {
                RecursiveInit(component.Component);
            }
            Logger.Log("Done initiating engine.");

            ResetMouseListener();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _logo = Content.Load<Texture2D>("EngineLogo");
            _font = Content.Load<SpriteFont>("EngineFont");

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(base.GraphicsDevice);

            // TODO: use this.Content to load your game content here
            var bmp = Engine.Properties.Resources.cursor_9x_pointer;
            var _lock = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] rgb = new byte[Math.Abs(_lock.Stride) * _lock.Height];
            Marshal.Copy(_lock.Scan0, rgb, 0, rgb.Length);
            bmp.UnlockBits(_lock);
            _mouseTexture = new Texture2D(GraphicsDevice, bmp.Width, bmp.Height);
            _mouseTexture.SetData<byte>(rgb);
            rgb = null;

            foreach(var component in _components)
            {
                if (component.Component is ILoadable)
                    (component.Component as ILoadable).Load(Content);
            }
            base.LoadContent();
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _mouseTexture.Dispose();
            _mouseTexture = null;
            Logger.Log("Despawning all entities...");
            foreach(var layer in _layers)
            {
                while(layer.Entities.Length > 0)
                {
                    var entity = layer.Entities[0];
                    entity.OnGameExit();
                    if (entity is IDisposable)
                        (entity as IDisposable).Dispose();
                    layer.RemoveEntity(entity);
                }
            }
            _layers = null;
            Logger.Log("Unloading all engine modules!");
            while(_components.Count > 0)
            {
                var component = _components[0];
                Logger.Log("Unloading: " + component.Component.GetType().Name);
                if(component.Component is IDisposable)
                    (component.Component as IDisposable).Dispose();
                _components.RemoveAt(0);
            }
            Logger.Log("Done.");
            // TODO: Unload any non ContentManager content here
            _instance = null;
            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            while (_actions.Count != 0)
            {
                _actions.Dequeue().Invoke();
            }

            if (IsActive)
            {
                var renderSize = GetRenderScreenSize();
                if (GameRenderTarget == null)
                    //Setup the game's rendertarget so it matches the desired resolution.
                    GameRenderTarget = new RenderTarget2D(GraphicsDevice, (int)renderSize.X, (int)renderSize.Y, false, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, DepthFormat.Depth24, 8, RenderTargetUsage.PreserveContents);
                if (_ctx == null)
                    _ctx = new GraphicsContext(GraphicsDevice);

                _ctx.ScissorRectangle = Rectangle.Empty;
                _keyboardListener.Update(gameTime);

                var virtualSize = GetRenderScreenSize();

                if (_viewport.VirtualWidth != (int)virtualSize.X || _viewport.VirtualHeight != (int)virtualSize.Y)
                    ResetMouseListener();

                _mouseListener.Update(gameTime);
            }



            if (_loaded == false)
            {
                if(_loadTask == null)
                {
                    _loadTask = Task.Run(() => LoadGame());
                }
                else
                {
                    if(_loadTask.Exception != null && _loadTask.Exception.InnerExceptions.Count > 0)
                    {
                        Exit();
                        throw _loadTask.Exception;
                    }
                    if (_loadTask.IsCompleted)
                    {
                        
                        _loaded = true;
                        _loadTask = null;
                    }
                }
            }


            if (_loadTask == null || _loadTask.IsCompleted)
            {
                foreach (var layer in _layers.ToArray())
                {
                    foreach (var entity in layer.Entities)
                    {
                        entity.Update(gameTime);
                    }
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (GameRenderTarget == null)
                return;
            if (IsActive)
            {
                _ctx.SetRenderTarget(GameRenderTarget);
                GraphicsDevice.Clear(Color.Black);
                if (_loadTask != null && _loadTask.IsCompleted == false)
                {
                    _ctx.StartFrame(BlendState.AlphaBlend);

                    int halfWidth = _ctx.Width / 2;
                    int halfHeight = _ctx.Height / 2;
                    _ctx.FillRectangle(new Vector2((_ctx.Width - halfWidth) / 2, (_ctx.Height - halfHeight) / 2), new Vector2(halfWidth, halfHeight), _logo, ImageLayout.Zoom);

                    string status = _status;

                    string percentage = $"{Math.Round(_percentage * 100, 2)}%";

                    int textY = _ctx.Height - (_ctx.Height / 3);
                    var measure = TextRenderer.MeasureText(status, _font, halfWidth, TextRenderers.WrapMode.Words);

                    _ctx.DrawString(status, new Vector2((_ctx.Width - halfWidth) / 2, textY), Color.White, _font, TextAlignment.Center, halfWidth, TextRenderers.WrapMode.Words);

                    var pMeasure = _font.MeasureString(percentage);

                    _ctx.DrawString(_font, percentage, new Vector2((_ctx.Width - pMeasure.X) / 2, textY + measure.Y + 30), Color.White);

                    _ctx.EndFrame();
                }
                else
                {
                    _ctx.StartFrame(BlendState.AlphaBlend);
                    foreach (var layer in _layers.ToArray())
                        foreach (var entity in layer.Entities)
                        {
                            _ctx.ScissorRectangle = Rectangle.Empty;

                            entity.Draw(gameTime, this._ctx);

                        }

                }
                var mouse = Mouse.GetState();

                var mouseLoc = new Vector2(
                        ((float)mouse.X / BackBufferWidth) * _ctx.Width,
                        ((float)mouse.Y / BackBufferHeight) * _ctx.Height
                    );

                _ctx.EndFrame();
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
                                SamplerState.LinearWrap, DepthStencilState.Default,
                                RasterizerState.CullNone);
                _spriteBatch.Draw(_mouseTexture, new Rectangle((int)mouseLoc.X, (int)mouseLoc.Y, _mouseTexture.Width, _mouseTexture.Height), Color.White);

                _spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
            }
            var rstate = new RasterizerState
            {
                MultiSampleAntiAlias = true
            };
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
                            SamplerState.LinearWrap, DepthStencilState.Default,
                            rstate);
            _spriteBatch.Draw(GameRenderTarget, new Rectangle(0, 0, _ctx.Width, _ctx.Height), Color.White);
            _spriteBatch.End();
            FrameDrawn?.Invoke(this, EventArgs.Empty);
        }

#endregion
    }

    /// <summary>
    /// Contains an <see cref="IEngineComponent"/> and whether it's been initialized properly yet. 
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// Whether the component has been initialized.
        /// </summary>
        public bool IsInitiated { get; set; }
        /// <summary>
        /// The underlying component.
        /// </summary>
        public IEngineComponent Component { get; set; }
    }

    /// <summary>
    /// A class containing a list of <see cref="IEntity"/> objects that acts as a top-level entity container. 
    /// </summary>
    public sealed class Layer
    {
        private List<IEntity> _entities = new List<IEntity>();

        /// <summary>
        /// Gets a list of all entities on the layer.
        /// </summary>
        public IEntity[] Entities
        {
            get
            {
                return _entities.ToArray();
            }
        }

        /// <summary>
        /// Adds an entity to the layer.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public void AddEntity(IEntity entity)
        {
            if (entity == null)
                return;
            if (_entities.Contains(entity))
                return;
            _entities.Add(entity);
        }

        /// <summary>
        /// Removes an entity from the layer.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        public void RemoveEntity(IEntity entity)
        {
            if (entity == null)
                return;
            if (!_entities.Contains(entity))
                return;
            _entities.Remove(entity);
        }

        /// <summary>
        /// Clears all entities from the layer.
        /// </summary>
        public void ClearEntities()
        {
            _entities.Clear();
        }

        /// <summary>
        /// Sends the specified entity to the back of the layer.
        /// </summary>
        /// <param name="entity">The entity to move.</param>
        public void SendToBack(IEntity entity)
        {
            if (entity == null)
                return;
            if (!_entities.Contains(entity))
                return;
            if (_entities.IndexOf(entity) == 0)
                return;
            RemoveEntity(entity);
            _entities.Insert(0, entity);
        }

        /// <summary>
        /// Brings the specified entity to the front of the layer.
        /// </summary>
        /// <param name="entity">The entity to mve.</param>
        public void BringToFront(IEntity entity)
        {
            if (entity == null)
                return;
            if (!_entities.Contains(entity))
                return;
            if (_entities.IndexOf(entity) == _entities.Count - 1)
                return;
            RemoveEntity(entity);
            AddEntity(entity);
        }

        public bool HasEntity(IEntity entity)
        {
            return _entities.Contains(entity);
        }
    }
}
