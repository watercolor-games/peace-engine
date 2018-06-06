using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine.GUI
{
    public class Scrollable : Control
    {
        public event EventHandler<MouseEventArgs> Scrolled;

        internal void FireScroll(MouseEventArgs e)
        {
            OnMouseScroll(e);
            Scrolled?.Invoke(this, e);
        }

        protected virtual void OnMouseScroll(MouseEventArgs e) { }
    }
}
