using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OIU.Libs.Logger;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace OIU.Libs.Data;


/// <summary>
/// If some entity implement this interface indicate there is a background job asociate to it.
/// </summary>
public interface IEntityWithJob
{
    /// <summary>
    /// Current job executed by the entity. Null if no job executed.
    /// </summary>
    Guid? CurrJobId { get; set; }
    /// <summary>
    /// Last job executed by in the entity. Null if never executed a job.
    /// </summary>
    Guid? LastJobId { get; set; }
    /// <summary>
    /// Command bus generate job identifier.
    /// </summary>
    string? BusJobId { get; set; }
}
/// <summary>
/// 
/// </summary>
public static class IEntityWithJobExtensions
{
    extension(IEntityWithJob entity)
    {
        /// <summary>
        /// Stop the current job of the entity and move the job id to last job id, then return the current job id. If there is no job running for the entity, throw an exception.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Guid Stop()
        {
            var currJobId = entity.CurrJobId ?? throw new InvalidOperationException("There is no job running for this entity");
            entity.CurrJobId = null;
            entity.LastJobId = currJobId;

            return currJobId;
        }
        /// <summary>
        /// Allow start new job if there is no job running for the entity, otherwise return bad response with error message
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public IResponse Start<TCommand>(TCommand command, Guid jobId) where TCommand : ICommand
        {
            if (entity.CurrJobId.HasValue)
                return command.BadResponse(string.Empty, $"{WKE.AlreadyExistAJobOverThisOperation:D}:{entity.CurrJobId}");

            entity.CurrJobId = jobId;

            return command.OkResponse();
        }
    }
    extension<T>(EntityTypeBuilder<T> @this) where T : class, IEntityWithJob
    {
        /// <summary>
        /// Configue the entity type builder with the properties of the job. This method should be called in the entity type builder of the entities that implement the IEntityWithJob interface.
        /// </summary>
        public void ConfigureJobEntityTypeBuilder()
        {
            @this.Property(p => p.BusJobId).HasColumnOrder(500).HasMaxLength(15).HasComment("Indicate the id of the job in the CommandBus register");
            @this.Property(p => p.CurrJobId).HasColumnOrder(501).HasComment("Nexu job identifier for the action in progress");
            @this.Property(p => p.LastJobId).HasColumnOrder(502).HasComment("Nexu last job identifier for an action");
        }
    }
}