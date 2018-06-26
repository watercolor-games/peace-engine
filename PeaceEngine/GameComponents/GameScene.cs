using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GameComponents.UI;
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

        private GameComponent _focused = null;

        private void SetFocus(GameComponent c)
        {
            _focused = c;
        }

        private ContentManager _content = null;

        public ContentManager Content => _content;

        public readonly GameComponent.SceneComponentCollection Components = null;

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

        internal void FireKeyEvent(KeyboardEventArgs e)
        {
            OnKeyEvent(e);
            if (_focused != null)
                _focused.FireKeyEvent(e);
        }

        internal void FireMouseDragEnd(MouseEventArgs e)
        {
            OnMouseDragEnd(e);
            if (_dragging != null)
            {
                _dragging.FireMouseDragEnd(e);
                _dragging = null;
            }
        }

        internal void FireMouseDrag(MouseEventArgs e)
        {
            OnMouseDrag(e);
            if (_dragging != null)
                _dragging.FireMouseDrag(e);
        }

        internal void FireMouseDragStart(MouseEventArgs e)
        {
            OnMouseDragStart(e);
            var hovered = GetHovered(e.Position.ToVector2());
            if (hovered != null)
            {
                _dragging = hovered;
                hovered.FireMouseDragStart(e);
            }
        }

        internal void FireMouseDown(MouseEventArgs e)
        {
            OnMouseDown(e);
            //Traverse the control hierarchy and find a control that the mouse is hovering over.
            var hovered = GetHovered(e.Position.ToVector2());
            //Set the control as the current focus. If we didn't find any, this will cause control focus to be lost - i.e, the player clicked somewhere other than in the UI.
            SetFocus(hovered);
            //If we DO have a new focused control, fire a click event on it.
            if (hovered != null)
                hovered.FireMouseDown(e);
            _mousedown = hovered;
        }

        internal void FireMouseDoubleClick(MouseEventArgs e)
        {
            OnDoubleClick(e);
            //Traverse the control hierarchy and find a control that the mouse is hovering over.
            var hovered = GetHovered(e.Position.ToVector2());
            //Is the mouse on a UI element?
            if (hovered != null)
            {
                //Is it the focused UI element?
                if (hovered == _focused)
                {
                    //Fire "mouse up" event.
                    hovered.FireMouseDoubleClick(e);
                }
            }
        }

        internal void FireMouseMove(MouseEventArgs e)
        {
            OnMouseMove(e);

            //Traverse the control hierarchy and find a control that the mouse is hovering over.
            var hovered = GetHovered(e.Position.ToVector2());
            if (HoveredComponent != hovered)
            {
                //If HoveredControl isn't null, fire a mouse leave event on it.
                if (HoveredComponent != null)
                    HoveredComponent.FireMouseLeave(e);
                //If we're not null, fire a mouse enter event.
                if (hovered != null)
                {
                    hovered.FireMouseEnter(e);
                }
            }

            //Make it the "Hovered Control" so it renders as hovered.
            HoveredComponent = hovered;

            //Now we propagate the mouse move event if the control isn't null.
            if (hovered != null)
                hovered.FireMouseMove(e);
        }

        internal void FireScroll(MouseEventArgs e)
        {
            OnMouseScroll(e);
            var scrollable = GetHovered(e.Position.ToVector2());

            if (scrollable == null)
                return;

            scrollable.FireScroll(e);
        }

        protected virtual void OnMouseScroll(MouseEventArgs e) { }

        internal void FireMouseClick(MouseEventArgs e)
        {
            OnClick(e);
            //Traverse the control hierarchy and find a control that the mouse is hovering over.
            var hovered = GetHovered(e.Position.ToVector2());
            //Is the mouse on a UI element?
            if (hovered != null)
            {
                //Is it the focused UI element?
                if (hovered == _focused)
                {
                    //Fire "mouse up" event.
                    hovered.FireMouseClick(e);
                }
            }
        }

        internal void FireMouseUp(MouseEventArgs e)
        {
            OnMouseUp(e);
            //Traverse the control hierarchy and find a control that the mouse is hovering over.
            var hovered = GetHovered(e.Position.ToVector2());
            //Is the mouse on a UI element?
            if (hovered != null)
            {
                //Is it the focused UI element?
                if (hovered == _focused)
                {
                    //Fire "mouse up" event.
                    hovered.FireMouseUp(e);
                }
            }

            //If we do have a focused ui element, now would be a good time to make sure it's not rendering as "pressed"
            _mousedown = null;
        }

        public GameComponent GetHovered(Vector2 mousePosition)
        {
            //The last control in the search iteration.
            GameComponent last = null;
            //Find a top-level that the mouse is in.
            GameComponent current = Components.OrderByDescending(x => Array.IndexOf(Components.ToArray(), x)).FirstOrDefault(x => x.Visible && MouseInBounds(x, mousePosition));
            //Walk down the control's children using the same search query until the current control is null.
            while (current != null)
            {
                //Set the last control to the current
                last = current;
                //Search for a child.
                current = current.Components.OrderByDescending(x => Array.IndexOf(current.Components.ToArray(), x)).FirstOrDefault(x => x.Visible && MouseInBounds(x, mousePosition));
            }
            //Return the last control
            return last;
        }

        private bool MouseInBounds(GameComponent ctrl, Vector2 pos)
        {
            var controlScreen = ctrl.ToScreen(Vector2.Zero);
            return (pos.X >= controlScreen.X && pos.Y >= controlScreen.Y && pos.X <= controlScreen.X + ctrl.Width && pos.Y <= controlScreen.Y + ctrl.Height);
        }

        private GameComponent _dragging = null;

        public GameComponent HoveredComponent { get; private set; }

        private GameComponent _mousedown = null;

        public GameScene()
        {
            Components = new GameComponent.SceneComponentCollection(this);
        }

        public T New<T>() where T : new()
        {
            return _game.New<T>();
        }

        public void LoadScene<T>() where T : GameScene
        {
            _game.SetScene<T>();
        }

        public void Exit()
        {
            _game.Exit();
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
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Where(x => x.GetCustomAttributes(true).Any(y => y is AutoLoadAttribute)))
            {
                var component = _game.New(field.FieldType);
                field.SetValue(this, component);
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
