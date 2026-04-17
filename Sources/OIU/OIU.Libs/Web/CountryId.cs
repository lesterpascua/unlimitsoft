using StronglyTypedIds;

namespace OIU.Libs.Web;


/// <summary>
/// Country identifier.
/// </summary>
[StronglyTypedId(Template.Int)]
public readonly partial struct CountryId
{
    /// <summary>
    /// Jamaica country id
    /// </summary>
    public const int JM = 1;
    /// <summary>
    /// United state country id
    /// </summary>
    public const int US = 2;
    /// <summary>
    /// Spain country id
    /// </summary>
    public const int ES = 4;
    /// <summary>
    /// United Kindow country id
    /// </summary>
    public const int GB = 3;
}