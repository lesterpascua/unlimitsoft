using System;

namespace SoftUnlimit.Logger
{
    /// <summary>
    /// Get some trace information for specific context
    /// </summary>
    public interface ICorrelationContext
    {
        /// <summary>
        /// Correlation identifier. In contract with trace id correlation is the id of the entire flow, so in you want keep the same 
        /// correlation for a flow use x-correlarion-id header in all request.
        /// </summary>
        string? CorrelationId { get; }
        /// <summary>
        /// Set correlation data. If is already assign throw exception.
        /// </summary>
        /// <param name="correlationId"></param>
        void SetCorrelationId(string correlationId);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class DefaultCorrelationContext : ICorrelationContext
    {
        /// <inheritdoc />
        public string? CorrelationId { get; private set; }

        /// <inheritdoc />
        public void SetCorrelationId(string correlationId)
        {
            if (CorrelationId != null)
                throw new InvalidOperationException("Invalid assing correlation id multiples time.");

            CorrelationId = correlationId;
        }
    }
}
