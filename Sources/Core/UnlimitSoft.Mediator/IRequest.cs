namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public interface IRequest { }
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IRequest<out TResponse> : IRequest
{
}