namespace VsixUtil
{
    internal class InstalledVersion
    {
        internal InstalledVersion(string applicationPath, Version version)
        {
            ApplicationPath = applicationPath;
            Version = version;
        }

        public Version Version
        {
            get;
        }

        public string ApplicationPath
        {
            get;
        }

    }
}
