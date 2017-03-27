using System;
using System.IO;
using System.Diagnostics;

namespace VsixUtil
{
    class VersionManagerRunner
    {
        public static void Run(string appPath, Version version, string rootSuffix, ToolAction toolAction, string arg1)
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
                versionManager.Run(appPath, version, rootSuffix, toolAction, arg1);
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
    }
}
