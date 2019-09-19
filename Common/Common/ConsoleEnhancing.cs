using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class ConsoleEnhancing
    {
        public static void WriteWithColor(object message, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = originalColor;
        }
        public static void WriteLineWithColor(object message, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }
    }
}
