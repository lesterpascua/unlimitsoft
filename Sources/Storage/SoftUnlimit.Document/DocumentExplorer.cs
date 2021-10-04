using Microsoft.EntityFrameworkCore;
using MimeMapping;
using SoftUnlimit.Data;
using SoftUnlimit.Document.Data;
using SoftUnlimit.Document.Data.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Document
{
    /// <summary>
    /// Manager for Cloud document.
    /// </summary>
    public class RepositoryDocumentExplorer : IDocumentExplorer
    {
        #region Fields

        private readonly Dictionary<Guid, IDocumentProvider> _providers;
        private readonly ISelectionAlgorithm _selectAlgorithm;
        private readonly Type _metadataType;
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly Func<IRepository<Metadata>> _repositoryFactory;
        private readonly Func<IQueryRepository<Metadata>> _queryRepositoryFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providers"></param>
        /// <param name="selectAlgorithm"></param>
        /// <param name="metadataType"></param>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="repositoryFactory"></param>
        /// <param name="queryRepositoryFactory"></param>
        public RepositoryDocumentExplorer(IEnumerable<IDocumentProvider> providers, ISelectionAlgorithm selectAlgorithm, Type metadataType,
            Func<IUnitOfWork> unitOfWorkFactory, Func<IRepository<Metadata>> repositoryFactory, Func<IQueryRepository<Metadata>> queryRepositoryFactory)
        {
            if (metadataType != typeof(Metadata) && !metadataType.IsSubclassOf(typeof(Metadata)))
                throw new InvalidOperationException("metadataType must me sub type or type of metadata");
            this._selectAlgorithm = selectAlgorithm;
            this._metadataType = metadataType;
            this._unitOfWorkFactory = unitOfWorkFactory;
            this._repositoryFactory = repositoryFactory;
            this._queryRepositoryFactory = queryRepositoryFactory;
            this._providers = providers.ToDictionary(k => k.Identifier, v => v);
        }

        #endregion

        #region Public Method

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public virtual async Task<string[]> GetFolders(string folder)
        {
            var queryRepository = this._queryRepositoryFactory();

            if (!folder.EndsWith(Path.DirectorySeparatorChar))
                folder += Path.DirectorySeparatorChar;

            var query = queryRepository.Find(p => p.Folder.StartsWith(folder));
            return await query.Select(s => s.Folder).ToArrayAsync();
        }

        /// <summary>
        /// Delete document from provider and local metadata.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="KeyNotFoundException">If the id does not exist.</exception>
        /// <returns></returns>
        public virtual async Task<bool> DeleteDocument(Guid id)
        {
            var unitOfWork = this._unitOfWorkFactory();
            var repository = this._repositoryFactory();

            Metadata metadata = await this.FindIdDocOrThrowIfNotFound(repository, id);
            IDocumentProvider provider = this._providers[metadata.Provider];

            if (!await provider.DeleteAsync(metadata.CloudLink))
                return false;

            repository.Remove(metadata);
            await unitOfWork.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Get document data.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="KeyNotFoundException">If the id does not exist.</exception>
        /// <returns></returns>
        public virtual async Task<(Stream, Metadata)> GetDocument(Guid id)
        {
            var queryRepository = this._queryRepositoryFactory();
            
            Metadata metadata = await this.FindIdDocOrThrowIfNotFound(queryRepository, id);
            if (metadata.Expire != null && metadata.Expire <= DateTime.UtcNow)
                throw new KeyNotFoundException("Document is already expired.");

            IDocumentProvider provider = this._providers[metadata.Provider];
            
            Stream stream = await provider.GetAsync(metadata.CloudLink);
            return (stream, metadata);
        }
        /// <summary>
        /// Change data in cloud provider.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="expire"></param>
        /// <exception cref="KeyNotFoundException">if the id does not exist</exception>
        /// <exception cref="InvalidOperationException">if you can not update the data in the cloud</exception>
        public virtual async Task UpdateDocument(Guid id, Stream data, DateTime? expire)
        {
            var unitOfWork = this._unitOfWorkFactory();
            var repository = this._repositoryFactory();

            Metadata metadata = await this.FindIdDocOrThrowIfNotFound(repository, id);
            IDocumentProvider provider = this._providers[metadata.Provider];

            if (data != null)
            {
                string cloudLink = await provider.UpdateAsync(metadata.CloudLink, data);
                metadata.CloudLink = cloudLink ?? throw new InvalidOperationException($"Error from document provider {provider.Name}.");
            }
            if (expire != null)
                metadata.Expire = expire;

            await unitOfWork.SaveChangesAsync();
        }
        /// <summary>
        /// Create a document in the cloud with the specified data 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="extenssion"></param>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="expire"></param>
        /// <param name="initAction">If you want to performs extra initialization type specified action here.</param>
        /// <exception cref="ArgumentNullException">If extenssion is null.</exception>
        /// <exception cref="ArgumentException">If name is duplicate.</exception>
        /// <exception cref="InvalidOperationException">If you could not upload to the cloud or could not create metadata.</exception>
        /// <returns>Document identifier.</returns>
        public virtual async Task<Metadata> CreateDocument(Stream data, string extenssion, string folder = null, string name = null, DateTime? expire = null, Action<Metadata> initAction = null)
        {
            var unitOfWork = this._unitOfWorkFactory();
            var repository = this._repositoryFactory();

            if (extenssion == null)
                throw new ArgumentNullException(nameof(extenssion));

            if (name == null)
            {
                name = await this.GenerateUniqueName(repository, folder, extenssion);
            } else if (await repository.FindAll().AnyAsync(p => p.Folder == folder && p.Name == name))
                throw new ArgumentException("Duplicate name", nameof(name));

            IDocumentProvider provider = this._selectAlgorithm.Pick(data.Length - data.Position);
            string cloudLink = await provider.CreateAsync(data, folder, name);

            Metadata metadata = (Metadata)Activator.CreateInstance(this._metadataType);
            metadata.Name = name;
            metadata.Folder = folder;
            metadata.Expire = expire;
            metadata.MimeType = MimeUtility.GetMimeMapping(extenssion);
            metadata.Provider = provider.Identifier;
            metadata.CloudLink = cloudLink ?? throw new InvalidOperationException("Error from cloud document provider.");
            metadata.Length = data.Length;

            initAction?.Invoke(metadata);

            var state = await repository.AddAsync(metadata);
            if (state != SoftUnlimit.Data.EntityState.Added)
            {
                if (await provider.DeleteAsync(cloudLink) == false)
                    throw new InvalidOperationException("Canot add entity and document keep in cloud");

                throw new InvalidOperationException("Canot add entity");
            }

            await unitOfWork.SaveChangesAsync();
            return metadata;
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<Metadata> FindIdDocOrThrowIfNotFound(IQueryRepository<Metadata> repository, Guid id)
        {
            Metadata metadata = await repository.FindAsync(id);
            if (metadata == null)
                throw new KeyNotFoundException($"Document with id {id} not found.");

            return metadata;
        }
        /// <summary>
        /// Generate a unique name for a document.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="folder"></param>
        /// <param name="extenssion"></param>
        /// <returns></returns>
        private async Task<string> GenerateUniqueName(IQueryRepository<Metadata> repository, string folder, string extenssion)
        {
            string name;
            do
            {
                name = $"{Path.GetRandomFileName()}.{extenssion}";
            } while (await repository.FindAll().AnyAsync(p => p.Folder == folder && p.Name == name));

            return name;
        }

        #endregion
    }
}
