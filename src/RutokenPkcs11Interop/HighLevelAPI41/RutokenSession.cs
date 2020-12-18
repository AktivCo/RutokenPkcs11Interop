﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.Common;
using Net.RutokenPkcs11Interop.Helpers;

using Net.RutokenPkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.LowLevelAPI41;

namespace Net.RutokenPkcs11Interop.HighLevelAPI41
{
    public class RutokenSession : Net.Pkcs11Interop.HighLevelAPI41.Session, IRutokenSession
    {
        internal RutokenSession(Pkcs11InteropFactories factories, LowLevelAPI41.RutokenPkcs11Library pkcs11Library, ulong sessionId)
            : base(factories, pkcs11Library, sessionId)
        {
        }

        public void UnblockUserPIN()
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_UnblockUserPIN(_sessionId);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_UnblockUserPIN", rv);
        }

        public void SetTokenName(string label)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            var labelArray = ConvertUtils.Utf8StringToBytes(label);
            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_SetTokenName(_sessionId, labelArray);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_SetTokenName", rv);
        }

        public string GetTokenLabel()
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            uint tokenLabelLength = 0;
            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_GetTokenName(_sessionId, null, ref tokenLabelLength);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_GetTokenName", rv);

            if (tokenLabelLength <= 0)
                throw new InvalidOperationException(
                    "C_EX_GetTokenName: invalid token label length");

            byte[] tokenLabel = new byte[tokenLabelLength];

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_GetTokenName(_sessionId, tokenLabel, ref tokenLabelLength);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_GetTokenName", rv);

            if (tokenLabel.Length != tokenLabelLength)
                Array.Resize(ref tokenLabel, Convert.ToInt32(tokenLabelLength));

            return ConvertUtils.BytesToUtf8String(tokenLabel);
        }

        public void SetLicense(ulong licenseNum, byte[] license)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (!Settings.LicenseAllowedNumbers.Contains(Convert.ToUInt32(licenseNum)))
                throw new ArgumentOutOfRangeException(nameof(licenseNum));

            if (license == null)
                throw new ArgumentNullException(nameof(license));

            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_SetLicense(
                _sessionId, Convert.ToUInt32(licenseNum), license);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_SetLicense", rv);
        }

        public byte[] GetLicense(ulong licenseNum)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (!Settings.LicenseAllowedNumbers.Contains(Convert.ToUInt32(licenseNum)))
                throw new ArgumentOutOfRangeException(nameof(licenseNum));

            uint licenseLen = 0;
            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_GetLicense(
                _sessionId, Convert.ToUInt32(licenseNum), null, ref licenseLen);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_GetLicense", rv);

            if (licenseLen == 0)
                throw new InvalidOperationException("C_EX_GetLicense: no license found");

            byte[] license = new byte[licenseLen];

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_GetLicense(
                _sessionId, Convert.ToUInt32(licenseNum), license, ref licenseLen);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_GetLicense", rv);

            return license;
        }

        public void LoadActivationKey(byte[] key)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_LoadActivationKey(
                _sessionId, key);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_LoadActivationKey", rv);
        }

        public byte[] GenerateActivationPassword(
            ActivationPasswordNumber passwordNumber, ActivationPasswordCharacterSet characterSet)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            // Получение длины пароля активации
            uint passwordLength = 0;
            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_GenerateActivationPassword(
                _sessionId, (uint)passwordNumber, null, ref passwordLength, (uint)characterSet);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_GenerateActivationPassword", rv);

            if (passwordLength <= 0)
                throw new InvalidOperationException(
                    "C_EX_GenerateActivationPassword: invalid password length");

            // Генерация пароля активации
            byte[] password = new byte[passwordLength];
            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_GenerateActivationPassword(
                _sessionId, (uint)passwordNumber, password, ref passwordLength, (uint)characterSet);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_GenerateActivationPassword", rv);

            return password;
        }

        public byte[] SignInvisible(
            IMechanism mechanism, IObjectHandle keyHandle, byte[] data)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (mechanism == null)
                throw new ArgumentNullException(nameof(mechanism));

            if (keyHandle == null)
                throw new ArgumentNullException(nameof(keyHandle));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var ckMechanism = new CK_MECHANISM()
            {
                Mechanism = ConvertUtils.UInt32FromUInt64(mechanism.Type)
            };

            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_SignInvisibleInit(_sessionId, ref ckMechanism,
                ConvertUtils.UInt32FromUInt64(keyHandle.ObjectId));
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_SignInvisibleInit", rv);

            uint signatureLen = 0;
            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_SignInvisible(_sessionId, data, Convert.ToUInt32(data.Length), null,
                ref signatureLen);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_SignInvisible", rv);

            if (signatureLen <= 0)
                throw new InvalidOperationException(
                    "C_EX_SignInvisible: invalid signature length");

            byte[] signature = new byte[signatureLen];
            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_SignInvisible(_sessionId, data, Convert.ToUInt32(data.Length),
                signature, ref signatureLen);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_SignInvisible", rv);

            if (signature.Length != signatureLen)
                Array.Resize(ref signature, Convert.ToInt32(signatureLen));

            return signature;
        }

        public string CreateCSR(IObjectHandle publicKey,
            string[] dn, IObjectHandle privateKey, string[] attributes, string[] extensions)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            IntPtr[] dnPtr = StringArrayHelpers.ConvertStringArrayToIntPtrArray(dn);
            IntPtr[] extsPtr = StringArrayHelpers.ConvertStringArrayToIntPtrArray(extensions);

            IntPtr csr;
            uint csrLength;

            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_CreateCSR(_sessionId, ConvertUtils.UInt32FromUInt64(publicKey.ObjectId),
                dnPtr, (uint) dnPtr.Length,
                out csr, out csrLength,
                ConvertUtils.UInt32FromUInt64(privateKey.ObjectId),
                null, 0,
                extsPtr, (uint) extsPtr.Length);

            StringArrayHelpers.FreeUnmanagedIntPtrArray(dnPtr);
            StringArrayHelpers.FreeUnmanagedIntPtrArray(extsPtr);

            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_CreateCSR", rv);

            try
            {
                var csrString = PKIHelpers.GetBase64CSR(csr, (int) csrLength);
                if (csrString.Length == 0)
                    throw new InvalidOperationException("C_EX_CreateCSR: invalid csr length");

                return csrString;
            }
            finally
            {
                rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_FreeBuffer(csr);
                if (rv != CKR.CKR_OK)
                    throw new Pkcs11Exception("C_EX_FreeBuffer", rv);
            }
        }

        public string GetCertificateInfoText(IObjectHandle certificate)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            IntPtr certificateInfo;
            uint certificateInfoLen;

            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_GetCertificateInfoText(
                _sessionId, ConvertUtils.UInt32FromUInt64(certificate.ObjectId), out certificateInfo, out certificateInfoLen);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_GetCertificateInfoText", rv);

            if (certificateInfoLen <= 0)
                throw new InvalidOperationException(
                    "C_EX_GetCertificateInfoText: invalid certificate info length");

            byte[] certificateInfoArray = new byte[certificateInfoLen];
            Marshal.Copy(certificateInfo, certificateInfoArray, 0, (int) certificateInfoLen);

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_FreeBuffer(certificateInfo);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_FreeBuffer", rv);

            return ConvertUtils.BytesToUtf8String(certificateInfoArray);
        }

        public byte[] PKCS7Sign(byte[] data, IObjectHandle certificate,
            IObjectHandle privateKey, ulong[] certificates, ulong flags)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            IntPtr signature;
            uint signatureLen;

            uint[] certificates40 = certificates.Select(cert => (uint)cert).ToArray();

            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_PKCS7Sign(_sessionId, data, ConvertUtils.UInt32FromUInt64(certificate.ObjectId),
                out signature, out signatureLen,
                ConvertUtils.UInt32FromUInt64(privateKey.ObjectId),
                certificates40, Convert.ToUInt32(flags));
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_PKCS7Sign", rv);

            if (signatureLen <= 0)
                throw new InvalidOperationException("C_EX_PKCS7Sign: invalid signature length");

            byte[] signatureArray = new byte[signatureLen];
            Marshal.Copy(signature, signatureArray, 0, (int) signatureLen);

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_FreeBuffer(signature);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_FreeBuffer", rv);

            return signatureArray;
        }

        public Pkcs7VerificationResult PKCS7Verify(byte[] cms, CkVendorX509Store vendorX509Store,
            VendorCrlMode mode, ulong flags)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (cms == null)
                throw new ArgumentNullException(nameof(cms));

            if (vendorX509Store == null)
                throw new ArgumentNullException(nameof(vendorX509Store));

            var storeNative = new LowLevelAPI41.CK_VENDOR_X509_STORE(vendorX509Store);

            var data = IntPtr.Zero;
            uint dataLen = 0;

            var initialSignerSertificates = IntPtr.Zero;
            var signerSertificates = IntPtr.Zero;
            uint signerSertificatesCount = 0;

            try
            {
                CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_PKCS7VerifyInit(_sessionId, cms, ref storeNative, Convert.ToUInt32(mode), Convert.ToUInt32(flags));
                if (rv != CKR.CKR_OK)
                    throw new Pkcs11Exception("C_EX_PKCS7VerifyInit", rv);

                rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_PKCS7Verify(_sessionId, out data, out dataLen, out signerSertificates, out signerSertificatesCount);

                var result = new Pkcs7VerificationResult();

                if (rv == CKR.CKR_OK)
                {
                    result.Data = new byte[dataLen];
                    Marshal.Copy(data, result.Data, 0, (int)dataLen);

                    result.Certificates = new List<byte[]>();
                    var structSize = Marshal.SizeOf(typeof(LowLevelAPI41.CK_VENDOR_BUFFER));
                    initialSignerSertificates = signerSertificates;

                    for (var i = 0; i < signerSertificatesCount; i++)
                    {
                        var certificatePtr = (LowLevelAPI41.CK_VENDOR_BUFFER)Marshal.PtrToStructure(signerSertificates, typeof(LowLevelAPI41.CK_VENDOR_BUFFER));
                        signerSertificates += structSize;

                        var certificateData = new byte[certificatePtr.Size];
                        Marshal.Copy(certificatePtr.Data, certificateData, 0, (int)certificatePtr.Size);

                        result.Certificates.Add(certificateData);
                    }

                    result.IsValid = true;
                }
                else if (rv == CKR.CKR_SIGNATURE_INVALID)
                {
                    result.IsValid = false;
                }
                else
                {
                    throw new Pkcs11Exception("C_EX_PKCS7Verify", rv);
                }

                return result;
            }
            finally
            {
                storeNative.Dispose();

                if (initialSignerSertificates != IntPtr.Zero)
                {
                    var structSize = Marshal.SizeOf(typeof(LowLevelAPI41.CK_VENDOR_BUFFER));

                    for (var i = 0; i < signerSertificatesCount; i++)
                    {
                        var certificatePtr = (LowLevelAPI41.CK_VENDOR_BUFFER)Marshal.PtrToStructure(initialSignerSertificates, typeof(LowLevelAPI41.CK_VENDOR_BUFFER));
                        initialSignerSertificates += structSize;

                        CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_FreeBuffer(certificatePtr.Data);
                        if (rv != CKR.CKR_OK)
                            throw new Pkcs11Exception("C_EX_FreeBuffer", rv);
                    }
                }

                if (data != IntPtr.Zero)
                {
                    CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_FreeBuffer(data);
                    if (rv != CKR.CKR_OK)
                        throw new Pkcs11Exception("C_EX_FreeBuffer", rv);
                }
            }
        }

        public Pkcs7VerificationResult PKCS7Verify(byte[] cms, Stream inputStream, CkVendorX509Store vendorX509Store,
            VendorCrlMode mode, ulong flags)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (cms == null)
                throw new ArgumentNullException(nameof(cms));

            if (vendorX509Store == null)
                throw new ArgumentNullException(nameof(vendorX509Store));

            var storeNative = new LowLevelAPI41.CK_VENDOR_X509_STORE(vendorX509Store);

            var initialSignerSertificates = IntPtr.Zero;
            var signerSertificates = IntPtr.Zero;
            uint signerSertificatesCount = 0;

            try
            {
                CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_PKCS7VerifyInit(_sessionId, cms, ref storeNative, Convert.ToUInt32(mode), Convert.ToUInt32(flags));
                if (rv != CKR.CKR_OK)
                    throw new Pkcs11Exception("C_EX_PKCS7VerifyInit", rv);

                byte[] part = new byte[inputStream.Length];

                while (inputStream.Read(part, 0, part.Length) > 0)
                {
                    rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_PKCS7VerifyUpdate(_sessionId, part);
                    if (rv != CKR.CKR_OK)
                        throw new Pkcs11Exception("C_EX_PKCS7VerifyUpdate", rv);
                }

                rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_PKCS7VerifyFinal(_sessionId, out signerSertificates, out signerSertificatesCount);

                var result = new Pkcs7VerificationResult();

                if (rv == CKR.CKR_OK)
                {
                    result.Certificates = new List<byte[]>();
                    var structSize = Marshal.SizeOf(typeof(LowLevelAPI41.CK_VENDOR_BUFFER));
                    initialSignerSertificates = signerSertificates;
                    for (var i = 0; i < signerSertificatesCount; i++)
                    {
                        var certificatePtr = (LowLevelAPI41.CK_VENDOR_BUFFER)Marshal.PtrToStructure(signerSertificates, typeof(LowLevelAPI41.CK_VENDOR_BUFFER));
                        signerSertificates += structSize;

                        var certificateData = new byte[certificatePtr.Size];
                        Marshal.Copy(certificatePtr.Data, certificateData, 0, (int)certificatePtr.Size);

                        result.Certificates.Add(certificateData);
                    }

                    result.IsValid = true;
                }
                else if (rv == CKR.CKR_SIGNATURE_INVALID)
                {
                    result.IsValid = false;
                }
                else
                {
                    throw new Pkcs11Exception("C_EX_PKCS7VerifyFinal", rv);
                }

                return result;
            }
            finally
            {
                storeNative.Dispose();

                if (initialSignerSertificates != IntPtr.Zero)
                {
                    var structSize = Marshal.SizeOf(typeof(LowLevelAPI41.CK_VENDOR_BUFFER));

                    for (var i = 0; i < signerSertificatesCount; i++)
                    {
                        var certificatePtr = (LowLevelAPI41.CK_VENDOR_BUFFER)Marshal.PtrToStructure(initialSignerSertificates, typeof(LowLevelAPI41.CK_VENDOR_BUFFER));
                        initialSignerSertificates += structSize;

                        CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_FreeBuffer(certificatePtr.Data);
                        if (rv != CKR.CKR_OK)
                            throw new Pkcs11Exception("C_EX_FreeBuffer", rv);
                    }
                }
            }
        }

        public void TokenManage(TokenManageMode mode, byte[] value)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            IntPtr valuePtr = Marshal.AllocHGlobal(value.Length);
            Marshal.Copy(value, 0, valuePtr, value.Length);

            try
            {
                CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_TokenManage(_sessionId, (uint)mode, valuePtr);
                if (rv != CKR.CKR_OK)
                    throw new Pkcs11Exception("C_EX_TokenManage", rv);
            }
            finally
            {
                Marshal.FreeHGlobal(valuePtr);
            }
        }

        public byte[] ExtendedWrapKey(
            IMechanism generationMechanism, List<IObjectAttribute> keyAttributes,
            IMechanism derivationMechanism, IObjectHandle baseKey,
            IMechanism wrappingMechanism, ref IObjectHandle key)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (generationMechanism == null)
                throw new ArgumentNullException(nameof(generationMechanism));

            if (derivationMechanism == null)
                throw new ArgumentNullException(nameof(derivationMechanism));

            if (keyAttributes == null)
                throw new ArgumentNullException(nameof(keyAttributes));

            if (baseKey == null)
                throw new ArgumentNullException(nameof(baseKey));

            if (wrappingMechanism == null)
                throw new ArgumentNullException(nameof(wrappingMechanism));

            var ckGenerationMechanism = new CK_MECHANISM()
            {
                Mechanism = ConvertUtils.UInt32FromUInt64(generationMechanism.Type)
            };

            var ckDerivationMechanism = new CK_MECHANISM()
            {
                Mechanism = ConvertUtils.UInt32FromUInt64(derivationMechanism.Type)
            };
            var ckWrappingMechanism = new CK_MECHANISM()
            {
                Mechanism = ConvertUtils.UInt32FromUInt64(wrappingMechanism.Type)
            };

            // Преобразование ObjectAttributes в CK_ATTRIBUTES
            CK_ATTRIBUTE[] ckKeyAttributes = null;
            uint ckKeyAttributesLen = 0;
            ckKeyAttributes = new CK_ATTRIBUTE[keyAttributes.Count];
            for (int i = 0; i < keyAttributes.Count; i++)
            {
                ckKeyAttributes[i] = (CK_ATTRIBUTE) keyAttributes[i].ToMarshalableStructure();
            }
            ckKeyAttributesLen = Convert.ToUInt32(keyAttributes.Count);

            // Получение длины wrapped key
            uint generatedKey = CK.CK_INVALID_HANDLE;
            uint wrappedKeyLen = 0;
            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_WrapKey(_sessionId, ref ckGenerationMechanism, ckKeyAttributes,
                ckKeyAttributesLen,
                ref ckDerivationMechanism, ConvertUtils.UInt32FromUInt64(baseKey.ObjectId), ref ckWrappingMechanism, null, ref wrappedKeyLen,
                ref generatedKey);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_WrapKey", rv);

            if (wrappedKeyLen <= 0)
                throw new InvalidOperationException(
                    "C_EX_WrapKey: invalid wrapped key length");

            // Маскирование ключа
            byte[] wrappedKey = new byte[wrappedKeyLen];
            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_WrapKey(_sessionId, ref ckGenerationMechanism, ckKeyAttributes,
                ckKeyAttributesLen,
                ref ckDerivationMechanism, ConvertUtils.UInt32FromUInt64(baseKey.ObjectId), ref ckWrappingMechanism, wrappedKey, ref wrappedKeyLen,
                ref generatedKey);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_WrapKey", rv);

            if (generatedKey == CK.CK_INVALID_HANDLE)
                throw new InvalidOperationException("C_EX_WrapKey: invalid generated key handle");

            if (wrappedKey.Length != wrappedKeyLen)
                Array.Resize(ref wrappedKey, Convert.ToInt32(wrappedKeyLen));

            key = new Net.Pkcs11Interop.HighLevelAPI41.ObjectHandle(generatedKey);

            return wrappedKey;
        }

        public IObjectHandle ExtendedUnwrapKey(
            IMechanism derivationMechanism, IObjectHandle baseKey,
            IMechanism unwrappingMechanism,
            byte[] wrappedKey, List<IObjectAttribute> keyAttributes)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (derivationMechanism == null)
                throw new ArgumentNullException(nameof(derivationMechanism));

            if (baseKey == null)
                throw new ArgumentNullException(nameof(baseKey));

            if (unwrappingMechanism == null)
                throw new ArgumentNullException(nameof(unwrappingMechanism));

            if (wrappedKey == null)
                throw new ArgumentNullException(nameof(wrappedKey));

            if (keyAttributes == null)
                throw new ArgumentNullException(nameof(keyAttributes));

            var ckDerivationMechanism = new CK_MECHANISM()
            {
                Mechanism = ConvertUtils.UInt32FromUInt64(derivationMechanism.Type)
            };
            var ckUnwrappingMechanism = new CK_MECHANISM()
            {
                Mechanism = ConvertUtils.UInt32FromUInt64(unwrappingMechanism.Type)
            };

            // Преобразование ObjectAttributes в CK_ATTRIBUTES
            CK_ATTRIBUTE[] ckKeyAttributes = null;
            uint ckKeyAttributesLen = 0;
            ckKeyAttributes = new CK_ATTRIBUTE[keyAttributes.Count];
            for (int i = 0; i < keyAttributes.Count; i++)
            {
                ckKeyAttributes[i] = (CK_ATTRIBUTE)keyAttributes[i].ToMarshalableStructure();
            }
            ckKeyAttributesLen = Convert.ToUInt32(keyAttributes.Count);

            // Размаскирование ключа
            uint unwrappedKey = CK.CK_INVALID_HANDLE;
            CKR rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_EX_UnwrapKey(_sessionId,
                ref ckDerivationMechanism, ConvertUtils.UInt32FromUInt64(baseKey.ObjectId),
                ref ckUnwrappingMechanism, wrappedKey, (uint) wrappedKey.Length,
                ckKeyAttributes, ckKeyAttributesLen,
                ref unwrappedKey);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_EX_WrapKey", rv);

            if (unwrappedKey == CK.CK_INVALID_HANDLE)
                throw new InvalidOperationException("C_EX_WrapKey: invalid unwrapped key handle");

            return new Net.Pkcs11Interop.HighLevelAPI41.ObjectHandle(unwrappedKey);
        }

        public IObjectHandle GetPinPolicyObject(CKU userType)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            CKR rv;
            CK_ATTRIBUTE[] pinPolicyTemplate = new CK_ATTRIBUTE[3];

            pinPolicyTemplate[0] = CkaUtils.CreateAttribute(CKA.CKA_CLASS, CKO.CKO_HW_FEATURE);
            pinPolicyTemplate[1] = CkaUtils.CreateAttribute(CKA.CKA_HW_FEATURE_TYPE, (uint)Extended_CKH.CKH_VENDOR_PIN_POLICY);
            pinPolicyTemplate[2] = CkaUtils.CreateAttribute((CKA) Extended_CKA.CKA_VENDOR_USER_TYPE, (uint) userType);

            uint foundObjectCount = 0;
            uint[] foundObjectIds = new uint[1];

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_FindObjectsInit(_sessionId, pinPolicyTemplate, Convert.ToUInt32(pinPolicyTemplate.Length));
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_FindObjectsInit", rv);

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_FindObjects(_sessionId, foundObjectIds, Convert.ToUInt32(foundObjectIds.Length), ref foundObjectCount);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_FindObjects", rv);

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_FindObjectsFinal(_sessionId);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_FindObjectsFinal", rv);

            if (foundObjectCount != 1)
                throw new Exception("Pin policy object not found");

            return new Net.Pkcs11Interop.HighLevelAPI41.ObjectHandle(foundObjectIds[0]);
        }

        public bool PinPolicySupports(CKU userType)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            CKR rv;
            IObjectHandle pinPolicyObj = GetPinPolicyObject(userType);
            CK_ATTRIBUTE[] pinPolicyTemplate = new CK_ATTRIBUTE[1];
            pinPolicyTemplate[0] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_SUPPORTED_PIN_POLICIES);

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_GetAttributeValue(_sessionId, ConvertUtils.UInt32FromUInt64(pinPolicyObj.ObjectId), pinPolicyTemplate, Convert.ToUInt32(pinPolicyTemplate.Length));
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_GetAttributeValue", rv);

            return pinPolicyTemplate[0].valueLen != 0;
        }

        public PinPolicy GetPinPolicy(CKU userType)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            CKR rv;
            PinPolicy pinPolicy = new PinPolicy();
            IObjectHandle pinPolicyObj = GetPinPolicyObject(userType);

            CK_ATTRIBUTE[] pinPolicyTemplate = new CK_ATTRIBUTE[10];
            pinPolicyTemplate[0] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_MIN_LENGTH, new byte[1]);
            pinPolicyTemplate[1] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_HISTORY_DEPTH, new byte[1]);
            pinPolicyTemplate[2] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_ALLOW_DEFAULT_PIN_USAGE, new byte[1]);
            pinPolicyTemplate[3] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_DIGIT_REQUIRED, new byte[1]);
            pinPolicyTemplate[4] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_UPPERCASE_REQUIRED, new byte[1]);
            pinPolicyTemplate[5] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_LOWERCASE_REQUIRED, new byte[1]);
            pinPolicyTemplate[6] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_SPEC_CHAR_REQUIRED, new byte[1]);
            pinPolicyTemplate[7] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_DIFF_CHARS_REQUIRED, new byte[1]);
            pinPolicyTemplate[8] = CkaUtils.CreateAttribute(CKA.CKA_MODIFIABLE, new bool());
            pinPolicyTemplate[9] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICIES_DELETABLE, new bool());

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_GetAttributeValue(_sessionId, ConvertUtils.UInt32FromUInt64(pinPolicyObj.ObjectId), pinPolicyTemplate, Convert.ToUInt32(pinPolicyTemplate.Length));
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_GetAttributeValue", rv);

            byte[] minPolicyLength;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[0], out minPolicyLength);
            pinPolicy.MinPinLength = minPolicyLength[0];

            byte[] pinHistoryDepth;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[1], out pinHistoryDepth);
            pinPolicy.PinHistoryDepth = pinHistoryDepth[0];

            byte[] allowDefaultPinUsage;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[2], out allowDefaultPinUsage);
            pinPolicy.AllowDefaultPinUsage = allowDefaultPinUsage[0] != 0;

            byte[] pinContainsDigit;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[3], out pinContainsDigit);
            pinPolicy.PinContainsDigit = pinContainsDigit[0] != 0;

            byte[] pinContainsUpperLetter;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[4], out pinContainsUpperLetter);
            pinPolicy.PinContainsUpperLetter = pinContainsUpperLetter[0] != 0;

            byte[] pinContainsLowerLetter;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[5], out pinContainsLowerLetter);
            pinPolicy.PinContainsLowerLetter = pinContainsLowerLetter[0] != 0;

            byte[] pinContainsSpecChar;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[6], out pinContainsSpecChar);
            pinPolicy.PinContainsSpecChar = pinContainsSpecChar[0] != 0;

            byte[] restrictOneCharPin;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[7], out restrictOneCharPin);
            pinPolicy.RestrictOneCharPin = restrictOneCharPin[0] != 0;

            bool allowChangePinPolicy;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[8], out allowChangePinPolicy);
            pinPolicy.AllowChangePinPolicy = allowChangePinPolicy;

            bool removePinPolicyAfterFormat;
            CkaUtils.ConvertValue(ref pinPolicyTemplate[9], out removePinPolicyAfterFormat);
            pinPolicy.RemovePinPolicyAfterFormat = removePinPolicyAfterFormat;

            return pinPolicy;
        }

        public void SetPinPolicy(PinPolicy pinPolicy, CKU userType)
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            CKR rv;
            IObjectHandle pinPolicyObj = GetPinPolicyObject(userType);


            CK_ATTRIBUTE[] pinPolicyTemplate = new CK_ATTRIBUTE[10];
            uint len = 0;

            if (pinPolicy.MinPinLength != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_MIN_LENGTH, new byte[] { Convert.ToByte(pinPolicy.MinPinLength) });
            if (pinPolicy.PinHistoryDepth != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_HISTORY_DEPTH, new byte[] { Convert.ToByte(pinPolicy.PinHistoryDepth) });
            if (pinPolicy.AllowDefaultPinUsage != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_ALLOW_DEFAULT_PIN_USAGE, new byte[] { Convert.ToByte(pinPolicy.AllowDefaultPinUsage) });
            if (pinPolicy.PinContainsDigit != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_DIGIT_REQUIRED, new byte[] { Convert.ToByte(pinPolicy.PinContainsDigit) });
            if (pinPolicy.PinContainsUpperLetter != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_UPPERCASE_REQUIRED, new byte[] { Convert.ToByte(pinPolicy.PinContainsUpperLetter) });
            if (pinPolicy.PinContainsLowerLetter != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_LOWERCASE_REQUIRED, new byte[] { Convert.ToByte(pinPolicy.PinContainsLowerLetter) });
            if (pinPolicy.PinContainsSpecChar != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_SPEC_CHAR_REQUIRED, new byte[] { Convert.ToByte(pinPolicy.PinContainsSpecChar) });
            if (pinPolicy.RestrictOneCharPin != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICY_DIFF_CHARS_REQUIRED, new byte[] { Convert.ToByte(pinPolicy.RestrictOneCharPin) });
            if (pinPolicy.AllowChangePinPolicy != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute(CKA.CKA_MODIFIABLE, Convert.ToBoolean(pinPolicy.AllowChangePinPolicy));
            if (pinPolicy.RemovePinPolicyAfterFormat != null)
                pinPolicyTemplate[len++] = CkaUtils.CreateAttribute((CKA)Extended_CKA.CKA_VENDOR_PIN_POLICIES_DELETABLE, Convert.ToBoolean(pinPolicy.RemovePinPolicyAfterFormat));

            rv = ((LowLevelAPI41.RutokenPkcs11Library)_pkcs11Library).C_SetAttributeValue(_sessionId, ConvertUtils.UInt32FromUInt64(pinPolicyObj.ObjectId), pinPolicyTemplate, len);
            if (rv != CKR.CKR_OK)
                throw new Pkcs11Exception("C_GetAttributeValue", rv);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}