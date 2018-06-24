using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GameComponents.UI.Themes;
using Plex.Engine.GraphicsSubsystem;

namespace Plex.Engine.GameComponents.Windowing
{
    public abstract class Window : FrameComponent
    {
        private UserInterface _client = null;
        private FrameComponent _clientFrame = null;

        private UserInterface _titleUI = null;
        private FrameComponent _titleArea = null;

        public Control.TopLevelCollection Controls => _client.Controls;
        public Rectangle ClientBounds => _client.Bounds;

        public Theme Theme { get => _client.Theme; set => _client.Theme = value; }
        public WindowTheme WindowTheme { get; set; }
        public string Title { get; set; } = "Window";

        private Hitbox _titleHitbox = new Hitbox();
        private Hitbox _closeHitbox = new Hitbox();
        private Hitbox _minHitbox = new Hitbox();
        private Hitbox _maxHitbox = new Hitbox();

        private Vector2 _mouseLastPos = Vector2.Zero;

        public Window() : base()
        {
            _client = GameLoop.GetInstance().New<UserInterface>();
            _clientFrame = GameLoop.GetInstance().New<FrameComponent>();
            Components.Add(_clientFrame);
            _clientFrame.Components.Add(_client);

            _titleUI = GameLoop.GetInstance().New<UserInterface>();
            _titleArea = GameLoop.GetInstance().New<FrameComponent>();
            Components.Add(_titleArea);

            _titleUI.Controls.Add(_titleHitbox);
            _titleHitbox.Children.Add(_closeHitbox);
            _titleHitbox.Children.Add(_minHitbox);
            _titleHitbox.Children.Add(_maxHitbox);

            _titleHitbox.MouseDragStart += _titleHitbox_MouseDragStart;
            _titleHitbox.MouseDrag += _titleHitbox_MouseDrag;

            WindowTheme = GameLoop.GetInstance().New<EngineWindowTheme>();
        }

        private void _titleHitbox_MouseDrag(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
        {
            var pos = _titleHitbox.ToScreen(e.Position.X, e.Position.Y);
            var diff = pos - _mouseLastPos;
            X += (int)diff.X;
            Y += (int)diff.Y;
            _mouseLastPos = pos;
        }

        private void _titleHitbox_MouseDragStart(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
        {
            if(e.Button == MonoGame.Extended.Input.InputListeners.MouseButton.Left)
                _mouseLastPos = _titleHitbox.ToScreen(e.Position.X, e.Position.Y);
        }

        protected sealed override void OnSpawn()
        {
            if(Parent != null)
            {
                X = (Parent.Width - Width) / 2;
                Y = (Parent.Height - Height) / 2;
            }
            else if(Scene != null)
            {
                X = (Scene.Width - Width) / 2;
                Y = (Scene.Height - Height) / 2;
            }
            _titleArea.Components.Add(_titleUI);
            base.OnSpawn();
        }

        protected sealed override void OnDespawn()
        {
            _titleArea.Components.Remove(_titleUI);
            base.OnDespawn();
        }

        public void Close()
        {
            if (Parent != null)
                Parent.Components.Remove(this);
            if (Scene != null)
                Scene.Components.Remove(this);
        }

        protected sealed override void OnDraw(GameTime time, GraphicsContext gfx)
        {
            Theme.DrawBlankControlArea(gfx);
            WindowTheme.DrawWindowFrame(gfx, Title);
        }

        protected sealed override void OnUpdate(GameTime time)
        {
            _titleArea.X = 0;
            _titleArea.Y = 0;
            _titleArea.Width = Width;
            _titleArea.Height = WindowTheme.TitleHeight;

            _titleHitbox.X = 0;
            _titleHitbox.Y = 0;
            _titleHitbox.Width = Width;
            _titleHitbox.Height = WindowTheme.TitleHeight;
            
            _clientFrame.X = WindowTheme.BorderSize;
            _clientFrame.Y = WindowTheme.TitleHeight;
            _clientFrame.Width = Width - (WindowTheme.BorderSize * 2);
            _clientFrame.Height = Height - WindowTheme.TitleHeight - WindowTheme.BorderSize;

            UpdateWindow(time);
            base.OnUpdate(time);
        }

        protected virtual void UpdateWindow(GameTime time)
        {
            
        }
    }
}
