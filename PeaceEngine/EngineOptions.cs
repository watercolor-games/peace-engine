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

        public void Start<T>(string[] args) where T : GameScene
        {
            using (var game = new GameLoop(args))
            {
                game.GameName = GameName;
                game.StartingSceneType = typeof(T);
                game.Run();
            }
        }
    }
}
