using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptAway
{
    internal class Message
    {
        public static void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($" {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($" {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WelcomeMessage()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("================================================================================" +
                            "\n                           Welcome to CryptAway v1.0        " +
                            "\n================================================================================");
            Log("Write a command to begin. If you don't know them, you can call 'help' to get started.");

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogDev(string function, string message) {
            if (!Home.devMode) return;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" [{function}] {message}");
            Console.BackgroundColor = ConsoleColor.Black;
        }

    }
}
