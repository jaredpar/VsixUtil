using System;
using System.IO;

namespace VsixUtil
{
    public class Program
    {
        public static CommandLine ParseCommandLine(params string[] args)
        {
            if (args.Length == 0)
            {
                return CommandLine.Help;
            }

            var toolAction = ToolAction.Help;
            var version = "";
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
                        if(!arg.StartsWith("/") && args.Length == 1)
                        {
                            // Default to Install if there's a single argument.
                            toolAction = ToolAction.Install;
                            index += 1;
                            break;
                        }

                        Console.WriteLine("{0} is not a valid argument", arg);
                        return CommandLine.Help;
                }
            }

            return new CommandLine(toolAction, version, rootSuffix, arg);
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

            var installedVersions = InstalledVersionUtilities.GetInstalledVersions(commandLine.Version);
            foreach(var installedVersion in installedVersions)
            {
                var appPath = installedVersion.ApplicationPath;
                var vsVersion = installedVersion.VsVersion;
                using (var applicationContext = new ApplicationContext(appPath, vsVersion))
                {
                    var versionManager = applicationContext.CreateInstance<RemoteCommandRunner>();
                    versionManager.Run(new ProxyConsoleContext(), appPath, vsVersion,
                        commandLine.RootSuffix, commandLine.ToolAction, commandLine.Arg);
                }
            }
        }
    }
}
