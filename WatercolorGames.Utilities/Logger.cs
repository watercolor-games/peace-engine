using System;
using System.IO;
using System.Runtime.CompilerServices;
namespace Plex.Objects
{
    /// <summary>
    /// It's like TRACE(), but complicated
    /// </summary>
    public static class Logger
    {
        // This section is used to clean up the debug output by getting rid
        // of the root source directory from logger output
        static string getSrcPath([CallerFilePath]string filePath = "")
        {
            return filePath;
        }

        static readonly int strip;

        static Logger()
        {
            strip = getSrcPath().IndexOf("peace-engine/WatercolorGames.Utilities/Logger.cs");
        }


        /// <summary>
        /// Log text to the console.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="colour">The colour to display the message in</param>
        /// <param name="filePath">Ignore this</param>
        /// <param name="lineNumber">See filePath</param>
        /// <param name="callerName">See filePath</param>
        public static void Log(string message, System.ConsoleColor colour = System.ConsoleColor.Gray, [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0, [CallerMemberName]string callerName = "")
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = colour;
                Console.WriteLine($"[{DateTime.Now}] <thread {System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("X")}> <{Path.GetFileName(filePath)}:line {lineNumber}> {callerName}: {message}");
            }
        }
    }
}
