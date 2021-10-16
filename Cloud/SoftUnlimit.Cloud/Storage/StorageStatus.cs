namespace SoftUnlimit.Cloud.Storage
{
    public enum StorageStatus
    {
        /// <summary>
        /// Operation sucess
        /// </summary>
        Success = 1,
        /// <summary>
        /// File not found
        /// </summary>
        NotFound = 2,
        /// <summary>
        /// Generic error
        /// </summary>
        Error = 3,
        /// <summary>
        /// Service offline.
        /// </summary>
        ServiceOffline = 4,
    }
}
