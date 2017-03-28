using System;
using Microsoft.VisualStudio.ExtensionManager;

namespace VsixUtil
{
    public class RemoteCommandRunner : MarshalByRefObject
    {
        public void Run(IConsoleContext consoleContext, string appPath, Version version,
            string rootSuffix, ToolAction toolAction, string arg1)
        {
            var extensionManager = ExtensionManagerFactory.CreateExtensionManager(appPath, version, rootSuffix);
            var commandRunner = new CommandRunner(consoleContext, extensionManager, appPath, version, rootSuffix);
            commandRunner.Run(toolAction, arg1);
        }
    }
}
