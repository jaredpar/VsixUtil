using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace VsixUtil
{
    public static class CommonUtil
    {
        internal static int GetVersionNumber(VsVersion version)
        {
            switch (version)
            {
                case VsVersion.Vs2010:
                    return 10;
                case VsVersion.Vs2012:
                    return 11;
                case VsVersion.Vs2013:
                    return 12;
                case VsVersion.Vs2015:
                    return 14;
                case VsVersion.Vs2017:
                    return 15;
                default:
                    throw new Exception("Bad Version");
            }
        }

        internal static VsVersion GetVsVersion(int versionNumber)
        {
            switch (versionNumber)
            {
                case 10:
                    return VsVersion.Vs2010;
                case 11:
                    return VsVersion.Vs2012;
                case 12:
                    return VsVersion.Vs2013;
                case 14:
                    return VsVersion.Vs2015;
                case 15:
                    return VsVersion.Vs2017;
                default:
                    throw new Exception("Bad Version");
            }
        }

        internal static void PrintHelp()
        {
            Console.WriteLine("vsixutil [/install] extensionPath [/version number] [/rootSuffix name]");
            Console.WriteLine("vsixutil /uninstall identifier [/version number] [/rootSuffix name]");
            Console.WriteLine("vsixutil /list [filter]");
        }
    }
}
