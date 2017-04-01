using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace VsixUtil
{
    public static class CommonUtil
    {
        internal static void PrintHelp()
        {
            Console.WriteLine("vsixutil [/install] extensionPath [/version number] [/rootSuffix name]");
            Console.WriteLine("vsixutil /uninstall identifier [/version number] [/rootSuffix name]");
            Console.WriteLine("vsixutil /list [filter]");
        }
    }
}
