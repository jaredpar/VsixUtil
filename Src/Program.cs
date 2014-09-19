using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace VsixUtil
{
    internal enum ToolAction
    {
        Install,
        List,
        Help
    }

    internal struct CommandLine
    {
        internal static readonly CommandLine Help = new CommandLine(ToolAction.Help, "", "");

        internal readonly ToolAction ToolAction;
        internal readonly string RootSuffix;
        internal readonly string Arg;

        internal CommandLine(ToolAction toolAction, string rootSuffix, string arg)
        {
            ToolAction = toolAction;
            RootSuffix = rootSuffix;
            Arg = arg;
        }
    }

    internal enum Version
    {
        Vs2010,
        Vs2012,
        Vs2013,
        VsDev14
    }

    internal static class CommonUtil
    {
        internal static string GetVersionNumber(Version version)
        {
            switch (version)
            {
                case Version.Vs2010:
                    return "10";
                case Version.Vs2012:
                    return "11";
                case Version.Vs2013:
                    return "12";
                case Version.VsDev14:
                    return "14";
                default:
                    throw new Exception("Bad Version");
            }
        }

        internal static string GetAssemblyVersionNumber(Version version)
        {
            return string.Format("{0}.0.0.0", GetVersionNumber(version));
        }

        internal static string GetApplicationPath(Version version)
        {
            var path = string.Format("SOFTWARE\\Microsoft\\VisualStudio\\{0}.0\\Setup\\VS", CommonUtil.GetVersionNumber(version));
            using (var registryKey = Registry.LocalMachine.OpenSubKey(path))
            {
                return (string)(registryKey.GetValue("EnvironmentPath"));
            }
        }

        internal static bool IsVersionInstalled(Version version)
        {
            try
            {
                return GetApplicationPath(version) != null;
            }
            catch
            {
                return false;
            }
        }

        internal static List<Version> GetInstalledVersions()
        {
            return Enum
                .GetValues(typeof(Version))
                .Cast<Version>()
                .Where(x => IsVersionInstalled(x))
                .ToList();
        }

        internal static void PrintHelp()
        {
            Console.WriteLine("vsixutil [/install(+|-)] extensionPath [rootSuffix]");
            Console.WriteLine("vsixutil /list");
        }
    }

    internal sealed class CommandRunner
    {
        internal readonly Version _version;
        internal readonly string _rootSuffix;
        internal readonly IVsExtensionManager _extensionManager;

        internal CommandRunner(Version version, string rootSuffix, IVsExtensionManager extensionManager)
        {
            _version = version;
            _rootSuffix = rootSuffix;
            _extensionManager = extensionManager;
        }

        internal void Run(ToolAction toolAction, string arg)
        {
            switch (toolAction)
            {
                case ToolAction.Install:
                    RunInstall(arg);
                    break;
                case ToolAction.List:
                    RunList(arg);
                    break;
                case ToolAction.Help:
                    RunHelp();
                    break;
                default:
                    throw new Exception(string.Format("Not implemented {0}", toolAction));
            }
        }

        private void RunInstall(string extensionPath)
        {
            try
            {
                Console.Write("{0} Install ... ", _version);
                var installableExtension = _extensionManager.CreateInstallableExtension(extensionPath);
                var identifier = installableExtension.Header.Identifier;
                TryUninstall(identifier, printError: false);
                _extensionManager.Install(installableExtension, perMachine: false);

                var installedExtension = _extensionManager.GetInstalledExtension(identifier);
                _extensionManager.Enable(installedExtension);
                Console.WriteLine("Succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }
        }

        private bool TryUninstall(string identifier, bool printError = false)
        {
            try
            {
                var installedExtension = _extensionManager.GetInstalledExtension(identifier);
                if (installedExtension == null)
                {
                    return false;
                }

                _extensionManager.Uninstall(installedExtension);
                return true;
            }
            catch (Exception ex)
            {
                if (printError)
                {
                    Console.WriteLine(ex.Message);
                }

                return false;
            }
        }

        private void RunList(string filter)
        {
            Console.WriteLine(_version);
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

        private static void RunHelp()
        {
            Console.WriteLine("vsixutil [/rootSuffix name] /install extensionPath");
            Console.WriteLine("vsixutil [/rootSuffix name] /list [filter]");
        }
    }

    internal interface IVersionManager
    {
        void Run(Version version, string rootSuffix, ToolAction toolAction, string arg);
    }

    internal sealed class VersionManager : MarshalByRefObject, IVersionManager
    {
        #region AssemblyRedirectory

        internal sealed class AssemblyRedirector
        {
            internal ResolveEventHandler _resolveEventHandler;
            internal Version _version;

            internal Version Version
            {
                get { return _version; }
                set { _version = value; }
            }

            internal AssemblyRedirector()
            {
                _version = Version.Vs2010;
                _resolveEventHandler = ResolveAssembly;
                AppDomain.CurrentDomain.AssemblyResolve += _resolveEventHandler;
            }

            private Assembly ResolveAssembly(object sender, ResolveEventArgs e)
            {
                try
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= _resolveEventHandler;

                    if (e.Name.StartsWith("Microsoft.VisualStudio.ExtensionManager,", StringComparison.OrdinalIgnoreCase))
                    {
                        string qualifiedName = string.Format(
                            "Microsoft.VisualStudio.ExtensionManager, Version={0}, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                            CommonUtil.GetAssemblyVersionNumber(_version));
                        return Assembly.Load(qualifiedName);
                    }

                    return null;

                }
                finally
                {
                    AppDomain.CurrentDomain.AssemblyResolve += _resolveEventHandler;
                }
            }
        }

        #endregion

        private readonly AssemblyRedirector _assemblyRedirector;

        public VersionManager()
        {
            _assemblyRedirector = new AssemblyRedirector();
        }

        public void Run(Version version, string rootSuffix, ToolAction toolAction, string arg1)
        {
            _assemblyRedirector.Version = version;
            RunCore(version, rootSuffix, toolAction, arg1);
        }

        private void RunCore(Version version, string rootSuffix, ToolAction toolAction, string arg1)
        {
            var obj = CreateExtensionManager(version, "");
            var commandRunner = new CommandRunner(version, rootSuffix, (IVsExtensionManager)obj);
            commandRunner.Run(toolAction, arg1);
        }

        private static Assembly LoadImplementationAssembly(Version version)
        {
            var format = "Microsoft.VisualStudio.ExtensionManager.Implementation, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            var strongName = string.Format(format, CommonUtil.GetVersionNumber(version));
            return Assembly.Load(strongName);
        }

        private static Assembly LoadSettingsAssembly(Version version)
        {
            var format = "Microsoft.VisualStudio.Settings{0}, Version={1}.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            string suffix = "";
            switch (version)
            {
                case Version.Vs2010:
                    suffix = "";
                    break;
                case Version.Vs2012:
                    suffix = ".11.0";
                    break;
                case Version.Vs2013:
                    suffix = ".12.0";
                    break;
                case Version.VsDev14:
                    suffix = ".14.0";
                    break;
                default:
                    throw new Exception("Bad Version");
            }

            var strongName = string.Format(format, suffix, CommonUtil.GetVersionNumber(version));
            return Assembly.Load(strongName);
        }

        private static Type GetExtensionManagerServiceType(Version version)
        {
            var assembly = LoadImplementationAssembly(version);
            return assembly.GetType("Microsoft.VisualStudio.ExtensionManager.ExtensionManagerService");
        }

        internal static object CreateExtensionManager(Version version, string rootSuffix)
        {
            var settingsAssembly = LoadSettingsAssembly(version);
            var applicationPath = CommonUtil.GetApplicationPath(version);

            var externalSettingsManagerType = settingsAssembly.GetType("Microsoft.VisualStudio.Settings.ExternalSettingsManager");
            var settingsManager = externalSettingsManagerType
                .GetMethods()
                .Where(x => x.Name == "CreateForApplication")
                .Where(x =>
                    {
                        var parameters = x.GetParameters();
                        return
                            parameters.Length == 2 &&
                            parameters[0].ParameterType == typeof(string) &&
                            parameters[1].ParameterType == typeof(string);
                    })
                .FirstOrDefault()
                .Invoke(null, new[] { applicationPath, rootSuffix });

            var extensionManagerServiceType = GetExtensionManagerServiceType(version);
            var obj = extensionManagerServiceType
                .GetConstructors()
                .Where(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType.Name.Contains("SettingsManager"))
                .FirstOrDefault()
                .Invoke(new[] { settingsManager });
            return obj;
        }
    }

    internal class Program
    {
        private static CommandLine ParseCommandLine(string[] args)
        {
            if (args.Length == 0)
            {
                return CommandLine.Help;
            }

            var toolAction = ToolAction.Help;
            var rootSuffix = "";
            var arg = "";

            int index = 0;
            while (index < args.Length)
            {
                switch (args[index].ToLower())
                {
                    case "/install":
                        if (index + 1 >= args.Length)
                        {
                            Console.Write("/install requires an argument");
                            return CommandLine.Help;
                        }

                        toolAction = ToolAction.Install;
                        arg = args[index + 1];
                        index += 2;
                        break;
                    case "/rootSuffix":
                        if (index + 1 >= args.Length)
                        {
                            Console.Write("/rootSuffix requires an argument");
                            return CommandLine.Help;
                        }

                        rootSuffix = args[index + 1];
                        index += 2;
                        break;
                    case "/list":
                        toolAction = ToolAction.List;
                        if (index + 1 < args.Length)
                        {
                            arg = args[index + 1];
                        }

                        index = args.Length;
                        break;
                    case "/help":
                        toolAction = ToolAction.Help;
                        index = args.Length;
                        break;
                    default:
                        Console.WriteLine("{0} is not a valid argument", args[0]);
                        return CommandLine.Help;
                }
            }

            return new CommandLine(toolAction, rootSuffix, arg);
        }

        private static void Run(Version version, CommandLine commandLine)
        {
            var appDomain = AppDomain.CreateDomain(version.ToString());
            try
            {
                var versionManager = (IVersionManager)appDomain.CreateInstanceAndUnwrap(
                    typeof(Program).Assembly.FullName,
                    typeof(VersionManager).FullName);
                versionManager.Run(version, commandLine.RootSuffix, commandLine.ToolAction, commandLine.Arg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }

        internal static void Main(string[] args)
        {
            var commandLine = ParseCommandLine(args);
            if (commandLine.ToolAction == ToolAction.Help)
            {
                CommonUtil.PrintHelp();
                Environment.Exit(1);
                return;
            }

            foreach (var version in CommonUtil.GetInstalledVersions())
            {
                Run(version, commandLine);
            }
        }
    }
}
