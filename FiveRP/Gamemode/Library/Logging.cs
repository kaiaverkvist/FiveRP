using System;
using System.Diagnostics;

namespace FiveRP.Gamemode.Library
{
    public static class Logging
    {
        /// <summary>
        /// Sends a default log, allowing you to choose colour.
        /// </summary>
        /// <param name="logtext">log text</param>
        /// <param name="colour">ConsoleColor</param>
        public static void Log(string logtext, ConsoleColor colour = ConsoleColor.White)
        {
            Debug.WriteLine(logtext);

            Console.ForegroundColor = colour;
            Console.WriteLine("[Log] " + DateTime.Now + ": " + logtext);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="logtext">log text</param>
        public static void LogError(string logtext)
        {
            Debug.WriteLine(logtext);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Log] " + DateTime.Now + ": " + logtext);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}