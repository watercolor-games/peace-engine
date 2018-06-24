using Microsoft.Xna.Framework;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GameComponents.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaceEngine.DemoProject
{
    public class DemoWindow : Window
    {
        private Label _head = new Label();
        private Label _desc = new Label();

        public DemoWindow() : base()
        {
            Controls.Add(_head);
            Controls.Add(_desc);
            Title = "Demo Window";
            Width = 800;
            Height = 600;

            _head.AutoSize = true;
            _head.AutoSizeMaxWidth = Width - 30;
            _head.X = 15;
            _head.Y = 15;
            _head.TextStyle = Plex.Engine.GameComponents.UI.Themes.TextStyle.Heading1;
            _head.Text = "This is a Window.";

            _desc.Text = "Windows are game components that hold a user interface inside a client area. Windows can be dragged, maximized, minimized, closed, and can contain any UI elements you please. A window's border can also very easily be themed and customized without affecting your UI.";
            _desc.AutoSize = true;
            _desc.AutoSizeMaxWidth = _head.AutoSizeMaxWidth;
        }

        protected override void UpdateWindow(GameTime time)
        {
            _desc.X = 15;
            _desc.Y = _head.Y + _head.Height + 7;
            base.UpdateWindow(time);
        }
    }
}
