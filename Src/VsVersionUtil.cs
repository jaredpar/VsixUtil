using System;

namespace VsixUtil
{
    public class VsVersionUtil
    {
        public static VsVersion ToVsVersion(string version)
        {
            int versionNumber;
            if (!int.TryParse(version, out versionNumber))
            {
                throw new Exception("Bad Version");
            }

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
                case 2010:
                    return VsVersion.Vs2010;
                case 11:
                case 2012:
                    return VsVersion.Vs2012;
                case 12:
                case 2013:
                    return VsVersion.Vs2013;
                case 14:
                case 2015:
                    return VsVersion.Vs2015;
                case 15:
                case 2017:
                    return VsVersion.Vs2017;
                default:
                    throw new Exception("Bad Version");
            }
        }
    }
}
