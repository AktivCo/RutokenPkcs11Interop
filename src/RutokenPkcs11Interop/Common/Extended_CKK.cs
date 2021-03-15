﻿using Net.Pkcs11Interop.Common;

namespace Net.RutokenPkcs11Interop.Common
{
    public enum Extended_CKK : uint
    {
        NSSCK_VENDOR_PKCS11_RU_TEAM = 0xD4321000,
        CK_VENDOR_PKCS11_RU_TEAM_TC26 = NSSCK_VENDOR_PKCS11_RU_TEAM,

        CKK_GOSTR3410_512 = Extended_CKM.CK_VENDOR_PKCS11_RU_TEAM_TK26 | 0x003,
        CKK_KUZNECHIK = CK_VENDOR_PKCS11_RU_TEAM_TC26 | 0x004,
        CKK_KUZNYECHIK = CKK_KUZNECHIK,
        CKK_MAGMA = CK_VENDOR_PKCS11_RU_TEAM_TC26 | 0x005
    }
}
