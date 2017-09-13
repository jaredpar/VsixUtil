using System.Linq;
using NUnit.Framework;

namespace VsixUtil.Tests
{
    public class InstalledVersionUtilitiesTests
    {
        public class GetInstalledVersions
        {
            [Test]
            public void ApplicationPath_FileExists()
            {
                var installedVersions = InstalledVersionUtilities.GetInstalledVersions();
                foreach (var installedVersion in installedVersions)
                {
                    FileAssert.Exists(installedVersion.ApplicationPath);
                }
            }

            [Test]
            public void Products()
            {
                var installedVersions = InstalledVersionUtilities.GetInstalledVersions();
                installedVersions = installedVersions.Where(iv => iv.VsVersion == VsVersion.Vs2017);

                foreach (var installedVersion in installedVersions)
                {
                    Assert.That(installedVersion.Product, Is.Not.Null);
                    Assert.That(installedVersion.Product, Does.Not.Contain("."));
                }
            }
        }

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
