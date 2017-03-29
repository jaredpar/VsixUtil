using System.IO;
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
            string applicationPath = installedVersion.ApplicationPath;
            var vsVersion = installedVersion.VsVersion;
            using (var applicationContext = new ApplicationContext(applicationPath, vsVersion))
            {
                var remote = applicationContext.CreateInstance<Remote>();

                var version = remote.GetAssemblyVersion("Microsoft.VisualStudio.ExtensionManager");

                Assert.That(version.Major, Is.EqualTo(installedVersion.Version.Major));
            }
        }

        [TestCaseSource(nameof(GetInstalledVersions))]
        public void ExtensionManagerImplementationAssembly_HasExpectedVersion(InstalledVersion installedVersion)
        {
            string applicationPath = installedVersion.ApplicationPath;
            var vsVersion = installedVersion.VsVersion;
            using (var applicationContext = new ApplicationContext(applicationPath, vsVersion))
            {
                var remote = applicationContext.CreateInstance<Remote>();

                var version = remote.GetAssemblyVersion("Microsoft.VisualStudio.ExtensionManager.Implementation");

                Assert.That(version.Major, Is.EqualTo(installedVersion.Version.Major));
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
