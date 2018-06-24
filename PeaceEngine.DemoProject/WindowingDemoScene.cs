using Microsoft.Xna.Framework;
using PeaceEngine.DemoProject.Themes;
using Plex.Engine.GameComponents;
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
        private DemoWindow _testWindow = null;

        protected override void OnDraw(GameTime time, GraphicsContext gfx)
        {
        }

        protected override void OnLoad()
        {
            _testWindow.Theme = New<UIDemoTheme>();
        }

        protected override void OnUnload()
        {
        }

        protected override void OnUpdate(GameTime time)
        {
        }
    }
}
