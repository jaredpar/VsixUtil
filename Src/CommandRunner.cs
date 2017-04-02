using Microsoft.VisualStudio.ExtensionManager;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VsixUtil
{
    public sealed class CommandRunner : MarshalByRefObject
    {
        internal readonly IVsExtensionManager _extensionManager;
        internal string _appPath;
        internal VsVersion _vsVersion;
        internal string _rootSuffix;

        internal CommandRunner(IConsoleContext consoleContext, IVsExtensionManager extensionManager,
            string appPath, VsVersion vsVersion, string rootSuffix)
        {
            Console = consoleContext;
            _extensionManager = extensionManager;
            _appPath = appPath;
            _vsVersion = vsVersion;
            _rootSuffix = rootSuffix;
        }

        IConsoleContext Console
        {
            get;
        }

        public void Run(ToolAction toolAction, string arg)
        {
            switch (toolAction)
            {
                case ToolAction.Install:
                    RunInstall(arg);
                    break;
                case ToolAction.Uninstall:
                    RunUninstall(arg);
                    break;
                case ToolAction.List:
                    RunList(arg);
                    break;
                case ToolAction.Help:
                    Program.PrintHelp(Console);
                    break;
                default:
                    throw new Exception(string.Format("Not implemented {0}", toolAction));
            }
        }

        private void RunInstall(string extensionPath)
        {
            try
            {
                Console.Write("{0} Install ... ", _vsVersion);
                var installableExtension = _extensionManager.CreateInstallableExtension(extensionPath);
                var identifier = installableExtension.Header.Identifier;
                UninstallSilent(identifier);

                var perMachine = false;
                var header = installableExtension.Header;
                if (header.AllUsers != perMachine)
                {
                    if (SetAllUsers(header, perMachine))
                    {
                        Console.Write(string.Format("NOTE: Changing `AllUsers` to {0} ... ", perMachine));
                    }
                    else
                    {
                        Console.Write(string.Format("WARNING: Couldn't change `AllUsers` to {0} ... ", perMachine));
                    }
                }

                _extensionManager.Install(installableExtension, perMachine);

                var installedExtension = _extensionManager.GetInstalledExtension(identifier);
                _extensionManager.Enable(installedExtension);
                Console.WriteLine("Succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }
        }

        private static bool SetAllUsers(IExtensionHeader header, bool allUsers)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var allUsersProperty = header.GetType().GetProperty("AllUsers", flags);
            if (allUsersProperty == null || !allUsersProperty.CanWrite)
            {
                return false;
            }

            allUsersProperty.SetValue(header, allUsers, null);
            return true;
        }

        private void RunUninstall(string identifier)
        {
            try
            {
                Console.Write("{0} Uninstall ... ", _vsVersion);
                var installedExtension = _extensionManager.GetInstalledExtension(identifier);
                if (installedExtension != null)
                {
                    _extensionManager.Uninstall(installedExtension);
                }

                Console.WriteLine("Succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }
        }

        private void UninstallSilent(string identifier, bool printError = false)
        {
            try
            {
                var installedExtension = _extensionManager.GetInstalledExtension(identifier);
                if (installedExtension != null)
                {
                    _extensionManager.Uninstall(installedExtension);
                }
            }
            catch
            {

            }
        }

        private void RunList(string filter)
        {
            Console.WriteLine();
            Console.WriteLine($"{_appPath} ({_vsVersion})");
            Console.WriteLine("  {0, -40} - {1}", "Name", "Identifier");

            var regex = filter != null
                ? new Regex(filter, RegexOptions.IgnoreCase)
                : null;

            foreach (var extension in _extensionManager.GetInstalledExtensions())
            {
                var header = extension.Header;
                if (regex == null || regex.IsMatch(header.Name))
                {
                    var formattedName = header.Name.Replace(Environment.NewLine, " ");
                    if (formattedName.Length > 35)
                    {
                        formattedName = formattedName.Substring(0, 35) + " ...";
                    }
                    Console.WriteLine("  {0,-40} - {1}", formattedName, header.Identifier);
                }
            }
        }
    }
}
