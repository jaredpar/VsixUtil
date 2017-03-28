using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsixUtil.Tests
{
    public class ApplicationContextTests
    {
        [TestCase(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe", Version.Vs2015)]
        public void List(string applicationPath, Version version)
        {
            using (var applicationContext = new ApplicationContext(applicationPath, version))
            {
                var instance = applicationContext.CreateInstance<VersionManager>();
                instance.Run(applicationPath, version, "", ToolAction.List, null);
            }
        }
    }
}
