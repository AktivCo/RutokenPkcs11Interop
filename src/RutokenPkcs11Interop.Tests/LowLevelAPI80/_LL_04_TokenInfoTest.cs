﻿using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.LowLevelAPI80;
using Net.RutokenPkcs11Interop.LowLevelAPI80;

using NativeULong = System.UInt64;

namespace Net.RutokenPkcs11InteropTests.LowLevelAPI80
{
    [TestFixture()]
    public class _LL_04_TokenInfoTest
    {
        /// <summary>
        /// Basic C_GetTokenInfo test.
        /// </summary>
        [Test()]
        public void _LL_04_01_TokenInfoTest()
        {
            Helpers.CheckPlatform();

            CKR rv = CKR.CKR_OK;

            using (RutokenPkcs11Library pkcs11 = new RutokenPkcs11Library(Settings.Pkcs11LibraryPath))
            {
                // Инициализация библиотеки
                rv = pkcs11.C_Initialize(Settings.InitArgs80);
                if ((rv != CKR.CKR_OK) && (rv != CKR.CKR_CRYPTOKI_ALREADY_INITIALIZED))
                    Assert.Fail(rv.ToString());

                // Установление соединения с Рутокен в первом доступном слоте
                NativeULong slotId = Helpers.GetUsableSlot(pkcs11);

                // Получение информации о токене
                var tokenInfo = new CK_TOKEN_INFO();
                rv = pkcs11.C_GetTokenInfo(slotId, ref tokenInfo);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());

                Assert.IsFalse(String.IsNullOrEmpty(ConvertUtils.BytesToUtf8String(tokenInfo.ManufacturerId)));

                // Завершение сессии
                rv = pkcs11.C_Finalize(IntPtr.Zero);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
            }
        }

        /// <summary>
        /// C_EX_GetTokenInfoExtended test.
        /// </summary>
        [Test()]
        public void _LL_04_02_TokenInfoExtendedTest()
        {
            Helpers.CheckPlatform();

            CKR rv = CKR.CKR_OK;

            using (var pkcs11 = new RutokenPkcs11Library(Settings.Pkcs11LibraryPath))
            {
                // Инициализация библиотеки
                rv = pkcs11.C_Initialize(Settings.InitArgs80);
                if ((rv != CKR.CKR_OK) && (rv != CKR.CKR_CRYPTOKI_ALREADY_INITIALIZED))
                    Assert.Fail(rv.ToString());

                // Установление соединения с Рутокен в первом доступном слоте
                NativeULong slotId = Helpers.GetUsableSlot(pkcs11);

                // Получение расширенной информации о токене
                var tokenInfo = new CK_TOKEN_INFO_EXTENDED
                {
                    SizeofThisStructure = Convert.ToUInt64(Marshal.SizeOf(typeof(CK_TOKEN_INFO_EXTENDED)))
                };

                rv = pkcs11.C_EX_GetTokenInfoExtended(slotId, ref tokenInfo);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());

                Assert.IsFalse(String.IsNullOrEmpty(ConvertUtils.BytesToUtf8String(tokenInfo.ATR)));

                // Завершение сессии
                rv = pkcs11.C_Finalize(IntPtr.Zero);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
            }
        }
    }
}
