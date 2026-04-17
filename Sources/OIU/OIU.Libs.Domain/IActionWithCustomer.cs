namespace OIU.Libs.Domain;


/// <summary>
/// Indicate the command or query is asociate to a customer.
/// </summary>
public interface IActionWithCustomer
{
    /// <summary>
    /// The command has an argument indicate the customer owner of the command.
    /// </summary>
    Guid CustomerId { get; }
}