using System;
using System.IO;
using NUnit.Framework;

namespace VsixUtil.Tests
{
    public class ExtensionManagerFactoryTests
    {
        [TestCase(@"%ProgramFiles(x86)%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe", Version.Vs2010)]
        [TestCase(@"%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe", Version.Vs2012)]
        [TestCase(@"%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe", Version.Vs2013)]
        [TestCase(@"%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe", Version.Vs2015)]
        [TestCase(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe", Version.Vs2017)]
        [TestCase(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.exe", Version.Vs2017)]
        [TestCase(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe", Version.Vs2017)]
        [TestCase(@"%ProgramFiles(x86)%\Microsoft Visual Studio\Preview\TeamExplorer\Common7\IDE\devenv.exe", Version.Vs2017)]
        public void WasCreated(string applicationPath, Version version)
        {
            applicationPath = Environment.ExpandEnvironmentVariables(applicationPath);
            if(!File.Exists(applicationPath))
            {
                return;
            }

            using (var applicationContext = new ApplicationContext(applicationPath, version))
            {
                var remote = applicationContext.CreateInstance<Remote>();

                var exists = remote.WasCreated(applicationPath, version);

                Assert.That(exists, Is.True);
            }
        }

        class Remote : MarshalByRefObject
        {
            public bool WasCreated(string applicationPath, Version version)
            {
                var extensionManager = ExtensionManagerFactory.CreateExtensionManager(applicationPath, version, "");
                return extensionManager != null;
            }
        }

    }
}
