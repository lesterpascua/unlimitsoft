using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Web.Model;

namespace OIU.Libs.Domain;


/// <summary>
/// 
/// </summary>
public class OIUQueryProps : QueryProps, IProps
{
    /// <inheritdoc />
    public Guid IdentityId { get; set; }
    /// <inheritdoc />
    public string? CorrelationId { get; set; }
}
/// <summary>
/// 
/// </summary>
public abstract class OIUQuery<TResult> : Query<TResult, OIUQueryProps>
{
    /// <summary>
    /// 
    /// </summary>
    protected OIUQuery() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="identityId"></param>
    /// <param name="correlationId"></param>
    protected OIUQuery(Guid identityId, string? correlationId)
    {
        Props = new OIUQueryProps
        {
            IdentityId = identityId,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Default paging for queries.
    /// </summary>
    public static readonly Paging DefaultPaging = new() { Page = 0, PageSize = 20 };
    /// <summary>
    /// Default sorting for queries.
    /// </summary>
    public static readonly ColumnName[] DefaultOrder = [new ColumnName { Asc = true, Name = "Id" }];
}
