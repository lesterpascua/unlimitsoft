using System;

namespace UnlimitSoft.MultiTenant;

/// <summary>
/// Base class for tenant information.
/// </summary>
/// <param name="Id">The tenant Id</param>
/// <param name="Key">The tenant identifier. Use this identifier to resolve the tenant</param>
public record Tenant(Guid Id, string? Key);