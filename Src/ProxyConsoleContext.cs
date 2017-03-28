using System;

namespace VsixUtil
{
    public class ProxyConsoleContext : MarshalByRefObject, IConsoleContext
    {
        public void Write(string text)
        {
            Console.Write(text);
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }
    }
}
