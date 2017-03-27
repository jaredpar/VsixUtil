using Microsoft.VisualStudio.ExtensionManager;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VsixUtil
{
    internal sealed class VersionManager : MarshalByRefObject, IVersionManager
    {
        public VersionManager()
        {
        }

        public void Run(string appPath, Version version, string rootSuffix, ToolAction toolAction, string arg1)
        {
            var appDir = Path.GetDirectoryName(appPath);
            var probingPaths = ".;PrivateAssemblies;PublicAssemblies";
            using (new ProbingPathResolver(appDir, probingPaths.Split(';')))
            {
                PrivateRun(appPath, version, rootSuffix, toolAction, arg1);
            }
        }

        private static void PrivateRun(string appPath, Version version, string rootSuffix, ToolAction toolAction, string arg1)
        {
            var obj = CreateExtensionManager(appPath, version, rootSuffix);
            var commandRunner = new CommandRunner(appPath, version, rootSuffix, (IVsExtensionManager)obj);
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

        internal static object CreateExtensionManager(string applicationPath, Version version, string rootSuffix)
        {
            var settingsAssembly = LoadSettingsAssembly(version);

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
}
