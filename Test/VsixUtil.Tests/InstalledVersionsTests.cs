using System.IO;
using NUnit.Framework;
using System.Linq;

namespace VsixUtil.Tests
{
    public class InstalledVersionsTests
    {
        public class GetInstalledVersions
        {
            [Test]
            public void ApplicationPath_FileExists()
            {
                var installedVersions = InstalledVersionUtilities.GetInstalledVersions();
                foreach (var installedVersion in installedVersions)
                {
                    Assert.That(File.Exists(installedVersion.ApplicationPath));
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
    }
}
