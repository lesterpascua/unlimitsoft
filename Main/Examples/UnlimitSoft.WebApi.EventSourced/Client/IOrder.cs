namespace UnlimitSoft.WebApi.EventSourced.Client;


/// <summary>
/// 
/// </summary>
public interface IOrder
{
    /// <summary>
    /// 
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Name of the item
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Order creation date.
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// Last time when the order was updated
    /// </summary>
    public int Amount { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public long Version { get; set; }
}