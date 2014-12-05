using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace VsixUtil
{
    internal enum ToolAction
    {
        Install,
        Uninstall,
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
        Vs2015
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
                case Version.Vs2015:
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
            Console.WriteLine("vsixutil [/rootSuffix name] /install extensionPath");
            Console.WriteLine("vsixutil [/rootSuffix name] /uninstall identifier");
            Console.WriteLine("vsixutil /list [filter]");
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
                case ToolAction.Uninstall:
                    RunUninstall(arg);
                    break;
                case ToolAction.List:
                    RunList(arg);
                    break;
                case ToolAction.Help:
                    CommonUtil.PrintHelp();
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
                UninstallSilent(identifier);
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

        private void RunUninstall(string identifier)
        {
            try
            {
                Console.Write("{0} Uninstall ... ", _version);
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
    }

    internal interface IVersionManager
    {
        void Run(Version version, string rootSuffix, ToolAction toolAction, string arg);
    }

    internal sealed class VersionManager : MarshalByRefObject, IVersionManager
    {
        public VersionManager()
        {

        }

        public void Run(Version version, string rootSuffix, ToolAction toolAction, string arg1)
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
                case Version.Vs2015:
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
                    case "/i":
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
                    case "/u":
                    case "/uninstall":
                        if (index + 1 >= args.Length)
                        {
                            Console.Write("/uninstall requires an argument");
                            return CommandLine.Help;
                        }

                        toolAction = ToolAction.Uninstall;
                        arg = args[index + 1];
                        index += 2;
                        break;
                    case "/r":
                    case "/rootsuffix":
                        if (index + 1 >= args.Length)
                        {
                            Console.Write("/rootSuffix requires an argument");
                            return CommandLine.Help;
                        }

                        rootSuffix = args[index + 1];
                        index += 2;
                        break;
                    case "/l":
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
                        Console.WriteLine("{0} is not a valid argument", args[index]);
                        return CommandLine.Help;
                }
            }

            return new CommandLine(toolAction, rootSuffix, arg);
        }

        private static string GenerateConfigFileContents(Version version)
        {
            Debug.Assert(version != Version.Vs2010);
            const string contentFormat = @"
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""Microsoft.VisualStudio.ExtensionManager"" publicKeyToken=""b03f5f7f11d50a3a"" culture=""neutral"" />
        <bindingRedirect oldVersion=""10.0.0.0-{0}"" newVersion=""{0}"" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            return string.Format(contentFormat, CommonUtil.GetAssemblyVersionNumber(version));
        }

        private static void Run(Version version, CommandLine commandLine)
        {
            AppDomainSetup appDomainSetup = null;
            if (version != Version.Vs2010)
            {
                var configFile = Path.GetTempFileName();
                File.WriteAllText(configFile, GenerateConfigFileContents(version));
                appDomainSetup = new AppDomainSetup();
                appDomainSetup.ConfigurationFile = configFile;
            }
            var appDomain = AppDomain.CreateDomain(version.ToString(), securityInfo: null, info: appDomainSetup);
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
