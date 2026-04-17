namespace OIU.Libs.Configuration;


/// <summary>
/// Services configuration parameters.
/// </summary>
public abstract class ServiceOptions
{
    /// <summary>
    /// Environment name where the system is running
    /// </summary>
    public string Env { get; set; } = default!;
    /// <summary>
    /// Name of the Region where the service is run (this will be use to identifier closer service to use and skip de api manager.
    /// </summary>
    /// <remarks>
    /// The section ApiService allow set the url of the diferent api according to the region. This allow use the closer service depending of the 
    /// region where the application is executed. In case of the service of OneJN we can call direct the endpoint of the service in the same cluster 
    /// instead of the externals. If don't want use this the name will require configure used dns geo-location to get the optimal performance.
    /// </remarks>
    /// <example>
    /// Region: EastUS (indicate the cluster is running in East US
    /// Region: EastUS1  (indicate the cluster is running in East US and is the 1 in case of exist multiples cluster in the same region)
    /// Region: EastUS2  (indicate the cluster is running in East US and is the 2 in case of exist multiples cluster in the same region)
    /// </example>
    public string? Region { get; set; }
    /// <summary>
    /// Maximun amount of day to store the history of versioned event. System will remove all older event already published.
    /// </summary>
    public int SaveEventHistory { get; set; }

    /// <summary>
    /// Manual set WorkerId
    /// </summary>
    public string? WorkerId { get; set; }
    /// <summary>
    /// The service is a slave service. By defaul all service are master so if you want specified a slave servide 
    /// you need to do it explicity.
    /// </summary>
    public bool IsSlave { get; set; }
}