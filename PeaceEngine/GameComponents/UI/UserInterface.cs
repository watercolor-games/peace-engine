using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GameComponents.UI.Themes;
using Plex.Engine.GraphicsSubsystem;

namespace Plex.Engine.GameComponents.UI
{
    public sealed class UserInterface : GameComponent
    {
        private const double _totalTooltipHoverTime = 0.25;
        private double _tooltipHoverTime = 0;

        private ToolTipComponent _tooltip = null;

        private Control _focused = null;

        public bool IsFocused(Control ctrl)
        {
            return ctrl == _focused;
        }

        public Control GetHovered(Vector2 mousePosition)
        {
            //The last control in the search iteration.
            Control last = null;
            //Find a top-level that the mouse is in.
            Control current = Controls.OrderByDescending(x => Array.IndexOf(Controls.ToArray(), x)).FirstOrDefault(x => x.Visible && x.Enabled && MouseInBounds(x, mousePosition));
            //Walk down the control's children using the same search query until the current control is null.
            while (current != null)
            {
                //Set the last control to the current
                last = current;
                //Search for a child.
                current = current.Children.OrderByDescending(x => Array.IndexOf(current.Children.ToArray(), x)).FirstOrDefault(x => x.Visible && x.Enabled && MouseInBounds(x, mousePosition));
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
            while (parent != null)
            {
                if (parent is Scrollable)
                    return parent as Scrollable;
                parent = parent.Parent;
            }

            return null;
        }

        public void SetFocus(Control ctrl)
        {
            if (_focused == ctrl)
                return;
            _focused = ctrl;
        }

        public readonly Control.TopLevelCollection Controls = null;

        [Dependency]
        private GameLoop _GameLoop = null;

        private Theme _theme = null;

        public Theme Theme
        {
            get => _theme;
            set => _theme = value ?? throw new ArgumentNullException(nameof(value));
        }

        public UserInterface()
        {
            Controls = new Control.TopLevelCollection(this);
        }

        protected override void OnSpawn()
        {
            _GameLoop.OnKeyEvent += _GameLoop_OnKeyEvent;
            _GameLoop.MouseDragStart += _GameLoop_MouseDragStart;
            _GameLoop.MouseDrag += _GameLoop_MouseDrag;
            _GameLoop.MouseDragEnd += _GameLoop_MouseDragEnd;
            _GameLoop.MouseDown += _GameLoop_MouseDown;
            _GameLoop.MouseUp += _GameLoop_MouseUp;
            _GameLoop.MouseMove += _GameLoop_MouseMove;
            _GameLoop.MouseClicked += _GameLoop_MouseClicked;
            _GameLoop.MouseDoubleClicked += _GameLoop_MouseDoubleClicked;
            _GameLoop.MouseWheelMoved += _GameLoop_MouseWheelMoved;

            if(Theme == null)
                Theme = _GameLoop.New<EngineTheme>();

            _tooltip = _GameLoop.New<ToolTipComponent>();

            Components.Add(_tooltip);

            base.OnSpawn();
        }

        private void _GameLoop_OnKeyEvent(object sender, KeyboardEventArgs e)
        {
            _focused?.FireKeyEvent(e);
        }

        protected override void OnDespawn()
        {
            _GameLoop.OnKeyEvent -= _GameLoop_OnKeyEvent;
            _GameLoop.MouseDragStart -= _GameLoop_MouseDragStart;
            _GameLoop.MouseDrag -= _GameLoop_MouseDrag;
            _GameLoop.MouseDragEnd -= _GameLoop_MouseDragEnd;
            _GameLoop.MouseDown -= _GameLoop_MouseDown;
            _GameLoop.MouseUp -= _GameLoop_MouseUp;
            _GameLoop.MouseMove -= _GameLoop_MouseMove;
            _GameLoop.MouseClicked -= _GameLoop_MouseClicked;
            _GameLoop.MouseDoubleClicked -= _GameLoop_MouseDoubleClicked;
            _GameLoop.MouseWheelMoved -= _GameLoop_MouseWheelMoved;

            Components.Remove(_tooltip);

            base.OnDespawn();
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
            if (hovered != null)
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
            bool doSmoothScroll = true;

            var scrollable = GetHoveredScrollable(e.Position.ToVector2());

            if (scrollable == null)
                return;

            if (doSmoothScroll)
            {
                _smoothScrollPrevious = _smoothScrollCurr;
                _smoothScrollDest = _smoothScrollPrevious + (e.ScrollWheelDelta / 4);
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
                {
                    hovered.FireMouseEnter(e.OffsetPosition(hovered.ToToplevel(0, 0)));
                }
            }
            
            if(hovered != null)
            {
                //Now, for tool tips.
                _tooltip.Text = hovered.ToolTip;
                _tooltip.X = e.Position.X;
                _tooltip.Y = e.Position.Y;
                _tooltipHoverTime = 0;

            }
            else
            {
                _tooltip.Text = "";
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

        protected override void OnDraw(GameTime time, GraphicsContext gfx)
        {
            foreach (var ctrl in Controls)
            {
                if (!ctrl.Visible)
                    continue;
                if (ctrl.Opacity == 0)
                    continue;
                ctrl.Draw(time, gfx);

                if (ctrl.BackBuffer != null)
                {
                    var tint = ctrl.Enabled ? Color.White : Color.Gray;
                    gfx.FillRectangle(new RectangleF(ctrl.X, ctrl.Y, ctrl.Width, ctrl.Height), ctrl.BackBuffer, tint * ctrl.Opacity);
                }
            }

            gfx.RenderOffsetX = 0;
            gfx.RenderOffsetY = 0;

            gfx.ScissorRectangle = Rectangle.Empty;
        }

        protected override void OnUpdate(GameTime time)
        {
            Bounds = new Rectangle(0, 0, _GameLoop.GameRenderTarget.Width, _GameLoop.GameRenderTarget.Height);

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

            _tooltip.Theme = Theme;

            if(HoveredControl!=null)
            {
                _tooltipHoverTime += time.ElapsedGameTime.TotalSeconds;
                _tooltip.Visible = _tooltipHoverTime >= _totalTooltipHoverTime;
            }
            else
            {
                _tooltip.Visible = false;
            }
        }
    }
}
