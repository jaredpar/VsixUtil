using System;
using System.IO;

namespace VsixUtil
{
    public class ConsoleContext : MarshalByRefObject, IConsoleContext
    {
        TextWriter outputWriter;

        public ConsoleContext(TextWriter outputWriter)
        {
            this.outputWriter = outputWriter;
        }

        public void Write(string text)
        {
            outputWriter.Write(text);
        }

        public void WriteLine(string text)
        {
            outputWriter.WriteLine(text);
        }
    }
}
