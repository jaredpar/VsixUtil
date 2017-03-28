using System.IO;
using NUnit.Framework;

namespace VsixUtil.Tests
{
    public class InstalledVersionsTests
    {
        public class GetInstalledVersions
        {
            [Test]
            public void ApplicationPath_FileExists()
            {
                var installedVersions = InstalledVersionUtilities.GetInstalledVersions(null);
                foreach (var installedVersion in installedVersions)
                {
                    Assert.That(File.Exists(installedVersion.ApplicationPath));
                }
            }
        }
    }
}
