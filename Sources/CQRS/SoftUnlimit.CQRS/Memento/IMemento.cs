using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SoftUnlimit.CQRS.Memento
{
    public class Memento<TEntity> 
        where TEntity : class, IEventSourced
    {
        private readonly IEventSourcedRepository<TEntity> _eventSourcedRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventSourcedRepository"></param>
        public Memento(IEventSourcedRepository<TEntity> eventSourcedRepository)
        {
            this._eventSourcedRepository = eventSourcedRepository;
        }

        public GetMemento(Guid id, long version, CancellationToken ct)
        {
            _eventSourcedRepository.FindByIdAsync(id, version, ct);
        }
    }
}
