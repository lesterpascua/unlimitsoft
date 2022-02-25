using SoftUnlimit.Logger;

namespace UnlimitSoft.WebApi.Logger.Logger
{
    public class MyLoggerContext : LoggerContext
    {
        /// <summary>
        /// Identity owner of the operation
        /// </summary>
        public string? IdentityId { get; set; }
    }
}
