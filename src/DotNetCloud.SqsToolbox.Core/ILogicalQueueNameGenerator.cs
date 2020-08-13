using System;
using System.Buffers;

namespace DotNetCloud.SqsToolbox.Core
{
    /// <summary>
    /// Can be used to generate a logical name for a queue based on properties of the queue URL.
    /// </summary>
    public interface ILogicalQueueNameGenerator
    {
        /// <summary>
        /// Generate a logical name for the queue reader and channel registrations.
        /// </summary>
        /// <param name="queueUrl">The AWS queue URL.</param>
        /// <returns>A logical name for the queue.</returns>
        public string GenerateName(string queueUrl);
    }

    internal sealed class DefaultLogicalQueueNameGenerator : ILogicalQueueNameGenerator
    {
        public string GenerateName(string queueUrl)
        {
            if (!Uri.TryCreate(queueUrl, UriKind.Absolute, out var uri))
                throw new InvalidOperationException("The queue URL was not valid");

            var hostSpan = uri.Host.AsSpan();
            var pathSpan = uri.LocalPath.AsSpan();

            // todo - handle localstack
            // todo - handle a URL which is not a queue URL (i.e. no path)

            var nameLength = pathSpan.Length - pathSpan.LastIndexOf('/') - 1;
            var hostStart = hostSpan.IndexOf('.') + 1;
            var hostLength = hostSpan.Slice(hostStart).IndexOf('.');

            var totalLength = nameLength + hostLength + 1;

            if (totalLength <= 64)
            {
                Span<char> nameChars = stackalloc char[totalLength];

                hostSpan.Slice(hostStart, hostLength).CopyTo(nameChars);
                nameChars[hostLength] = '_';
                pathSpan.Slice(pathSpan.LastIndexOf('/') + 1, nameLength).CopyTo(nameChars.Slice(hostLength + 1));

                return nameChars.ToString();
            }
            else
            {
                var nameChars = ArrayPool<char>.Shared.Rent(totalLength);
                var nameCharsSpan = nameChars.AsSpan();

                try
                {
                    hostSpan.Slice(hostStart, hostLength).CopyTo(nameCharsSpan);
                    nameChars[hostLength] = '_';
                    pathSpan.Slice(pathSpan.LastIndexOf('/') + 1, nameLength).CopyTo(nameCharsSpan.Slice(hostLength + 1));

                    return nameChars.ToString();
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(nameChars);
                }
            }
        }
    }
}
