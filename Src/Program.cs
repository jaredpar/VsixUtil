using System;
using System.Linq;

namespace VsixUtil
{
    public static class Program
    {
        public static CommandLine ParseCommandLine(IConsoleContext consoleContext, params string[] args)
        {
            if (args.Length == 0)
            {
                return CommandLine.Help;
            }

            var toolAction = ToolAction.Help;
            VsVersion? version = null;
            string product = null;
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
                            consoleContext.Write("/install requires an argument");
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
                            consoleContext.Write("/uninstall requires an argument");
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
                            consoleContext.Write("/version requires an argument");
                            return CommandLine.Help;
                        }

                        version = VsVersionUtil.ToVsVersion(args[index + 1]);
                        index += 2;
                        break;
                    case "/p":
                    case "/product":
                        if (index + 1 >= args.Length)
                        {
                            consoleContext.Write("/product requires an argument");
                            return CommandLine.Help;
                        }

                        product = args[index + 1];
                        index += 2;
                        break;
                    case "/r":
                    case "/rootsuffix":
                        if (index + 1 >= args.Length)
                        {
                            consoleContext.Write("/rootSuffix requires an argument");
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
                        if(!arg.StartsWith("/") && args.Length == 1)
                        {
                            // Default to Install if there's a single argument.
                            toolAction = ToolAction.Install;
                            index += 1;
                            break;
                        }

                        consoleContext.WriteLine("{0} is not a valid argument", arg);
                        return CommandLine.Help;
                }
            }

            return new CommandLine(toolAction, version, product, rootSuffix, arg);
        }

        internal static void Main(string[] args)
        {
            var consoleContext = new ConsoleContext(Console.Out);
            var commandLine = ParseCommandLine(consoleContext, args);
            if (commandLine.ToolAction == ToolAction.Help)
            {
                PrintHelp(consoleContext);
                Environment.Exit(1);
                return;
            }

            var installedVersions = InstalledVersionUtilities.GetInstalledVersions().Where(iv => Filter(iv, commandLine));
            foreach (var installedVersion in installedVersions)
            {
                using (var applicationContext = new ApplicationContext(installedVersion))
                {
                    var factory = applicationContext.CreateInstance<CommandRunnerFactory>();
                    var commandRunner = factory.Create(consoleContext, installedVersion, commandLine.RootSuffix);
                    commandRunner.Run(commandLine.ToolAction, commandLine.Arg);
                }
            }
        }

        public static bool Filter(InstalledVersion installedVersion, CommandLine commandLine)
        {
            return FilterVsVersion(installedVersion, commandLine.VsVersion) && FilterProduct(installedVersion, commandLine.Product);
        }

        static bool FilterVsVersion(InstalledVersion installedVersion, VsVersion? vsVersion)
        {
            return vsVersion?.Equals(installedVersion.VsVersion) ?? true;
        }

        static bool FilterProduct(InstalledVersion installedVersion, string product)
        {
            return product == null || (installedVersion.Product?.StartsWith(product, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        internal static void PrintHelp(IConsoleContext consoleContext)
        {
            consoleContext.WriteLine("vsixutil [/install] extensionPath [/version number] [/rootSuffix name]");
            consoleContext.WriteLine("vsixutil /uninstall identifier [/version number] [/rootSuffix name]");
            consoleContext.WriteLine("vsixutil /list [filter]");
        }
    }
}
