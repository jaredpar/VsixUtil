using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace VsixUtil.Tests
{
    public class ApplicationContextTests
    {
        [TestCaseSource(nameof(GetInstalledVersions))]
        public void ExtensionManagerAssembly_HasExpectedVersion(InstalledVersion installedVersion)
        {
            using (var applicationContext = new ApplicationContext(installedVersion))
            {
                var remote = applicationContext.CreateInstance<Remote>();

                var version = remote.GetAssemblyVersion("Microsoft.VisualStudio.ExtensionManager");

                var versionNumber = VsVersionUtil.GetVersionNumber(installedVersion.VsVersion);
                Assert.That(version.Major, Is.EqualTo(versionNumber));
            }
        }

        [TestCaseSource(nameof(GetInstalledVersions))]
        public void ExtensionManagerImplementationAssembly_HasExpectedVersion(InstalledVersion installedVersion)
        {
            using (var applicationContext = new ApplicationContext(installedVersion))
            {
                var remote = applicationContext.CreateInstance<Remote>();

                var version = remote.GetAssemblyVersion("Microsoft.VisualStudio.ExtensionManager.Implementation");

                var versionNumber = VsVersionUtil.GetVersionNumber(installedVersion.VsVersion);
                Assert.That(version.Major, Is.EqualTo(versionNumber));
            }
        }

        static IEnumerable<InstalledVersion> GetInstalledVersions() => InstalledVersionUtilities.GetInstalledVersions();

        class Remote : MarshalByRefObject
        {
            public Version GetAssemblyVersion(string assemblyName)
            {
                var assembly = Assembly.Load(assemblyName);
                return assembly.GetName().Version;
            }
        }
    }
}
