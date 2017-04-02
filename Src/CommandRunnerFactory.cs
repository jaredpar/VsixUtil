using System;

namespace VsixUtil
{
    public class CommandRunnerFactory : MarshalByRefObject
    {
        public CommandRunner Create(IConsoleContext consoleContext, InstalledVersion installedVersion, string rootSuffix)
        {
            var extensionManager = ExtensionManagerFactory.CreateExtensionManager(installedVersion, rootSuffix);
            return new CommandRunner(consoleContext, extensionManager, installedVersion, rootSuffix);
        }
    }
}
