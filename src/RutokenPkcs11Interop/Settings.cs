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
        private static string Pkcs11LibraryName
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
                    // new version of pkcs11 is destributed like framework
                    var framework = "rtpkcs11ecp.framework/rtpkcs11ecp";
                    if (File.Exists(Path.Combine(Environment.SystemDirectory, framework)))
                        return framework;

                    // old version of pkcs11 is destributed like dylib
                    var dylib = "librtpkcs11ecp.dylib";
                    if (File.Exists(Path.Combine(Environment.SystemDirectory, dylib)))
                        return dylib;

                    // prefer frameworks
                    return framework;
                }
                throw new InvalidOperationException("Native rutoken library path is not set");
            }
        }

        public static string RutokenEcpDllDefaultPath =>
            Path.Combine(Environment.SystemDirectory, Pkcs11LibraryName);

        public static bool OsLockingDefault => true;

        public static uint DefaultLicenseLength => 72;

        public static List<uint> LicenseAllowedNumbers => new List<uint> {1, 2};
    }
}
