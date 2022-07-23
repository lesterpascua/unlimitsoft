using System;
using System.Linq;

namespace UnlimitSoft.Web.Security
{
    /// <summary>
    /// Short information about the customer, use in commands and queries
    /// </summary>
    public class IdentityInfo
    {
        private string[] _roles, _scopes;

        /// <summary>
        /// 
        /// </summary>
        public IdentityInfo() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <param name="scope"></param>
        /// <param name="traceId"></param>
        /// <param name="correlationId"></param>
        public IdentityInfo(Guid id, string role = null, string scope = null, string traceId = null, string correlationId = null) => (Id, Role, Scope, TraceId, CorrelationId) = (id, role, scope, traceId, correlationId);

        /// <summary>
        /// User identifier
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Roles separate by coma of the identity
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Stope separate by coma of the identitty
        /// </summary>
        public string Scope { get; set; }
        /// <summary>
        /// Operation trace identifier.
        /// </summary>
        public string TraceId { get; set; }
        /// <summary>
        /// Correlation trace identifier.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] GetRoles() => _roles ??= Role?.Split(',');
        /// <summary>
        /// 
        /// </summary>
        public string[] GetScopes() => _scopes ??= Scope?.Split(',');
        /// <summary>
        /// Check if the identity is in some role.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsInRole(string role) => GetRoles()?.Contains(role) ?? false;
        /// <summary>
        /// Check if the identity is in some scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public bool IsInScope(string scope) => GetScopes()?.Contains(scope) ?? false;
    }
}
