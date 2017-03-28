using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace VsixUtil
{
    public static class CommonUtil
    {

        internal static string GetVersionNumber(Version version)
        {
            switch (version)
            {
                case Version.Vs2010:
                    return "10";
                case Version.Vs2012:
                    return "11";
                case Version.Vs2013:
                    return "12";
                case Version.Vs2015:
                    return "14";
                case Version.Vs2017:
                    return "15";
                default:
                    throw new Exception("Bad Version");
            }
        }

        internal static void PrintHelp()
        {
            Console.WriteLine("vsixutil [/install] extensionPath [/version number] [/sku name] [/rootSuffix name]");
            Console.WriteLine("vsixutil /uninstall identifier [/version number] [/sku name] [/rootSuffix name]");
            Console.WriteLine("vsixutil /list [filter]");
        }
    }
}
