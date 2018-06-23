using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;

namespace Plex.Engine.GameComponents.UI
{
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