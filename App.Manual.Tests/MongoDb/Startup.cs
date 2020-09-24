using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.MongoDb
{
    /// <summary>
    /// Busines logic entry point
    /// </summary>
    public class Startup
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Person> _repository;
        private readonly IQueryRepository<Person> _queryRepository;

        public Startup(IUnitOfWork unitOfWork, IRepository<Person> repository, IQueryRepository<Person> queryRepository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            this._queryRepository = queryRepository;
        }

        public async Task Run()
        {
            // Find all element before remove.
            var after = await ((IMongoQueryable<Person>)_queryRepository.FindAll().Where(p => p.Name != null)).ToCursorAsync();
            after.Dispose();

            // Iterate collection and remove all value from repository.
            var all = _repository.FindAll().ToArray();
            //foreach (var entry in all)
            //    _repository.Remove(entry);

            // Persist changes
            await _unitOfWork.SaveChangesAsync();

            var dbJob = new Person {
                Id = Guid.NewGuid(),
                Name = "Some Test Name"
            };
            await _repository.AddAsync(dbJob);
            await _unitOfWork.SaveChangesAsync();

            dbJob.Name = "New Name";

            _repository.Update(dbJob);
            await _unitOfWork.SaveChangesAsync();

            await Task.CompletedTask;
        }
    }
}
