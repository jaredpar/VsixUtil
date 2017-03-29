using System;

namespace VsixUtil
{
    public class InstalledVersion
    {
        public string ApplicationPath { get; }
        public Version Version { get; }
        public VsVersion VsVersion { get; }

        internal InstalledVersion(string applicationPath, Version version)
        {
            ApplicationPath = applicationPath;
            Version = version;
            VsVersion = CommonUtil.GetVsVersion(version.Major);
        }
    }
}
