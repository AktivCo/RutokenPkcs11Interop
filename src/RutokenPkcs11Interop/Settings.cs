using System;
using System.Collections.Generic;
using System.IO;
using Net.Pkcs11Interop.Common;

namespace Net.RutokenPkcs11Interop
{
    public static class Settings
    {
        /// <summary>
        /// Relative name or absolute path of unmanaged PKCS#11 library provided by smartcard or HSM vendor.
        /// </summary>
        private static string Pkcs11LibraryPath
        {
            get
            {
                if (Platform.IsWindows)
                {
                    return "rtpkcs11ecp.dll";
                }
                else if (Platform.IsLinux)
                {
                    return "librtpkcs11ecp.so";
                }
                else if (Platform.IsMacOsX)
                {
                    return "rtpkcs11ecp.framework/rtpkcs11ecp";
                }
                throw new InvalidOperationException("Native rutoken library path is not set");
            }
        }

        public static string RutokenEcpDllDefaultPath =>
            Path.Combine(Environment.SystemDirectory, Pkcs11LibraryPath);

        public static bool OsLockingDefault => true;

        public static uint DefaultLicenseLength => 72;

        public static List<uint> LicenseAllowedNumbers => new List<uint> {1, 2};
    }
}
