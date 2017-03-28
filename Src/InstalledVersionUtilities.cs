using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Setup.Configuration;

namespace VsixUtil
{
    public class InstalledVersionUtilities
    {
        readonly static string[] LegacyVersions = { "10.0", "11.0", "12.0", "14.0" };

        public static IEnumerable<InstalledVersion> GetInstalledVersions(string includeVersion)
        {
            foreach(var installedVersion in GetInstalledVersions())
            {
                if(IncludeVersion(includeVersion, installedVersion.Version))
                {
                    yield return installedVersion;
                }
            }
        }

        static bool IncludeVersion(string number, Version version)
        {
            if (string.IsNullOrEmpty(number))
            {
                return true;
            }

            return number == version.Major.ToString();
        }


        public static IEnumerable<InstalledVersion> GetInstalledVersions()
        {
            foreach (var installedVersion in GetLegacyInstalledVersions())
            {
                yield return installedVersion;
            }

            foreach (var installedVersion in GetSetupInstanceInstalledVersions())
            {
                yield return installedVersion;
            }
        }

        public static IEnumerable<InstalledVersion> GetSetupInstanceInstalledVersions()
        {
            foreach (var setupInstance in GetSetupInstances())
            {
                var productPath = setupInstance.GetProductPath();
                var appPath = setupInstance.ResolvePath(productPath);
                var installationVersion = setupInstance.GetInstallationVersion();
                var version = new Version(installationVersion);
                yield return new InstalledVersion(appPath, version);
            }
        }

        public static IEnumerable<ISetupInstance2> GetSetupInstances()
        {
            var setupConfiguration = new SetupConfiguration();
            var instanceEnumerator = setupConfiguration.EnumAllInstances();
            var instances = new ISetupInstance2[3];

            while (true)
            {
                instanceEnumerator.Next(instances.Length, instances, out int instancesFetched);
                if (instancesFetched == 0)
                {
                    break;
                }

                for (var index = 0; index < instancesFetched; index++)
                {
                    yield return instances[index];
                }
            }
        }

        public static IEnumerable<InstalledVersion> GetLegacyInstalledVersions()
        {
            foreach (var legacyVersion in LegacyVersions)
            {
                var version = new Version(legacyVersion);
                var appPath = GetApplicationPathFromRegistry(version);
                if (appPath != null && File.Exists(appPath))
                {
                    yield return new InstalledVersion(appPath, version);
                }
            }
        }

        private static string GetApplicationPathFromRegistry(Version version)
        {
            var path = string.Format("SOFTWARE\\Microsoft\\VisualStudio\\{0}.0\\Setup\\VS", version.Major);
            using (var registryKey = Registry.LocalMachine.OpenSubKey(path))
            {
                if (registryKey == null)
                {
                    return null;
                }

                return (string)(registryKey.GetValue("EnvironmentPath"));
            }
        }
    }
}
