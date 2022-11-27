using UnlimitSoft.CQRS.Command;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


/// <summary>
/// Used to identified command handler for this service.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IMyCommandHandler<in TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
     where TCommand : MyCommand<TResponse>
{
}
