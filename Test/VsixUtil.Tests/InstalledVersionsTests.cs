using NUnit.Framework;

namespace VsixUtil.Tests
{
    public class InstalledVersionTests
    {
        public class TheToStringMethod
        {
            [Test]
            public void ToString_ContainsApplicationPath()
            {
                var appPath = @"x:\app\path";

                var text = new InstalledVersion(appPath, VsVersion.Vs2010, "Community").ToString();

                Assert.That(text, Does.Contain(appPath));
            }

            [Test]
            public void ToString_ContainsProduct()
            {
                var product = "Enterprise";

                var text = new InstalledVersion(@"x:\app\path", VsVersion.Vs2010, product).ToString();

                Assert.That(text, Does.Contain(product));
            }

            [Test]
            public void ToString_ContainsVsVersion()
            {
                var vsVersion = VsVersion.Vs2017;

                var text = new InstalledVersion(@"x:\app\path", vsVersion, "Community").ToString();

                Assert.That(text, Does.Contain(vsVersion.ToString()));
            }
        }
    }
}
