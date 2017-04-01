namespace VsixUtil
{
    public class InstalledVersion
    {
        public string ApplicationPath { get; }
        public VsVersion VsVersion { get; }

        internal InstalledVersion(string applicationPath, VsVersion vsVersion)
        {
            ApplicationPath = applicationPath;
            VsVersion = vsVersion;
        }
    }
}
