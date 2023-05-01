namespace UnlimitSoft.Message;


/// <summary>
/// Base interface for all request
/// </summary>
public interface IRequest { }
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IRequest<out TResponse> : IRequest
{
}