using System;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;

namespace VsixUtil.Tests
{
    public class ExtensionManagerFactoryTests
    {
        [TestCaseSource(nameof(GetInstalledVersions))]
        public void GetInstalledVersions_CheckExtensionManagerWasCreated(InstalledVersion installedVersion)
        {
            var applicationPath = installedVersion.ApplicationPath;
            var vsVersion = installedVersion.VsVersion;
            using (var applicationContext = new ApplicationContext(applicationPath, vsVersion))
            {
                var remote = applicationContext.CreateInstance<Remote>();

                var exists = remote.WasCreated(applicationPath, vsVersion);

                Assert.That(exists, Is.True);
            }
        }

        static IEnumerable<InstalledVersion> GetInstalledVersions() => InstalledVersionUtilities.GetInstalledVersions();

        class Remote : MarshalByRefObject
        {
            public bool WasCreated(string applicationPath, VsVersion version)
            {
                var extensionManager = ExtensionManagerFactory.CreateExtensionManager(applicationPath, version, "");
                return extensionManager != null;
            }
        }
    }
}
