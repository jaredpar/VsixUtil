using System;

namespace VsixUtil
{
    public class VsVersionUtil
    {
        public static VsVersion ToVsVersion(string version)
        {
            var versionNumber = int.Parse(version);
            return GetVsVersion(versionNumber);
        }

        public static int GetVersionNumber(VsVersion version)
        {
            switch (version)
            {
                case VsVersion.Vs2010:
                    return 10;
                case VsVersion.Vs2012:
                    return 11;
                case VsVersion.Vs2013:
                    return 12;
                case VsVersion.Vs2015:
                    return 14;
                case VsVersion.Vs2017:
                    return 15;
                default:
                    throw new Exception("Bad Version");
            }
        }

        public static VsVersion GetVsVersion(Version version)
        {
            return GetVsVersion(version.Major);
        }

        public static VsVersion GetVsVersion(int versionNumber)
        {
            switch (versionNumber)
            {
                case 10:
                    return VsVersion.Vs2010;
                case 11:
                    return VsVersion.Vs2012;
                case 12:
                    return VsVersion.Vs2013;
                case 14:
                    return VsVersion.Vs2015;
                case 15:
                    return VsVersion.Vs2017;
                default:
                    throw new Exception("Bad Version");
            }
        }
    }
}
