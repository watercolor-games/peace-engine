using Microsoft.Xna.Framework;
using Plex.Engine.Config;
using Plex.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended.Input.InputListeners;
using System.IO;

namespace Plex.Engine.EngineServices
{
    public class ScreenshotService : IEngineModule
    {
        [Dependency]
        private GameLoop _loop = null;

        [Dependency]
        private AppDataManager _appdata = null;

        private string _screenshotPath = null;

        public void Initiate()
        {
            _loop.OnKeyEvent += _loop_OnKeyEvent;
            _screenshotPath = Path.Combine(_appdata.GamePath, "screenshots");
            if (!Directory.Exists(_screenshotPath))
                Directory.CreateDirectory(_screenshotPath);
        }

        private void _loop_OnKeyEvent(object sender, KeyboardEventArgs e)
        {
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.F3)
            {
                string filename = DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss") + ".png";
                using (var stream = File.Open(Path.Combine(_screenshotPath, filename), FileMode.OpenOrCreate))
                {
                    _loop.GameRenderTarget.SaveAsPng(stream, _loop.GameRenderTarget.Width, _loop.GameRenderTarget.Height);
                }
            }
        }
    }
}
