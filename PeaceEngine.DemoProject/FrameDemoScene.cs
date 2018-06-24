using Microsoft.Xna.Framework;
using PeaceEngine.DemoProject.Themes;
using Plex.Engine.GameComponents;
using Plex.Engine.GameComponents.UI;
using Plex.Engine.GraphicsSubsystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaceEngine.DemoProject
{
    public class FrameDemoScene : GameScene
    {
        [ChildComponent]
        private FrameComponent _frame = null;

        [AutoLoad]
        private UserInterface _frameUI = null;

        [AutoLoad]
        private Label _uiLabel = null;

        [AutoLoad]
        private Button _back = null;

        protected override void OnDraw(GameTime time, GraphicsContext gfx)
        {
            gfx.FillRectangle(_frame.Bounds, Color.DarkSlateBlue);
        }

        protected override void OnLoad()
        {
            _frame.Components.Add(_frameUI);

            _frameUI.Controls.Add(_uiLabel);
            _uiLabel.Text = "This label is stuck inside the blue box.";
            _uiLabel.ToolTip = "Try dragging this label out of the blue box. The blue area is a Frame, which is used to constrain child components to a specific area on-screen.";

            _frameUI.Theme = New<UIDemoTheme>();

            _uiLabel.MouseDragStart += _uiLabel_MouseDragStart;
            _uiLabel.MouseDrag += _uiLabel_MouseDrag;

            _back.Text = "Leave";
            _frameUI.Controls.Add(_back);

            _back.Click += (o, a) =>
            {
                LoadScene<DemoScene>();
            };
        }

        private void _uiLabel_MouseDrag(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
        {
            var pos = _uiLabel.ToScreen(e.Position.X, e.Position.Y);
            var diff = _labelMousePos - pos;
            _uiLabel.X -= (int)diff.X;
            _uiLabel.Y -= (int)diff.Y;
            _labelMousePos = pos;
        }

        private void _uiLabel_MouseDragStart(object sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
        {
            _labelMousePos = _uiLabel.ToScreen(e.Position.X, e.Position.Y);
        }

        private Vector2 _labelMousePos = Vector2.Zero;

        protected override void OnUnload()
        {
        }

        protected override void OnUpdate(GameTime time)
        {
            _frame.X = (Width / 8);
            _frame.Width = Width - (_frame.X*2);
            _frame.Height = Height / 2;
            _frame.Y = (Height - _frame.Height) / 2;

            _back.X = 15;
            _back.Y = _frameUI.Height - _back.Height - 15;
        }
    }
}
