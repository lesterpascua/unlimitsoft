using UnlimitSoft.Web.Model;

namespace OIU.Libs;



/// <summary>
/// 
/// </summary>
public static class Constants
{
    /// <summary>
    /// Identity used to run an operation
    /// </summary>
    public const string HeaderIdentity = "X-Identity";
    /// <summary>
    /// Identity used to run an operation
    /// </summary>
    public const string LogContextIdentity = "IdentityId";

    /// <summary>
    /// 
    /// </summary>
    public const string AuthorizationHeader = "Authorization";
    /// <summary>
    /// Default console template
    /// </summary>
    public const string OutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}";

    /// <summary>
    /// Allow define mimetypes used in the services
    /// </summary>
    public static class MimeType
    {
        /// <summary>
        /// 
        /// </summary>
        public const string Pdf = "application/pdf";
        /// <summary>
        /// 
        /// </summary>
        public const string Json = "application/json";
    }
    /// <summary>
    /// Lock constants
    /// </summary>
    public static class Lock
    {
        /// <summary>
        /// Master lock object
        /// </summary>
#if NET9_0_OR_GREATER
        public static readonly System.Threading.Lock Instance = new();
#else
        public static readonly object Instance = new();
#endif

        /// <summary>
        /// Distribute lock timeout
        /// </summary>
        public static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(10);
        /// <summary>
        /// Long Distribute lock timeout
        /// </summary>
        public static readonly TimeSpan LongTimeOut = TimeSpan.FromSeconds(30);
    }
    /// <summary>
    /// 
    /// </summary>
    public static class Search
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly ColumnName[] Oders = [new ColumnName { Name = "Id", Asc = true }];
    }
    /// <summary>
    /// 
    /// </summary>
    public static class Security
    {
        /// <summary>
        /// Roles availables.
        /// </summary>
        public static class Role
        {
            /// <summary>
            /// 
            /// </summary>
            public static class Anonymous
            {
                /// <summary>
                /// System identitifier
                /// </summary>
                public static readonly Guid IdentityId = Guid.Empty;
                /// <summary>
                /// Internal for the anonymous.
                /// </summary>
                public const string Default = "";
            }
        }
        /// <summary>
        /// Scopes availables in the system
        /// </summary>
        public static class Scope
        {
            /// <summary>
            /// 
            /// </summary>
            public static class System
            {
                /// <summary>
                /// System identitifier
                /// </summary>
                public static readonly Guid IdentityId = new("00000000-0000-0000-0000-000000000001");
                /// <summary>
                /// Internal for the system.
                /// </summary>
                public const string Default = "system";
            }
        }
    }
    /// <summary>
    /// Diferent versions used in the API
    /// </summary>
    public static class Api
    {
        /// <summary>
        /// 
        /// </summary>
        public const string V1 = "v1";
        /// <summary>
        /// Endpoint to be consumed from the admin users
        /// </summary>
        public const string Admin = "admin";
    }
}
