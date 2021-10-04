namespace SoftUnlimit.Security
{
    /// <summary>
    /// Allow get the service and worker identifier in the current service.
    /// </summary>
    public interface IServiceMetadata
    {
        /// <summary>
        /// Service identifier.
        /// </summary>
        ushort ServiceId { get; }
        /// <summary>
        /// Worker identifier.
        /// </summary>
        string WorkerId { get; }
    }
}
