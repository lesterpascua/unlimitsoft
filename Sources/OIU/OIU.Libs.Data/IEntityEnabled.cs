using UnlimitSoft.Data;

namespace OIU.Libs.Data;


/// <summary>
/// 
/// </summary>
public interface IEntityEnabled : IEntity
{
    /// <summary>
    /// Customer is ready to use in the service
    /// </summary>
    public bool Enabled { get; set; }
}
