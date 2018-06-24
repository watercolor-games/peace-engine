using Plex.Engine.GameComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine
{
    public class EngineOptions
    {
        public string GameName { get; set; } = "Peace Engine";
        public string Developer { get; set; } = "Watercolor Games";


        public void Start<T>(string[] args) where T : GameScene
        {
            using (var game = new GameLoop(args))
            {
                game.GameName = GameName;
                game.Developer = Developer;
                game.StartingSceneType = typeof(T);
                game.Run();
            }
        }
    }
}
