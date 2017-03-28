using System.IO;
using NUnit.Framework;

namespace VsixUtil.Tests
{
    public class ApplicationContextTests
    {
        [TestCase(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe", Version.Vs2015)]
        [TestCase(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe", Version.Vs2017)]
        [TestCase(@"C:\Program Files (x86)\Microsoft Visual Studio\Preview\TeamExplorer\Common7\IDE\devenv.exe", Version.Vs2017)]
        public void List(string applicationPath, Version version)
        {
            if(!File.Exists(applicationPath))
            {
                return; // Ignore versions that don't exist.
            }

            using (var applicationContext = new ApplicationContext(applicationPath, version))
            {
                var instance = applicationContext.CreateInstance<RemoteCommandRunner>();
                instance.Run(new ProxyConsoleContext(), applicationPath, version, "", ToolAction.List, null);
            }
        }
    }
}
