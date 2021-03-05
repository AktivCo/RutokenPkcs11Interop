﻿using System;
using Net.Pkcs11Interop.Common;
using Net.RutokenPkcs11Interop.LowLevelAPI80.MechanismParams;
using Net.RutokenPkcs11Interop.HighLevelAPI.MechanismParams;

using NativeULong = System.UInt64;

// Note: Code in this file is generated automatically

namespace Net.RutokenPkcs11Interop.HighLevelAPI80.MechanismParams
{
    public class CkVendorGostKegParams : ICkVendorGostKegParams
    {
        /// <summary>
        /// Flag indicating whether instance has been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Low level mechanism parameters
        /// </summary>
        private readonly CK_VENDOR_GOST_KEG_PARAMS _lowLevelStruct;

        public CkVendorGostKegParams(byte[] publicData, byte[] ukm)
        {
            _lowLevelStruct.pPublicData = IntPtr.Zero;
            _lowLevelStruct.ulPublicDataLen = 0;
            _lowLevelStruct.pUKM = IntPtr.Zero;
            _lowLevelStruct.ulUKMLen = 0;

            if (publicData == null)
                throw new ArgumentNullException(nameof(publicData));

            if (publicData.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(publicData), "Array has to be not null length");

            if (ukm == null)
                throw new ArgumentNullException(nameof(ukm));

            if (ukm.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(ukm), "has to be not null length");

            _lowLevelStruct.ulPublicDataLen = (NativeULong)publicData.Length;
            _lowLevelStruct.pPublicData = UnmanagedMemory.Allocate(publicData.Length);
            UnmanagedMemory.Write(_lowLevelStruct.pPublicData, publicData);

            _lowLevelStruct.ulUKMLen = (NativeULong)ukm.Length;
            _lowLevelStruct.pUKM = UnmanagedMemory.Allocate(ukm.Length);
            UnmanagedMemory.Write(_lowLevelStruct.pUKM, ukm);
        }

        #region IMechanismParams

        /// <summary>
        /// Returns managed object that can be marshaled to an unmanaged block of memory
        /// </summary>
        /// <returns>A managed object holding the data to be marshaled. This object must be an instance of a formatted class.</returns>
        public object ToMarshalableStructure()
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            return _lowLevelStruct;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Disposes object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes object
        /// </summary>
        /// <param name="disposing">Flag indicating whether managed resources should be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Dispose managed objects
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Class destructor that disposes object if caller forgot to do so
        /// </summary>
        ~CkVendorGostKegParams()
        {
            Dispose(false);
        }

        #endregion
    }
}
