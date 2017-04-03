using System;
using NUnit.Framework;

namespace VsixUtil.Tests
{
    public class ProgramTests
    {
        public class ParseCommandLine
        {
            [Test]
            public void NoArg_Help()
            {
                var consoleContext = new TestConsoleContext();

                var commandLine = Program.ParseCommandLine(consoleContext);

                Assert.That(commandLine.ToolAction, Is.EqualTo(ToolAction.Help));
            }

            [Test]
            public void SingleArg_Install()
            {
                var consoleContext = new TestConsoleContext();
                var vsixFile = "foo.vsix";

                var commandLine = Program.ParseCommandLine(consoleContext, vsixFile);

                Assert.That(commandLine.ToolAction, Is.EqualTo(ToolAction.Install));
                Assert.That(commandLine.Arg, Is.EqualTo(vsixFile));
            }

            [TestCase("/i", "foo.vsix", ToolAction.Install)]
            [TestCase("/install", "foo.vsix", ToolAction.Install)]
            [TestCase("/u", "myid", ToolAction.Uninstall)]
            [TestCase("/uninstall", "myid", ToolAction.Uninstall)]
            [TestCase("/l", "search", ToolAction.List)]
            [TestCase("/list", "search", ToolAction.List)]
            public void ToolActionWithArg(string option, string arg, ToolAction toolAction)
            {
                var consoleContext = new TestConsoleContext();

                var commandLine = Program.ParseCommandLine(consoleContext, option, arg);

                Assert.That(commandLine.ToolAction, Is.EqualTo(toolAction));
                Assert.That(commandLine.Arg, Is.EqualTo(arg));
            }

            [TestCase("/help", ToolAction.Help)]
            [TestCase("/install", ToolAction.Help)]
            [TestCase("/uninstall", ToolAction.Help)]
            [TestCase("/list", ToolAction.List)]
            [TestCase("/version", ToolAction.Help)]
            public void ToolActionNoArg(string option, ToolAction toolAction)
            {
                var consoleContext = new TestConsoleContext();

                var commandLine = Program.ParseCommandLine(consoleContext, option);

                Assert.That(commandLine.ToolAction, Is.EqualTo(toolAction));
            }

            [TestCase("/version", "10", VsVersion.Vs2010)]
            [TestCase("/version", "2010", VsVersion.Vs2010)]
            [TestCase("/version", "11", VsVersion.Vs2012)]
            [TestCase("/version", "2012", VsVersion.Vs2012)]
            [TestCase("/version", "12", VsVersion.Vs2013)]
            [TestCase("/version", "2013", VsVersion.Vs2013)]
            [TestCase("/version", "14", VsVersion.Vs2015)]
            [TestCase("/version", "2015", VsVersion.Vs2015)]
            [TestCase("/version", "15", VsVersion.Vs2017)]
            [TestCase("/version", "2017", VsVersion.Vs2017)]
            [TestCase("/v", "10", VsVersion.Vs2010)]
            public void Version(string option, string version, VsVersion vsVersion)
            {
                var consoleContext = new TestConsoleContext();

                var commandLine = Program.ParseCommandLine(consoleContext, option, version);

                Assert.That(commandLine.VsVersion, Is.EqualTo(vsVersion));
            }

            [TestCase("/version", "9", "Bad Version")]
            [TestCase("/version", "foo", "Bad Version")]
            public void Version_ExpectException(string option, string version, string exceptionMessage)
            {
                var consoleContext = new TestConsoleContext();

                var exception = Assert.Throws<Exception>(() => Program.ParseCommandLine(consoleContext, option, version));

                Assert.That(exception.Message, Is.EqualTo(exceptionMessage));
            }

            [TestCase("/r", "Exp")]
            [TestCase("/rootsuffix", "Exp")]
            public void RootSuffix(string option, string rootsuffix)
            {
                var consoleContext = new TestConsoleContext();

                var commandLine = Program.ParseCommandLine(consoleContext, option, rootsuffix);

                Assert.That(commandLine.RootSuffix, Is.EqualTo(rootsuffix));
            }

            public class TheFilterMethod
            {
                [TestCase(VsVersion.Vs2010, VsVersion.Vs2010, true)]
                [TestCase(VsVersion.Vs2010, VsVersion.Vs2012, false)]
                [TestCase(VsVersion.Vs2010, null, true)]
                public void FilterVsVersion(VsVersion vsVersion, VsVersion filterVsVersion, bool expect)
                {
                    var installedVersion = new InstalledVersion(null, vsVersion, "Community");
                    var commandLine = new CommandLine(vsVersion: filterVsVersion);

                    var include = Program.Filter(installedVersion, commandLine);

                    Assert.That(include, Is.EqualTo(expect));
                }

                [TestCase("Community", null, true)]
                [TestCase("Community", "Community", true)]
                [TestCase("Community", "community", true)]
                [TestCase("Community", "com", true)]
                [TestCase("Community", "Enterprise", false)]
                public void FilterProduct(string product, string filterProduct, bool expect)
                {
                    var installedVersion = new InstalledVersion(null, VsVersion.Vs2010, product);
                    var commandLine = new CommandLine(product: filterProduct);

                    var include = Program.Filter(installedVersion, commandLine);

                    Assert.That(include, Is.EqualTo(expect));
                }
            }
        }

    }
}
