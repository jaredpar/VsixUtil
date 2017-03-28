using System;

namespace VsixUtil
{
    public class InstalledVersion
    {
        public Version Version { get; }

        public string ApplicationPath { get; }

        internal InstalledVersion(string applicationPath, Version version)
        {
            ApplicationPath = applicationPath;
            Version = version;
        }
    }
}
