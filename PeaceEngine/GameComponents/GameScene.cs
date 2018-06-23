using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents
{
    public abstract class GameScene
    {
        [Dependency]
        private GameLoop _game = null;

        private ContentManager _content = null;

        public ContentManager Content => _content;

        public readonly GameComponent.SceneComponentCollection Components = null;

        public GameScene()
        {
            Components = new GameComponent.SceneComponentCollection(this);
        }

        public void Update(GameTime time)
        {
            OnUpdate(time);
            foreach (var component in Components)
                component.Update(time);
        }
        
        public void Draw(GameTime time, GraphicsContext gfx)
        {
            OnDraw(time, gfx);
            foreach (var component in Components)
                component.Draw(time, gfx);
        }

        public int Width => _game.GameRenderTarget.Width;
        public int Height => _game.GameRenderTarget.Height;

        private void LoadComponents()
        {
            var type = this.GetType();
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Where(x => x.GetCustomAttributes(true).Any(y => y is ChildComponentAttribute)))
            {
                if (!field.FieldType.Inherits(typeof(GameComponent)))
                    throw new InvalidOperationException(string.Format("An attempt was made to load a GameScene object with a child component field that is of type {0}. This type, however, does not inherit GameComponent.", field.FieldType.FullName));

                var component = (GameComponent)_game.New(field.FieldType);
                field.SetValue(this, component);
                Components.Add(component);
            }
        }

        internal void Load()
        {
            _content = _game.CreateContentManager();

            LoadComponents();

            OnLoad();
        }

        internal void Unload()
        {
            OnUnload();
            Content.Unload();
            Components.Clear();
        }

        protected abstract void OnUnload();
        protected abstract void OnLoad();
        protected abstract void OnUpdate(GameTime time);
        protected abstract void OnDraw(GameTime time, GraphicsContext gfx);

    }
}
