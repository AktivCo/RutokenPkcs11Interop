﻿using System;
using System.IO;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI41;
using NUnit.Framework;
using RutokenPkcs11Interop;

namespace RutokenPkcs11InteropTests.HighLevelAPI41
{
    [TestFixture()]
    class _HL41_20_EncryptAndDecryptTest
    {
        /// <summary>
        /// C_EncryptInit, C_Encrypt, C_DecryptInit and C_Decrypt test.
        /// </summary>
        [Test()]
        public void _HL41_20_01_EncryptAndDecrypt_Gost28147_89_ECB_Test()
        {
            if (Platform.UnmanagedLongSize != 4 || Platform.StructPackingSize != 1)
                Assert.Inconclusive("Test cannot be executed on this platform");

            using (Pkcs11 pkcs11 = new Pkcs11(Settings.Pkcs11LibraryPath, Settings.UseOsLocking))
            {
                // Find first slot with token present
                Slot slot = Helpers.GetUsableSlot(pkcs11);

                // Open RW session
                using (Session session = slot.OpenSession(false))
                {
                    // Login as normal user
                    session.Login(CKU.CKU_USER, Settings.NormalUserPin);

                    // Generate symetric key
                    ObjectHandle generatedKey = Helpers.GenerateGostSymmetricKey(session);

                    Mechanism mechanism = new Mechanism((uint) Extended_CKM.CKM_GOST28147_ECB);

                    byte[] sourceData = TestData.Encrypt_Gost28147_89_ECB_SourceData;

                    // Encrypt data
                    byte[] encryptedData = session.Encrypt(mechanism, generatedKey, sourceData);

                    // Decrypt data
                    byte[] decryptedData = session.Decrypt(mechanism, generatedKey, encryptedData);

                    Assert.IsTrue(Convert.ToBase64String(sourceData) == Convert.ToBase64String(decryptedData));

                    session.DestroyObject(generatedKey);
                    session.Logout();
                }
            }
        }

        /// <summary>
        /// C_EncryptInit, C_EncryptUpdate, C_EncryptFinish, C_DecryptInit, C_DecryptUpdate and C_DecryptFinish test.
        /// </summary>
        [Test()]
        public void _HL41_20_02_EncryptAndDecrypt_Gost28147_89_Stream_Test()
        {
            if (Platform.UnmanagedLongSize != 4 || Platform.StructPackingSize != 1)
                Assert.Inconclusive("Test cannot be executed on this platform");

            using (Pkcs11 pkcs11 = new Pkcs11(Settings.Pkcs11LibraryPath, Settings.UseOsLocking))
            {
                // Find first slot with token present
                Slot slot = Helpers.GetUsableSlot(pkcs11);

                // Open RW session
                using (Session session = slot.OpenSession(false))
                {
                    // Login as normal user
                    session.Login(CKU.CKU_USER, Settings.NormalUserPin);

                    // Generate symetric key
                    ObjectHandle generatedKey = Helpers.GenerateGostSymmetricKey(session);

                    // Specify encryption mechanism with initialization vector as parameter
                    Mechanism mechanism = new Mechanism((uint)Extended_CKM.CKM_GOST28147);

                    byte[] sourceData = TestData.Encrypt_Gost28147_89_SourceData;
                    byte[] encryptedData = null;
                    byte[] decryptedData = null;

                    // Multipart encryption can be used i.e. for encryption of streamed data
                    using (MemoryStream inputStream = new MemoryStream(sourceData), outputStream = new MemoryStream())
                    {
                        // Encrypt data
                        // Note that in real world application we would rather use bigger read buffer i.e. 4096
                        session.Encrypt(mechanism, generatedKey, inputStream, outputStream, 8);

                        // Read whole output stream to the byte array so we can compare results more easily
                        encryptedData = outputStream.ToArray();
                    }

                    // Do something interesting with encrypted data

                    // Multipart decryption can be used i.e. for decryption of streamed data
                    using (MemoryStream inputStream = new MemoryStream(encryptedData), outputStream = new MemoryStream())
                    {
                        // Decrypt data
                        // Note that in real world application we would rather use bigger read buffer i.e. 4096
                        session.Decrypt(mechanism, generatedKey, inputStream, outputStream, 8);

                        // Read whole output stream to the byte array so we can compare results more easily
                        decryptedData = outputStream.ToArray();
                    }

                    // Do something interesting with decrypted data
                    Assert.IsTrue(Convert.ToBase64String(sourceData) == Convert.ToBase64String(decryptedData));

                    session.DestroyObject(generatedKey);
                    session.Logout();
                }
            }
        }
    }
}