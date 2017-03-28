using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VsixUtil.Tests
{
    public class InstalledVersionUtilitiesTests
    {
        public class GetLegacyInstalledVersions
        {
            [Test]
            public void ApplicationPath_AllFilesExist()
            {
                var installedVersions = InstalledVersionUtilities.GetLegacyInstalledVersions();
                foreach (var installedVersion in installedVersions)
                {
                    FileAssert.Exists(installedVersion.ApplicationPath);
                }
            }
        }

        public class GetSetupConfigurationInstalledVersions
        {
            [Test]
            public void ApplicationPath_AllFilesExist()
            {
                var installedVersions = InstalledVersionUtilities.GetSetupInstanceInstalledVersions();
                foreach (var installedVersion in installedVersions)
                {
                    FileAssert.Exists(installedVersion.ApplicationPath);
                }
            }
        }
    }
}
