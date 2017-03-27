using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace VsixUtil
{
    internal static class CommonUtil
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

        internal static string GetAssemblyVersionNumber(Version version)
        {
            return string.Format("{0}.0.0.0", GetVersionNumber(version));
        }

        internal static string GetApplicationPath(Version version, string sku = null)
        {
            switch (version)
            {
                case Version.Vs2017:
                    return GetApplicationPathFromDirectory("2017", sku);
                default:
                    return GetApplicationPathFromRegistry(version);
            }
        }

        private static string GetApplicationPathFromRegistry(Version version)
        {
            var path = string.Format("SOFTWARE\\Microsoft\\VisualStudio\\{0}.0\\Setup\\VS", CommonUtil.GetVersionNumber(version));
            using (var registryKey = Registry.LocalMachine.OpenSubKey(path))
            {
                if (registryKey == null)
                {
                    return null;
                }

                return (string)(registryKey.GetValue("EnvironmentPath"));
            }
        }

        // NOTE: Assumes VS2017 has been installed at default location.
        static string GetApplicationPathFromDirectory(string version, string sku)
        {
            if(sku == null)
            {
                return null;
            }

            var searchPaths = new []
            {
                $"%ProgramFiles(x86)%\\Microsoft Visual Studio\\{version}\\{sku}\\Common7\\IDE\\devenv.exe",
                $"%ProgramFiles(x86)%\\Microsoft Visual Studio\\{sku}\\Common7\\IDE\\devenv.exe"
            };

            foreach(var path in searchPaths)
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(path);
                if (File.Exists(expandedPath))
                {
                    return expandedPath;
                }
            }

            return null;
        }

        internal static IEnumerable<InstalledVersion> GetInstalledVersions(CommandLine commandLine)
        {
            foreach(Version version in Enum.GetValues(typeof(Version)))
            {
                if (!IncludeVersion(commandLine.Version, version))
                {
                    continue;
                }

                string appPath;
                switch(version)
                {
                    case Version.Vs2017:
                        foreach (var sku in commandLine.Skus)
                        {
                            appPath = GetApplicationPath(version, sku);
                            if(appPath != null && File.Exists(appPath))
                            {
                                yield return new InstalledVersion(appPath, version);
                            }
                        }
                        break;
                    default:
                        appPath = GetApplicationPath(version);
                        if (appPath != null && File.Exists(appPath))
                        {
                            yield return new InstalledVersion(appPath, version);
                        }
                        break;
                }
            }
        }

        static bool IncludeVersion(string number, Version version)
        {
            if (string.IsNullOrEmpty(number))
            {
                return true;
            }

            var versionNumber = GetVersionNumber(version);
            return number == versionNumber;
        }

        internal static void PrintHelp()
        {
            Console.WriteLine("vsixutil [/install] extensionPath [/version number] [/sku name] [/rootSuffix name]");
            Console.WriteLine("vsixutil /uninstall identifier [/version number] [/sku name] [/rootSuffix name]");
            Console.WriteLine("vsixutil /list [filter]");
        }
    }
}
