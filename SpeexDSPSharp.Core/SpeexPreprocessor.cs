﻿using SpeexDSPSharp.Core.SafeHandlers;
using System;

namespace SpeexDSPSharp.Core
{
    public class SpeexPreprocessor
    {
        private readonly SpeexPreprocessStateSafeHandler _handler;
        private bool _disposed;

        public SpeexPreprocessor(int frame_size, int sample_rate)
        {
            _handler = NativeSpeexDSP.speex_preprocess_state_init(frame_size, sample_rate);
        }

        ~SpeexPreprocessor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public unsafe int Run(short[] x)
        {
            ThrowIfDisposed();
            fixed (short* xPtr = x)
            {
                var result = NativeSpeexDSP.speex_preprocess_run(_handler, xPtr);
                CheckError(result);
                return result;
            }
        }

        public unsafe void EstimateUpdate(short[] x)
        {
            ThrowIfDisposed();
            fixed (short* xPtr = x)
            {
                NativeSpeexDSP.speex_preprocess_estimate_update(_handler, xPtr);
            }
        }

        public unsafe int Ctl<T>(EchoCancellationCtl request, ref T value) where T : unmanaged
        {
            ThrowIfDisposed();
            fixed (void* valuePtr = &value)
            {
                var result = NativeSpeexDSP.speex_preprocess_ctl(_handler, (int)request, valuePtr);
                CheckError(result);
                return result;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (!_handler.IsClosed)
                    _handler.Close();
            }

            _disposed = true;
        }

        protected virtual void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected static void CheckError(int result)
        {
            if (result < 0)
                throw new SpeexDSPException(result.ToString());
        }
    }
}
