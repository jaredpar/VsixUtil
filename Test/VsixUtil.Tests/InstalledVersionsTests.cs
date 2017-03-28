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
                var skus = "Community;Professional;Enterprise".Split(';');
                var installedVersions = InstalledVersions.GetInstalledVersions(null, skus);
                foreach (var installedVersion in installedVersions)
                {
                    Assert.That(File.Exists(installedVersion.ApplicationPath));
                }
            }
        }
    }
}
