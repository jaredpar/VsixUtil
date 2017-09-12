using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace VsixUtil
{
    public class ApplicationContext : IDisposable
    {
        AppDomain appDomain;
        bool createDomain;

        public ApplicationContext(InstalledVersion installedVersion, bool createDomain)
        {
            this.createDomain = createDomain;

            if (createDomain)
            {
                var appDomainSetup = new AppDomainSetup();
                if (installedVersion.VsVersion != VsVersion.Vs2010)
                {
                    var configFile = Path.GetTempFileName();
                    File.WriteAllText(configFile, GenerateConfigFileContents(installedVersion.VsVersion));
                    appDomainSetup.ConfigurationFile = configFile;
                }

                appDomainSetup.ApplicationBase = Path.GetDirectoryName(GetType().Assembly.Location);
                appDomain = AppDomain.CreateDomain(installedVersion.ToString(), securityInfo: null, info: appDomainSetup);
            }
            else
            {
                appDomain = AppDomain.CurrentDomain;
            }

            InitProbingPathResolver(appDomain, installedVersion.ApplicationPath);
        }

        public T CreateInstance<T>()
        {
            return (T)appDomain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName);
        }

        public void Dispose()
        {
            if (createDomain)
            {
                AppDomain.Unload(appDomain);
            }
        }

        static void InitProbingPathResolver(AppDomain appDomain, string appPath)
        {
            var resolverType = typeof(ProbingPathResolver);
            var resolver = (ProbingPathResolver)appDomain.CreateInstanceFromAndUnwrap(
                resolverType.Assembly.Location, resolverType.FullName);
            var appDir = Path.GetDirectoryName(appPath);
            var probingPaths = ".;PrivateAssemblies;PublicAssemblies".Split(';');
            resolver.Init(appDir, probingPaths);
        }

        class ProbingPathResolver : MarshalByRefObject
        {
            private string Dir;
            private string[] ProbePaths;

            internal void Init(string dir, string[] probePaths)
            {
                Dir = dir;
                ProbePaths = probePaths;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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

        private static string GenerateConfigFileContents(VsVersion version)
        {
            Debug.Assert(version != VsVersion.Vs2010);
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
            return string.Format(contentFormat, GetAssemblyVersionNumber(version));
        }

        internal static string GetAssemblyVersionNumber(VsVersion version)
        {
            return string.Format("{0}.0.0.0", VsVersionUtil.GetVersionNumber(version));
        }
    }
}
