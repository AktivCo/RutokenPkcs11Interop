using System;
using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.Common;

namespace Net.RutokenPkcs11Interop.HighLevelAPI
{
    public interface IVolumeInfo
    {
        ulong VolumeSize { get; }

        FlashAccessMode AccessMode { get; }

        CKU VolumeOwner { get; }

        ulong Flags { get; }
    }
}
