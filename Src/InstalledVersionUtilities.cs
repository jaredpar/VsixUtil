using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using Microsoft.VisualStudio.Setup.Configuration;
using System.Linq;
using System.Runtime.InteropServices;

namespace VsixUtil
{
    public class InstalledVersionUtilities
    {
        readonly static VsVersion[] LegacyVersions = { VsVersion.Vs2010, VsVersion.Vs2012, VsVersion.Vs2013, VsVersion.Vs2015 };
        const uint REGDB_E_CLASSNOTREG = 0x80040154;

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

                var vsVersion = VsVersionUtil.GetVsVersion(version);
                var product = setupInstance.GetProduct().GetId().Split('.').Last();
                yield return new InstalledVersion(appPath, vsVersion, product);
            }
        }

        public static IEnumerable<ISetupInstance2> GetSetupInstances()
        {
            SetupConfiguration setupConfiguration;
            try
            {
                setupConfiguration = new SetupConfiguration();
            }
            catch (COMException e)
            {
                unchecked
                {
                    // Return no insances when VS2017 isn't installed.
                    if (e.ErrorCode == (int)REGDB_E_CLASSNOTREG)
                    {
                        yield break;
                    }
                }

                throw;
            }

            var instanceEnumerator = setupConfiguration.EnumAllInstances();
            var instances = new ISetupInstance2[3];

            while (true)
            {
                int instancesFetched;
                instanceEnumerator.Next(instances.Length, instances, out instancesFetched);
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
            foreach (var vsVersion in LegacyVersions)
            {
                var majorVersion = VsVersionUtil.GetVersionNumber(vsVersion);
                var appPath = GetApplicationPathFromRegistry(majorVersion);
                if (appPath != null && File.Exists(appPath))
                {
                    yield return new InstalledVersion(appPath, vsVersion);
                }
            }
        }

        private static string GetApplicationPathFromRegistry(int majorVersion)
        {
            var path = string.Format("SOFTWARE\\Microsoft\\VisualStudio\\{0}.0\\Setup\\VS", majorVersion);
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
