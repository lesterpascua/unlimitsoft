using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data.EntityFramework;

namespace SoftUnlimit.WebApi.Sources.Data
{
    public class MyUnitOfWork : EFCQRSDbUnitOfWork<DbContextWrite>, IMyUnitOfWork
    {
        #region Ctor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="eventSourcedMediator"></param>
        public MyUnitOfWork(DbContextWrite dbContext, IMediatorDispatchEventSourced eventSourcedMediator)
            : base(dbContext, null, eventSourcedMediator)
        {
        }
        #endregion
    }
}
