﻿using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    public struct FileCopyProgressInfo
    {
        public long BytesRead { get; set; }

        public long TotalBytesCopied { get; set; }

        public long SourceLength { get; set; }
    }

    /// <summary>
    /// Provides CopyToAsync method for operating on <see cref="Stream"/> instances.
    /// </summary>
	public static class StreamExtensions
    {
        private const int DefaultBufferSize = 81920;

        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified buffer size and cancellation token.
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/> to copy from.</param>
        /// <param name="sourceLength">The length of the source stream, if known - used for progress reporting.</param>
        /// <param name="destination">The <see cref="Stream"/> to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 81920.</param>
        /// <param name="progress">An async function for reporting progress.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task representing the operation</returns>
        public static async Task CopyToAsync(
            this Stream source,
            long sourceLength,
            Stream destination,
            int bufferSize,
            IProgress<FileCopyProgressInfo> progress,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, $"{nameof(bufferSize)} has to be greater than zero");
            }

            if (!source.CanRead)
            {
                throw new ArgumentException($"{nameof(source)} is not readable.", nameof(source));
            }

            if (!destination.CanWrite)
            {
                throw new ArgumentException($"{nameof(destination)} is not writable.", nameof(source));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sourceLength <= 0 && source.CanSeek)
            {
                sourceLength = source.Length - source.Position;
            }

            var totalBytesCopied = 0L;
            int bytesRead;
            var buffer = new byte[bufferSize];

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                if (bytesRead == 0 || cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

                totalBytesCopied += bytesRead;

                //await progress(new FileCopyProgressInfo { BytesRead = bytesRead, TotalBytesCopied = totalBytesCopied, SourceLength = sourceLength });

                progress.Report(new FileCopyProgressInfo { BytesRead = bytesRead, TotalBytesCopied = totalBytesCopied, SourceLength = sourceLength });
            }
        }

        /// <summary>
        /// Copies a stream to another stream
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/> to copy from.</param>
        /// <param name="sourceLength">The length of the source stream, if known - used for progress reporting.</param>
        /// <param name="destination">The <see cref="Stream"/> to which the contents of the current stream will be copied.</param>
        /// <param name="progress">An async function for reporting progress.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task representing the operation</returns>
        public static Task CopyToAsync(
            this Stream source,
            long sourceLength,
            Stream destination,
            IProgress<FileCopyProgressInfo> progress,
            CancellationToken cancellationToken = default
        ) => CopyToAsync(source, sourceLength, destination, DefaultBufferSize, progress, cancellationToken);

        /// <summary>
        /// Copies a stream to another stream
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/> to copy from</param>
        /// <param name="destination">The <see cref="Stream"/> to which the contents of the current stream will be copied.</param>
        /// <param name="progress">An async function for reporting progress.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        public static Task CopyToAsync(
            this Stream source,
            Stream destination,
            IProgress<FileCopyProgressInfo> progress,
            CancellationToken cancellationToken = default
        ) => CopyToAsync(source, 0L, destination, DefaultBufferSize, progress, cancellationToken);


        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified buffer size and cancellation token.
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/> to copy from.</param>
        /// <param name="sourceLength">The length of the source stream, if known - used for progress reporting.</param>
        /// <param name="destination">The <see cref="Stream"/> to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 81920.</param>
        /// <param name="progress">An async function for reporting progress.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task representing the operation</returns>
        public static async Task CopyToAsync(
            this Stream source,
            long sourceLength,
            Stream destination,
            int bufferSize,
            Func<FileCopyProgressInfo, Task> progress,
            //IProgress<FileCopyProgressInfo> progress,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, $"{nameof(bufferSize)} has to be greater than zero");
            }

            if (!source.CanRead)
            {
                throw new ArgumentException($"{nameof(source)} is not readable.", nameof(source));
            }

            if (!destination.CanWrite)
            {
                throw new ArgumentException($"{nameof(destination)} is not writable.", nameof(source));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sourceLength <= 0 && source.CanSeek)
            {
                sourceLength = source.Length - source.Position;
            }

            var totalBytesCopied = 0L;
            int bytesRead;
            var buffer = new byte[bufferSize];

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                if (bytesRead == 0 || cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

                totalBytesCopied += bytesRead;

                await progress(new FileCopyProgressInfo { BytesRead = bytesRead, TotalBytesCopied = totalBytesCopied, SourceLength = sourceLength });

                //progress.Report(new FileCopyProgressInfo { BytesRead = bytesRead, TotalBytesCopied = totalBytesCopied, SourceLength = sourceLength });
            }
        }

        /// <summary>
        /// Copies a stream to another stream
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/> to copy from.</param>
        /// <param name="sourceLength">The length of the source stream, if known - used for progress reporting.</param>
        /// <param name="destination">The <see cref="Stream"/> to which the contents of the current stream will be copied.</param>
        /// <param name="progress">An async function for reporting progress.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task representing the operation</returns>
        public static Task CopyToAsync(
            this Stream source,
            long sourceLength,
            Stream destination,
            //IProgress<FileCopyProgressInfo> progress,
            Func<FileCopyProgressInfo, Task> progress,
            CancellationToken cancellationToken = default
        ) => CopyToAsync(source, sourceLength, destination, DefaultBufferSize, progress, cancellationToken);

        /// <summary>
        /// Copies a stream to another stream
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/> to copy from</param>
        /// <param name="destination">The <see cref="Stream"/> to which the contents of the current stream will be copied.</param>
        /// <param name="progress">An async function for reporting progress.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        public static Task CopyToAsync(
            this Stream source,
            Stream destination,
            //IProgress<FileCopyProgressInfo> progress,
            Func<FileCopyProgressInfo, Task> progress,
            CancellationToken cancellationToken = default
        ) => CopyToAsync(source, 0L, destination, DefaultBufferSize, progress, cancellationToken);
    }
}