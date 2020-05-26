using System;
using System.Collections.Generic;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Core.Abstractions
{
    /// <summary>
    /// Calculates the next delay for a polling receive message attempt.
    /// </summary>
    public interface ISqsReceiveDelayCalculator
    {
        /// <summary>
        /// Calculates a delay between the previous and next polling receive attempt.
        /// </summary>
        /// <param name="messages">The <see cref="IEnumerable{T}"/> of <see cref="Message"/> from the last receive attempt.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the delay to apply before the next polling attempt.</returns>
        TimeSpan CalculateSecondsToDelay(IEnumerable<Message> messages);
    }
}
