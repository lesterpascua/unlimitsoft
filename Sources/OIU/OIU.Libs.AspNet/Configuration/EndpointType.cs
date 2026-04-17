namespace OIU.Libs.AspNet.Configuration;


/// <summary>
/// Indicate the type of endpoint registration to do.
/// </summary>
public enum EndpointType
{
    /// <summary>
    /// Register the endpoints as controllers, using the standard routing and configuration for controllers.
    /// </summary>
    Controller = 1,
    /// <summary>
    /// Indicates that data flows from the view to the controller.
    /// </summary>
    ViewToController = 2,
    /// <summary>
    /// Indicates that only endpoints are registered, without using the standard controller-based routing. This option is typically used when you want to define custom routes or use a different approach for handling requests, such as minimal APIs or custom middleware.
    /// </summary>
    OnlyEndpoints = 3
}