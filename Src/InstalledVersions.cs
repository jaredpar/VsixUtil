using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VsixUtil
{
    public class InstalledVersions
    {
        public static IEnumerable<InstalledVersion> GetInstalledVersions(string includeVersion, string[] includeSkus)
        {
            foreach (Version version in Enum.GetValues(typeof(Version)))
            {
                if (!IncludeVersion(includeVersion, version))
                {
                    continue;
                }

                string appPath;
                switch (version)
                {
                    case Version.Vs2017:
                        foreach (var sku in includeSkus)
                        {
                            appPath = GetApplicationPath(version, sku);
                            if (appPath != null && File.Exists(appPath))
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
            if (sku == null)
            {
                return null;
            }

            var searchPaths = new[]
            {
                $"%ProgramFiles(x86)%\\Microsoft Visual Studio\\{version}\\{sku}\\Common7\\IDE\\devenv.exe",
                $"%ProgramFiles(x86)%\\Microsoft Visual Studio\\{sku}\\Common7\\IDE\\devenv.exe"
            };

            foreach (var path in searchPaths)
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(path);
                if (File.Exists(expandedPath))
                {
                    return expandedPath;
                }
            }

            return null;
        }

        static bool IncludeVersion(string number, Version version)
        {
            if (string.IsNullOrEmpty(number))
            {
                return true;
            }

            var versionNumber = CommonUtil.GetVersionNumber(version);
            return number == versionNumber;
        }
    }
}
