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
                var commandLine = Program.ParseCommandLine();

                Assert.That(commandLine.ToolAction, Is.EqualTo(ToolAction.Help));
            }

            [Test]
            public void SingleArg_Install()
            {
                var vsixFile = "foo.vsix";

                var commandLine = Program.ParseCommandLine(vsixFile);

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
                var commandLine = Program.ParseCommandLine(option, arg);

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
                var commandLine = Program.ParseCommandLine(option);

                Assert.That(commandLine.ToolAction, Is.EqualTo(toolAction));
            }

            [TestCase("/v", "10")]
            [TestCase("/version", "10")]
            public void Version(string option, string version)
            {
                var commandLine = Program.ParseCommandLine(option, version);

                Assert.That(commandLine.Version, Is.EqualTo(version));
            }

            [TestCase("/s", "Community")]
            [TestCase("/sku", "Community")]
            [TestCase("/sku", "Enterprise;Professional")]
            public void Sku(string option, string sku)
            {
                var skus = sku.Split(';');

                var commandLine = Program.ParseCommandLine(option, sku);

                Assert.That(commandLine.Skus, Is.EqualTo(skus));
            }

            [Test]
            public void DefaultSkus()
            {
                var skus = "Community;Professional;Enterprise".Split(';');

                var commandLine = Program.ParseCommandLine("/install", "foo.vsix");

                Assert.That(commandLine.Skus, Is.EqualTo(skus));
            }

            [TestCase("/r", "Exp")]
            [TestCase("/rootsuffix", "Exp")]
            public void RootSuffix(string option, string rootsuffix)
            {
                var commandLine = Program.ParseCommandLine(option, rootsuffix);

                Assert.That(commandLine.RootSuffix, Is.EqualTo(rootsuffix));
            }
        }

    }
}
