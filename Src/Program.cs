using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        internal static readonly CommandLine Help = new CommandLine(ToolAction.Help, null, null, "", "");

        internal readonly ToolAction ToolAction;
        internal readonly string RootSuffix;
        internal readonly string Version;
        internal readonly string[] SkuPreference;
        internal readonly string Arg;

        internal CommandLine(ToolAction toolAction, string version, string[] skuPreference, string rootSuffix, string arg)
        {
            ToolAction = toolAction;
            Version = version;
            SkuPreference = skuPreference;
            RootSuffix = rootSuffix;
            Arg = arg;
        }
    }

    internal enum Version
    {
        Vs2010,
        Vs2012,
        Vs2013,
        Vs2015,
        Vs2017
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
                case Version.Vs2017:
                    return "15";
                default:
                    throw new Exception("Bad Version");
            }
        }

        internal static string GetAssemblyVersionNumber(Version version)
        {
            return string.Format("{0}.0.0.0", GetVersionNumber(version));
        }

        internal static string GetApplicationPath(Version version, string[] skuPreference)
        {
            switch (version)
            {
                case Version.Vs2017:
                    return GetApplicationPathVs2017(skuPreference);
                default:
                    return GetApplicationPathFromRegistry(version);
            }
        }

        private static string GetApplicationPathFromRegistry(Version version)
        {
            var path = string.Format("SOFTWARE\\Microsoft\\VisualStudio\\{0}.0\\Setup\\VS", CommonUtil.GetVersionNumber(version));
            using (var registryKey = Registry.LocalMachine.OpenSubKey(path))
            {
                if (registryKey == null)
                {
                    return null;
                }

                return (string)(registryKey.GetValue("EnvironmentPath"));
            }
        }

        // NOTE: Assumes VS2017 has been installed at default location.
        // Returns first SKU that it finds (e.g. Community, Professional or Enterprise).
        private static string GetApplicationPathVs2017(string[] skuPreference)
        {
            foreach (var sku in skuPreference)
            {
                var path = string.Format(@"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\{0}\Common7\IDE\devenv.exe", sku);
                path = Environment.ExpandEnvironmentVariables(path);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        internal static bool IsVersionInstalled(Version version, string[] skuPreference)
        {
            try
            {
                return GetApplicationPath(version, skuPreference) != null;
            }
            catch
            {
                return false;
            }
        }

        internal static List<Version> GetInstalledVersions(string[] skuPreference)
        {
            return Enum
                .GetValues(typeof(Version))
                .Cast<Version>()
                .Where(x => IsVersionInstalled(x, skuPreference))
                .ToList();
        }

        internal static void PrintHelp()
        {
            Console.WriteLine("vsixutil [/install] extensionPath [/version number] [/sku name] [/rootSuffix name]");
            Console.WriteLine("vsixutil /uninstall identifier [/version number] [/sku name] [/rootSuffix name]");
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
        void Run(Version version, string[] skuPreference, string rootSuffix, ToolAction toolAction, string arg);
    }

    internal sealed class VersionManager : MarshalByRefObject, IVersionManager
    {
        public VersionManager()
        {
        }

        public void Run(Version version, string[] skuPreference, string rootSuffix, ToolAction toolAction, string arg1)
        {
            var appPath = CommonUtil.GetApplicationPath(version, skuPreference);
            var appDir = Path.GetDirectoryName(appPath);
            var probingPaths = ".;PrivateAssemblies;PublicAssemblies";
            using (new ProbingPathResolver(appDir, probingPaths.Split(';')))
            {
                PrivateRun(version, skuPreference, rootSuffix, toolAction, arg1);
            }
        }

        private static void PrivateRun(Version version, string[] skuPreference, string rootSuffix, ToolAction toolAction, string arg1)
        {
            var obj = CreateExtensionManager(version, skuPreference, rootSuffix);
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
                case Version.Vs2017:
                    suffix = ".15.0";
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

        internal static object CreateExtensionManager(Version version, string[] skuPreference, string rootSuffix)
        {
            var settingsAssembly = LoadSettingsAssembly(version);
            var applicationPath = CommonUtil.GetApplicationPath(version, skuPreference);

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

    internal class ProbingPathResolver : IDisposable
    {
        private readonly string Dir;
        private readonly string[] ProbePaths;

        internal ProbingPathResolver(string dir, string[] probePaths)
        {
            Dir = dir;
            ProbePaths = probePaths;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            foreach (var probePath in ProbePaths)
            {
                var path = Path.Combine(Dir, probePath, assemblyName.Name + ".dll");
                if (File.Exists(path))
                {
                    return Assembly.LoadFrom(path);
                }
            }

            return null;
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
            var version = "";
            var sku = "Community;Professional;Enterprise";
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
                    case "/v":
                    case "/version":
                        if (index + 1 >= args.Length)
                        {
                            Console.Write("/version requires an argument");
                            return CommandLine.Help;
                        }

                        version = args[index + 1];
                        index += 2;
                        break;
                    case "/s":
                    case "/sku":
                        if (index + 1 >= args.Length)
                        {
                            Console.Write("/sku requires an argument");
                            return CommandLine.Help;
                        }

                        sku = args[index + 1];
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
                        arg = args[index];
                        if (!File.Exists(arg))
                        {
                            Console.WriteLine("{0} is not a valid argument", arg);
                            return CommandLine.Help;
                        }

                        // Default to Install if `arg` file exists.
                        toolAction = ToolAction.Install;
                        index += 1;
                        break;
                }
            }

            var skuPreference = sku.Split(';');
            return new CommandLine(toolAction, version, skuPreference, rootSuffix, arg);
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
                var versionManager = (IVersionManager)appDomain.CreateInstanceFromAndUnwrap(
                    typeof(Program).Assembly.Location,
                    typeof(VersionManager).FullName);
                versionManager.Run(version, commandLine.SkuPreference, commandLine.RootSuffix, commandLine.ToolAction, commandLine.Arg);
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

            foreach (var version in CommonUtil.GetInstalledVersions(commandLine.SkuPreference))
            {
                if(IncludeVersion(commandLine.Version, version))
                {
                    Run(version, commandLine);
                }
            }
        }

        private static bool IncludeVersion(string number, Version version)
        {
            if(string.IsNullOrEmpty(number))
            {
                return true;
            }

            var versionNumber = CommonUtil.GetVersionNumber(version);
            return number == versionNumber;
        }
    }
}
