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
            using (var applicationContext = new ApplicationContext(installedVersion, true))
            {
                var remote = applicationContext.CreateInstance<Remote>();

                var exists = remote.WasCreated(installedVersion);

                Assert.That(exists, Is.True);
            }
        }

        static IEnumerable<InstalledVersion> GetInstalledVersions() => InstalledVersionUtilities.GetInstalledVersions();

        class Remote : MarshalByRefObject
        {
            public bool WasCreated(InstalledVersion installedVersion)
            {
                var extensionManager = ExtensionManagerFactory.CreateExtensionManager(installedVersion, "");
                return extensionManager != null;
            }
        }
    }
}
