﻿using System;
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

namespace Plex.Engine
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Plexgate : Game
    {
        public bool uploading = false;
        public bool downloading = false;

        public GUI.PictureBox _uploadImg = new GUI.PictureBox();
        public GUI.PictureBox _downloadImg = new GUI.PictureBox();

        public event Action LoadingContent;

        internal GraphicsDeviceManager graphicsDevice;
        SpriteBatch spriteBatch;

        internal IPAddress IPAddress = null;
        internal int Port = 0;
        internal Thread ServerThread = null;

        private GUI.TextControl _objectiveTitle = new GUI.TextControl();
        private GUI.TextControl _objectiveDesc = new GUI.TextControl();
        
        public event Action Initializing;

        public void FireInitialized()
        {
            Desktop.InvokeOnWorkerThread(() =>
            {
                Initializing?.Invoke();
            });
        }

#if DEBUG
        private GUI.TextControl DebugText = new GUI.TextControl();
        private GUI.TextControl Watermark = new GUI.TextControl();
#endif
        private GUI.TextControl SystemError = new GUI.TextControl();
        private GUI.TextControl SystemErrorText = new GUI.TextControl();



        public bool IsInTutorial = false;
        public Rectangle MouseEventBounds;
        public string TutorialOverlayText = "";
        public Action TutorialOverlayCompleted = null;

        //Crash variables
        public bool IsCrashed = false;
        public double CrashAnimMS = 0.0;
        


        public void Crash()
        {
            CrashAnimMS = 0;
            IsCrashed = true;
        }


        private const double failFadeMaxMS = 500;

        public RenderTarget2D GameRenderTarget = null;

        public Color UITint = Color.White;

        private bool DisplayDebugInfo = false;

        private KeyboardListener keyboardListener = new KeyboardListener ();

        public Plexgate()
        {
            graphicsDevice = new GraphicsDeviceManager(this);
            var uconf = Objects.UserConfig.Get();
            graphicsDevice.PreferredBackBufferHeight = uconf.ScreenHeight;
            graphicsDevice.PreferredBackBufferWidth = uconf.ScreenWidth;
            UIManager.Viewport = new System.Drawing.Size(
                    uconf.ScreenWidth,
                    uconf.ScreenHeight
                );

            Content.RootDirectory = "Content";
            graphicsDevice.PreferMultiSampling = false;

            //Make window borderless
            Window.IsBorderless = false;

            //Set the title
            Window.Title = "Plex";



            //Fullscreen
            graphicsDevice.IsFullScreen = uconf.Fullscreen;

            // keyboard events
            keyboardListener.KeyPressed += KeyboardListener_KeyPressed;


            
            
#if DEBUG
            DebugText.Visible = true;
            DebugText.AutoSize = true;
            UIManager.AddHUD(DebugText);
#endif
            UIManager.AddHUD(_objectiveTitle);
            UIManager.AddHUD(_objectiveDesc);
            UIManager.AddHUD(SystemError);
            SystemError.Visible = false;
            UIManager.AddHUD(SystemErrorText);
            SystemErrorText.Visible = false;

            //Frametime not limited to 16.66 Hz / 60 FPS
            IsFixedTimeStep = true;
            graphicsDevice.SynchronizeWithVerticalRetrace = true;
            graphicsDevice.GraphicsProfile = GraphicsProfile.HiDef;

            UIManager.AddHUD(_uploadImg);
            UIManager.AddHUD(_downloadImg);

        }

        private string _threadid = "";

        public string ThreadID
        {
            get
            {
                return _threadid;
            }
        }

        private void KeyboardListener_KeyPressed(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Keys.F11)
            {
                UIManager.Fullscreen = !UIManager.Fullscreen;
            }
            else if (e.Modifiers.HasFlag(KeyboardModifiers.Control) && e.Key == Keys.D)
            {
                highestfps = 0;
                DisplayDebugInfo = !DisplayDebugInfo;
            }
            else if (e.Modifiers.HasFlag(KeyboardModifiers.Control) && e.Key == Keys.E)
            {
                UIManager.ExperimentalEffects = !UIManager.ExperimentalEffects;
            }
            else
            {
                // Notice: I would personally recommend just using KeyboardEventArgs instead of KeyEvent
                // from now on, but what ever. -phath0m
                UIManager.ProcessKeyEvent(new KeyEvent(e));
            }
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ServerManager.BuildBroadcastHandlerDB();

            ATextRenderer strategy = null;
            try
            {
                strategy = new Engine.TextRenderers.NativeTextRenderer();
            }
            catch
            {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    strategy = new Engine.TextRenderers.WindowsFormsTextRenderer();
                }
                else
                {
                    strategy = new Engine.TextRenderers.GdiPlusTextRenderer();
                }
			}
			
			TextRenderer.Init(strategy);
			Console.WriteLine(strategy.GetType().ToString());


            //Before we do ANYTHING, we've got to initiate the Plex engine.
            UIManager.GraphicsDevice = GraphicsDevice;



            
            UIManager.Init(this);

            Initializing?.Invoke();

            base.Initialize();

        }


        private Texture2D MouseTexture = null;

         /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _threadid = Thread.CurrentThread.ManagedThreadId.ToString(); ;
            GameRenderTarget = new RenderTarget2D(graphicsDevice.GraphicsDevice, UIManager.Viewport.Width, UIManager.Viewport.Height, false, graphicsDevice.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);

            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(base.GraphicsDevice);

            // TODO: use this.Content to load your game content here
            var bmp = Engine.Properties.Resources.cursor_9x_pointer;
            var _lock = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] rgb = new byte[Math.Abs(_lock.Stride) * _lock.Height];
            Marshal.Copy(_lock.Scan0, rgb, 0, rgb.Length);
            bmp.UnlockBits(_lock);
            MouseTexture = new Texture2D(GraphicsDevice, bmp.Width, bmp.Height);
            MouseTexture.SetData<byte>(rgb);
            rgb = null;


            LoadingContent?.Invoke();
        }




        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            MouseTexture = null;

            ServerThread?.Abort();
            // TODO: Unload any non ContentManager content here
        }
        
        private double mouseMS = 0;

        private MouseState LastMouseState;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            keyboardListener.Update(gameTime);
            if (UIManager.CrossThreadOperations.Count > 0)
            {
                var action = UIManager.CrossThreadOperations.Dequeue();
                action?.Invoke();
            }
            if (IsActive)
            {
                //Let's get the mouse state
                var mouseState = Mouse.GetState(this.Window);
                bool prc = true;
                int x = mouseState.X;
                int y = mouseState.Y;
                bool lastclicked = LastMouseState.LeftButton == ButtonState.Pressed;
                LastMouseState = new MouseState(mouseState.X, mouseState.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
                if (IsInTutorial)
                {
                    if (!(x >= MouseEventBounds.X && x <= MouseEventBounds.Right) || !(y >= MouseEventBounds.Y && y <= MouseEventBounds.Bottom))
                        prc = false;
                }
                if (prc == true)
                {

                    UIManager.ProcessMouseState(LastMouseState, mouseMS);
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        mouseMS = 0;
                        if (IsInTutorial && lastclicked == false)
                        {
                            IsInTutorial = false;
                            TutorialOverlayCompleted?.Invoke();

                        }
                    }
                    else
                    {
                        mouseMS += gameTime.ElapsedGameTime.TotalMilliseconds;

                    }
                }
            }
            //So we have mouse input, and the UI layout system working...

            //But an OS isn't useful without the keyboard!

            //Let's see how keyboard input works.



            //Cause layout update on all elements
            UIManager.LayoutUpdate(gameTime);

            //Some hackables have a connection timeout applied to them.
            //We must update timeout values here, and disconnect if the timeout
            //hits zero.

        
#if DEBUG
            DebugText.Visible = DisplayDebugInfo;
            if (DebugText.Visible)
            {
                DebugText.X = 5;
                DebugText.Y = 5;
                

            }

            Watermark.X = (1280 - Watermark.Width) / 2;
            Watermark.Y = (720 - Watermark.Height) / 2;
#endif

            _uploadImg.Visible = uploading;
            _uploadImg.Width = 64;
            _uploadImg.Height = 64;
            _uploadImg.X = (UIManager.Viewport.Width - _uploadImg.Width) - 15;
            _uploadImg.Y = (UIManager.Viewport.Height - _uploadImg.Height) - 15;

            _downloadImg.Visible = downloading;
            _downloadImg.Width = 64;
            _downloadImg.Height = 64;
            _downloadImg.Y = _uploadImg.Y;
            _downloadImg.X = _uploadImg.X - _downloadImg.Width - 15; 



            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            UIManager.DrawControlsToTargets(GraphicsDevice, spriteBatch);
            UIManager.DrawHUDToTargets(GraphicsDevice, spriteBatch);

            graphicsDevice.GraphicsDevice.SetRenderTarget(GameRenderTarget);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
                            SamplerState.LinearWrap, DepthStencilState.Default,
                            RasterizerState.CullNone);
            //Create a graphics context so we can draw shit
            //Draw the desktop BG.
            UIManager.DrawBackgroundLayer(GraphicsDevice, spriteBatch, 640, 480);

            spriteBatch.End();


            //The desktop is drawn, now we can draw the UI.
            UIManager.DrawTArgets(spriteBatch);

            //Since we've drawn all the shrouds and stuff...
            //we can draw the HUD.
            UIManager.DrawHUD(spriteBatch);


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
                            SamplerState.LinearWrap, DepthStencilState.Default,
                            RasterizerState.CullNone);

            //Draw a mouse cursor
            var mousepos = LastMouseState;
            spriteBatch.Draw(MouseTexture, new Rectangle(mousepos.X + 1, mousepos.Y + 1, MouseTexture.Width, MouseTexture.Height), Color.Black * 0.5f);
            spriteBatch.Draw(MouseTexture, new Rectangle(mousepos.X, mousepos.Y, MouseTexture.Width, MouseTexture.Height), Color.White);

            spriteBatch.End();
        
            graphicsDevice.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                            SamplerState.LinearWrap, DepthStencilState.Default,
                            RasterizerState.CullNone);
            spriteBatch.Draw(GameRenderTarget, new Rectangle(0, 0, graphicsDevice.PreferredBackBufferWidth, graphicsDevice.PreferredBackBufferHeight), Color.White);
            spriteBatch.End();
        
            framesdrawn++;
            base.Draw(gameTime);
#if DEBUG
            var color = Color.White;
            double fps = Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds);
            if (fps <= 20)
                color = Color.Red;
            highestfps = Math.Max(highestfps, fps);
            TotalFPS += fps;
            DebugText.Text = $@"Plex
=======================

Copyright (c) 2017 Plex Developers

Debug information

CTRL+D: toggle debug menu
CTRL+E: toggle experimental effects (experimental effects enabled: {UIManager.ExperimentalEffects})
Use the ""debug"" Terminal Command for engine debug commands.

FPS: {fps}
Average FPS: {TotalFPS / framesdrawn}
Current time: {DateTime.Now}
Memory usage: {(GC.GetTotalMemory(false) / 1024) / 1024} MB
";
#endif
        }
        public double TotalFPS = 0;
        public int framesdrawn = 0;
        public double highestfps = 0;
    }

    
    [Obsolete("Use Content Pipeline.")]
    public static class ImageExtensioons
    {
        public static Texture2D ToTexture2D(this System.Drawing.Image image, GraphicsDevice device)
        {
            var bmp = (System.Drawing.Bitmap)image;
            var lck = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = new byte[Math.Abs(lck.Stride) * lck.Height];
            Marshal.Copy(lck.Scan0, data, 0, data.Length);
            bmp.UnlockBits(lck);
            for (int i = 0; i < data.Length; i += 4)
            {
                byte r = data[i];
                byte b = data[i + 2];
                data[i] = b;
                data[i + 2] = r;
            }
            var tex2 = new Texture2D(device, bmp.Width, bmp.Height);
            tex2.SetData<byte>(data);
            return tex2;
        }
    }

}
