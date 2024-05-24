using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools.ColorConsole
{
    public sealed class ConsoleWriter : IConsoleWriter
    {
        private readonly IConsoleWrapper console;

        public ConsoleWriter()
            : this(new ConsoleWrapper())
        {
        }

        internal ConsoleWriter(IConsoleWrapper console)
        {
            this.console = console;
        }

        public void Write<T>(T message)
        {
            console.Write(message.ToString());
        }

        public void Write<T>(T message, ConsoleColor foreground)
        {
            WriteWithForegroundColor(() => Write<T>(message), foreground);
        }

        public void Write<T>(T message, ConsoleColor foreground, ConsoleColor background)
        {
            WriteWithForegroundAndBackgroundColors(
                () => Write<T>(message),
                foreground,
                background
            );
        }

        public void WriteLine<T>(T message)
        {
            console.WriteLine(message.ToString());
        }

        public void WriteLine<T>(T message, ConsoleColor foreground)
        {
            WriteWithForegroundColor(() => WriteLine<T>(message), foreground);
        }

        public void WriteLine<T>(T message, ConsoleColor foreground, ConsoleColor background)
        {
            WriteWithForegroundAndBackgroundColors(
                () => WriteLine<T>(message),
                foreground,
                background
            );
        }

        public void SetForeGroundColor(ConsoleColor color)
        {
            console.ForegroundColor = color;
        }

        public void SetBackGroundColor(ConsoleColor color)
        {
            console.BackgroundColor = color;
        }

        private void WriteWithForegroundColor(Action write, ConsoleColor color)
        {
            ConsoleColor previous = console.ForegroundColor;
            SetForeGroundColor(color);
            write();
            SetForeGroundColor(previous);
        }

        private void WriteWithForegroundAndBackgroundColors(
            Action write,
            ConsoleColor foreground,
            ConsoleColor background)
        {
            ConsoleColor previousForeground = console.ForegroundColor;
            ConsoleColor previousBackground = console.BackgroundColor;

            SetForeGroundColor(foreground);
            SetBackGroundColor(background);
            write();
            SetForeGroundColor(previousForeground);
            SetBackGroundColor(previousBackground);
        }
    }
}
