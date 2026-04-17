namespace OIU.Libs.AspNet.Configuration;


/// <summary>
/// Swagger configuration
/// </summary>
public sealed class DocumentationOptions
{
    /// <summary>
    /// Disable swagger
    /// </summary>
    public bool Disable { get; set; }
    /// <summary>
    /// Prefix used to generate the documentation
    /// </summary>
    public string? Prefix { get; set; }
}
