using System;
using Plex.Engine;

namespace PeaceEngine.DemoProject
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var game = new EngineOptions()
            {
                GameName = "Peace Engine Demo",
                Developer = "Watercolor Games"
            };
            game.Start<DemoScene>(args);
        }
    }
}
