using SpeexDSPSharp.Core.SafeHandlers;
using System;

namespace SpeexDSPSharp.Core
{
    public class SpeexDSPResampler : IDisposable
    {
       protected readonly SpeedxDSPResamplerSafeHandler _handler;
        private bool _disposed;
        private readonly int _channels;
        
        public SpeexDSPResampler(int channels, int inputRate, int outputRate, int quality)
        {
            if (channels <= 0)
                throw new ArgumentOutOfRangeException(nameof(channels), "Channels must be greater than zero");
            
            if (inputRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(inputRate), "Input rate must be greater than zero");
            
            if (outputRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(outputRate), "Output rate must be greater than zero");
            
            if (quality < 0 || quality > 10)
                throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 and 10");

            _channels = channels;
            
            unsafe
            {
                int error = 0;
                _handler = NativeSpeexDSP.speex_resampler_init(channels, inputRate, outputRate, quality, &error);
                
                if (error != 0)
                    throw new SpeexDSPException($"Failed to initialize resampler with error code: {error}");
            }
        }

        /// <summary>
        /// Speexdsp resampler destructor.
        /// </summary>
        ~SpeexDSPResampler()
        {
            Dispose(false);
        }

        /// <summary>
        /// Resamples audio data for a single channel using integer representation.
        /// </summary>
        /// <param name="channelIndex">Index of the channel to process (0 for mono, 0-n for multi-channel).</param>
        /// <param name="input">Input audio buffer.</param>
        /// <param name="output">Output audio buffer.</param>
        /// <returns>Tuple containing (input samples consumed, output samples produced)</returns>
        public unsafe (int inputUsed, int outputGenerated) ProcessInt(int channelIndex, Span<short> input, Span<short> output)
        {
            ThrowIfDisposed();
            
            if (channelIndex < 0 || channelIndex >= _channels)
                throw new ArgumentOutOfRangeException(nameof(channelIndex), $"Channel index must be between 0 and {_channels - 1}");

            int inLen = input.Length;
            int outLen = output.Length;

            fixed (short* inPtr = input)
            fixed (short* outPtr = output)
            {
                int result = NativeSpeexDSP.speex_resampler_process_int(
                    _handler, 
                    channelIndex, 
                    inPtr, 
                    &inLen, 
                    outPtr, 
                    &outLen);
                
                CheckError(result);
                return (inLen, outLen);
            }
        }
        
        /// <summary>
        /// Resamples audio data for a single channel using integer representation.
        /// </summary>
        /// <param name="channelIndex">Index of the channel to process (0 for mono, 0-n for multi-channel).</param>
        /// <param name="input">Input audio buffer.</param>
        /// <param name="output">Output audio buffer.</param>
        /// <returns>Tuple containing (input samples consumed, output samples produced)</returns>
        public (int inputUsed, int outputGenerated) ProcessInt(int channelIndex, short[] input, short[] output)
        {
            return ProcessInt(channelIndex, input.AsSpan(), output.AsSpan());
        }
        
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose logic.
        /// </summary>
        /// <param name="disposing">Set to true if fully disposing.</param>
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

        /// <summary>
        /// Throws an exception if this object is disposed or the handler is closed.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        protected virtual void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Checks if there is a speexdsp error and throws if the error is a negative value.
        /// </summary>
        /// <param name="error">The error code to input.</param>
        /// <exception cref="SpeexDSPException"></exception>
        protected static void CheckError(int error)
        {
            if (error < 0)
                throw new SpeexDSPException($"Resampler error: {error}");
        }
    }
}