using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Model.Abstractions;
using Abitech.NextApi.Model.Paged;
using Abitech.NextApi.Server.Base;
using Abitech.NextApi.Server.Service;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Basic abstractions for NextApi entity service
    /// </summary>
    /// <typeparam name="TDto">Type of dto</typeparam>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TKey">Type of entity key</typeparam>
    /// <typeparam name="TUnitOfWork">Type of unit of work</typeparam>
    /// <typeparam name="TRepo"></typeparam>
    public abstract class NextApiEntityService<TDto, TEntity, TKey, TRepo, TUnitOfWork> : NextApiService,
        INextApiEntityService<TDto, TKey>
        where TDto : class
        where TEntity : class
        where TRepo : class, INextApiRepository<TEntity, TKey>
        where TUnitOfWork : class, INextApiUnitOfWork
    {
        private readonly TUnitOfWork _unitOfWork;
        private readonly TRepo _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Save changes to database automatically
        /// </summary>
        protected bool AutoCommit { get; set; } = true;

        /// <summary>
        /// Initializes instance of entity service 
        /// </summary>
        /// <param name="unitOfWork">Unit of work, used when saving data</param>
        /// <param name="mapper">Used for mapping operations</param>
        /// <param name="repository">Used for data access</param>
        protected NextApiEntityService(TUnitOfWork unitOfWork, IMapper mapper,
            TRepo repository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        /// <inheritdoc />
        public virtual async Task<TDto> Create(TDto entity)
        {
            var entityFromDto = _mapper.Map<TDto, TEntity>(entity);
            await _repository.AddAsync(entityFromDto);
            await CommitAsync();
            return _mapper.Map<TEntity, TDto>(entityFromDto);
        }


        /// <inheritdoc />
        public virtual async Task Delete(TKey key)
        {
            await _repository.DeleteAsync(_repository.KeyPredicate(key));
            await CommitAsync();
        }


        /// <inheritdoc />
        public virtual async Task<TDto> Update(TKey key, TDto patch)
        {
            var entity = await _repository.GetByIdAsync(key);
            if (entity == null)
                throw new Exception($"Entity with id {key} is not exists");
            NextApiUtils.PatchEntity(patch, entity);
            await _repository.UpdateAsync(entity);
            await CommitAsync();
            return _mapper.Map<TEntity, TDto>(entity);
        }

        /// <inheritdoc />
        public virtual async Task<PagedList<TDto>> GetPaged(PagedRequest request)
        {
            var entitiesQuery = _repository.GetAll().Expand(request.Expand);
            // apply filter
            var filterExpression = request.Filter?.ToLambdaFilter<TEntity>();
            if (filterExpression != null)
            {
                entitiesQuery = entitiesQuery.Where(filterExpression);
            }

            var totalCount = entitiesQuery.Count();
            if (request.Skip != null)
                entitiesQuery = entitiesQuery.Skip(request.Skip.Value);
            if (request.Take != null)
                entitiesQuery = entitiesQuery.Take(request.Take.Value);
            var entities = await entitiesQuery.ToListAsync();
            return new PagedList<TDto>
            {
                Items = _mapper.Map<List<TEntity>, List<TDto>>(entities),
                TotalItems = totalCount
            };
        }

        /// <inheritdoc />
        public virtual async Task<TDto> GetById(TKey key, string[] expand = null)
        {
            var entity = await _repository
                .GetAll()
                .Where(_repository.KeyPredicate(key))
                .Expand(expand)
                .FirstOrDefaultAsync();
            if (entity == null)
                throw new Exception("Entity is not exists");
            return _mapper.Map<TEntity, TDto>(entity);
        }

        /// <summary>
        /// Implementation of GetByIds(get array of entities)
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<TDto[]> GetByIds(TKey[] keys, string[] expand = null)
        {
            var entities = await _repository
                .GetAll()
                .Where(_repository.KeyPredicate(keys))
                .Expand(expand)
                .ToArrayAsync();

            if (entities == null)
                throw new Exception("Entity is not exists");

            return _mapper.Map<TEntity[], TDto[]>(entities);
        }

        /// <summary>
        /// Commits changes in repo
        /// </summary>
        /// <returns></returns>
        protected async Task CommitAsync()
        {
            if (AutoCommit)
            {
                await _unitOfWork.CommitAsync();
            }
        }
    }
}
