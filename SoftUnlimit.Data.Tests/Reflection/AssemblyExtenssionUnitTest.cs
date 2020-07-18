using Moq;
using SoftUnlimit.Data.Reflection;
using SoftUnlimit.Data.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.Data.Reflection.Tests
{
    [Collection(nameof(SetupTestEnv))]
    public class AssemblyExtenssionUnitTest : IClassFixture<AssemblyExtenssionUnitTest.CommonFeature>
    {
        private readonly SetupTestEnv _setup;
        private readonly CommonFeature _feature;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setup"></param>
        public AssemblyExtenssionUnitTest(SetupTestEnv setup, CommonFeature feature)
        {
            this._setup = setup;
            this._feature = feature;
        }


        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void FindAllRepositories_EmptyAssembly_Expected_EmptyResult()
        {
            var assembly = this._feature.FakeEmptyAssembly();
            var result = assembly.FindAllRepositories(
                typeof(TestEntityTypeBuilder<>),
                typeof(IRepository<>),
                typeof(IQueryRepository<>),
                typeof(Repository<>),
                typeof(QueryRepository<>),
                (Type entity) => entity.GetInterfaces().Any(p => p == typeof(IEntity))
            );

            Assert.Empty(result);
        }
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void FindAllRepositories_MathAssembly_Expected_OneResult()
        {
            var assembly = this._feature.FakeAssemblyWithMatchesType();
            var result = assembly.FindAllRepositories(
                typeof(TestEntityTypeBuilder<>),
                typeof(IRepository<>),
                typeof(IQueryRepository<>),
                typeof(Repository<>),
                typeof(QueryRepository<>),
                (Type entity) => true
            );

            Assert.Single(result);
        }


        #region Nested Classes

        public class CommonFeature : IAsyncLifetime
        {
            public Task DisposeAsync() => Task.CompletedTask;
            public Task InitializeAsync() => Task.CompletedTask;

            /// <summary>
            /// Return fake assembly without types.
            /// </summary>
            /// <returns></returns>
            public Assembly FakeEmptyAssembly()
            {
                var fake = new Mock<Assembly>();
                fake.Setup(x => x.GetTypes()).Returns(new Type[0]);

                return fake.Object;
            }
            /// <summary>
            /// Return fake assembly without types.
            /// </summary>
            /// <returns></returns>
            public Assembly FakeAssemblyWithMatchesType()
            {
                var fake = new Mock<Assembly>();
                fake.Setup(x => x.GetTypes()).Returns(new Type[] {
                    typeof(Entity01TestEntityTypeBuilder)
                });

                return fake.Object;
            }
        }


        /// <summary>
        /// Base class for type mapping
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        public abstract class TestEntityTypeBuilder<TEntity> where TEntity : class
        {
        }
        public class Entity01TestEntityTypeBuilder : TestEntityTypeBuilder<object>
        {
        }
        public class QueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
        {
            public IQueryable<TEntity> FindAll() => throw new NotImplementedException();
            public ValueTask<TEntity> FindAsync(params object[] keyValues) => throw new NotImplementedException();
        }
        public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
        {
            public EntityState Add(TEntity entity) => throw new NotImplementedException();
            public Task<EntityState> AddAsync(TEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public void AddRange(params TEntity[] entities) => throw new NotImplementedException();
            public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IQueryable<TEntity> FindAll() => throw new NotImplementedException();
            public ValueTask<TEntity> FindAsync(params object[] keyValues) => throw new NotImplementedException();
            public EntityState Remove(TEntity entity) => throw new NotImplementedException();
            public EntityState Update(TEntity entity) => throw new NotImplementedException();
            public void UpdateRange(params TEntity[] entities) => throw new NotImplementedException();
        }

        #endregion
    }
}
