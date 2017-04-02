using System;

namespace VsixUtil
{
    public class CommandRunnerFactory : MarshalByRefObject
    {
        public CommandRunner Create(IConsoleContext consoleContext, string appPath, VsVersion version, string rootSuffix)
        {
            var extensionManager = ExtensionManagerFactory.CreateExtensionManager(appPath, version, rootSuffix);
            return new CommandRunner(consoleContext, extensionManager, appPath, version, rootSuffix);
        }
    }
}
