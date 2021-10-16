namespace SoftUnlimit.Cloud.Storage
{
    public enum StorageType
    {
        /// <summary>
        /// Container for files pending to scan for virus
        /// </summary>
        Pending = 1,
        /// <summary>
        /// Container for files without virus
        /// </summary>
        Clean = 2
    }
}
