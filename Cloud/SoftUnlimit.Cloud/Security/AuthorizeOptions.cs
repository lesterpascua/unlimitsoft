namespace SoftUnlimit.Cloud.Security
{
    public class AuthorizeOptions
    {
        /// <summary>
        /// Api key used for authentication
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// User information for all internal service call.
        /// </summary>
        public IdentityInfo User { get; set; }
    }
}
