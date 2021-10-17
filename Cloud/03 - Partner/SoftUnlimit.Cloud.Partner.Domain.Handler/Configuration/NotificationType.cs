namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration
{
    /// <summary>
    /// Alowed notification types.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Azure event bus.
        /// </summary>
        AzureEventBus = 1,
        WebHub = 2,
        Rest = 3,
        SalesforcePlatformEvent = 4
    }
}
