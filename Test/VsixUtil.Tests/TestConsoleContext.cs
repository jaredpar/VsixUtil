using System;
using System.IO;

namespace VsixUtil.Tests
{
    class TestConsoleContext : IConsoleContext
    {
        TextWriter writer = new StringWriter();

        public void Write(string text)
        {
            writer.Write(text);
        }

        public void WriteLine(string text)
        {
            writer.WriteLine(text);
        }
    }
}
