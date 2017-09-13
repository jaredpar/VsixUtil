using System;

namespace VsixUtil
{
    [Serializable]
    public class InstalledVersion
    {
        public string ApplicationPath { get; }
        public VsVersion VsVersion { get; }
        public string Product { get; }

        public InstalledVersion(string applicationPath, VsVersion vsVersion, string product = null)
        {
            ApplicationPath = applicationPath;
            VsVersion = vsVersion;
            Product = product;
        }

        public override string ToString()
        {
            return $"{ApplicationPath} ({VsVersion} {Product})";
        }
    }
}
