using System;
using System.Runtime.InteropServices;

namespace SpeexDSPSharp.Core.SafeHandlers
{
    public class SpeedxDSPResamplerSafeHandler : SafeHandle
    {
        public SpeedxDSPResamplerSafeHandler() : base(IntPtr.Zero, true)
        {

        }

        /// <inheritdoc/>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            NativeSpeexDSP.speex_resampler_destroy(handle);
            return true;
        }
    }
}