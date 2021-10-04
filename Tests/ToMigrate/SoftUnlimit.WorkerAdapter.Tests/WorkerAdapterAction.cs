using Moq;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace SoftUnlimit.WorkerAdapter.Tests
{
    public class WorkerAdapterAction
    {
        [Fact]
        public void Add_New_Worker()
        {
            bool save = false;
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(() => {
                    save = true;
                    return Task.FromResult(1);
                });

            List<WorkerStorage> workers = new List<WorkerStorage>();
            var fakeRepository = new Mock<IRepository<WorkerStorage>>();
            fakeRepository.Setup(x => x.AddAsync(It.IsAny<WorkerStorage>(), It.IsAny<CancellationToken>()))
                .Returns((WorkerStorage e, CancellationToken ct) => {
                    workers.Add(e);
                    return Task.FromResult(EntityState.Added);
                });

            IWorkerIDAdapter adapter = new UnitOfWrokWorkerIDAdapter<IUnitOfWork, IRepository<WorkerStorage>, WorkerStorage>(
                unitOfWorkMock.Object,
                fakeRepository.Object,
                new DefaultThreadSafeCache()
            );

            const uint serviceId = 10;
            const string identifier = "123456";
            const string healthEndpoint = "http://localhost/hc";

            DateTime now = DateTime.UtcNow;

            adapter.ReserveAsync(serviceId, identifier, healthEndpoint);

            Assert.True(save);
            Assert.Single(workers);
            Assert.Equal(serviceId, workers[0].ServiceId);
            Assert.Equal(identifier, workers[0].Identifier);
            Assert.Equal(healthEndpoint, workers[0].Endpoint);
            Assert.True(now <= workers[0].Created && workers[0].Created <= DateTime.UtcNow);
            Assert.True(now <= workers[0].Updated && workers[0].Updated <= DateTime.UtcNow);
        }
    }
}
