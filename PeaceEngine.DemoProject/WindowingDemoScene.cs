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
    public class WindowingDemoScene : GameScene
    {
        [ChildComponent]
        private UserInterface _ui = null;

        [ChildComponent]
        private DemoWindow _testWindow = null;

        [AutoLoad]
        private Button _minimize = null;

        protected override void OnDraw(GameTime time, GraphicsContext gfx)
        {
        }

        protected override void OnLoad()
        {
            _ui.Theme = New<UIDemoTheme>();
            _ui.Controls.Add(_minimize);
            
            _testWindow.Theme = New<UIDemoTheme>();

            _testWindow.Closed += (o, a) =>
            {
                LoadScene<DemoScene>();
            };
            _minimize.Click += (o, a) =>
            {
                _testWindow.Visible = !_testWindow.Visible;
            };
        }

        protected override void OnUnload()
        {
        }

        protected override void OnUpdate(GameTime time)
        {
            _minimize.Text = (_testWindow.Visible) ? "Minimize" : "Restore";
        }
    }
}
