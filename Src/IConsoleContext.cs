namespace VsixUtil
{
    public interface IConsoleContext
    {
        void Write(string text);
        void WriteLine(string text);
    }

    public static class ConsoleContextExtensions
    {
        public static void WriteLine(this IConsoleContext consoleContext)
        {
            consoleContext.WriteLine("");
        }

        public static void Write(this IConsoleContext consoleContext, string text, params object[] args)
        {
            consoleContext.Write(string.Format(text, args));
        }

        public static void WriteLine(this IConsoleContext consoleContext, string text, params object[] args)
        {
            consoleContext.WriteLine(string.Format(text, args));
        }
    }
}
