namespace SoftUnlimit.Logger
{
    /// <summary>
    /// Context of the logging
    /// </summary>
    public class LoggerContext
    {
        /// <summary>
        /// Id of the operation
        /// </summary>
        public string? TraceId { get; set; }
        /// <summary>
        /// Id of the correlation of the operation
        /// </summary>
        public string? CorrelationId { get; set; }
    }
}