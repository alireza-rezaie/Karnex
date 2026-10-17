using Application.Contracts;
using Application.Contracts.Specification;
using Domain.Entities.Base;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
        {
             await _dbSet.AddAsync(entity, cancellationToken);
            return await Task.FromResult(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<T,bool>> expression, CancellationToken cancellationToken)
        {
            return await _dbSet.AnyAsync(expression,cancellationToken);
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.AnyAsync(cancellationToken);
        }

        public async void Delete(T entity, CancellationToken cancellationToken)
        {
            var record = await GetByIdAsync(entity.Id, cancellationToken);
            record.IsDelete = true;
            await UpdateAsync(entity);
            //_context.Entry(entity).State = EntityState.Detached;
            //_context.Remove(entity);

        }

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbSet.FindAsync(id, cancellationToken);
        }


        public async Task<T> UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return await Task.FromResult(entity);

        }

        public async Task<T> GetEntityWithSpec(ISpecification<T> Spec , CancellationToken cancellationToken)
        {
            return await ApplySpecification(Spec).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<T>> ListAsyncSpec(ISpecification<T> Spec, CancellationToken cancellationToken)
        {
            return await ApplySpecification(Spec).ToListAsync(cancellationToken);
        }



        public async Task<int> CountAsyncSpec(ISpecification<T> Spec, CancellationToken cancellationToken)
        {
            return await ApplySpecification(Spec).CountAsync(cancellationToken);
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> Spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), Spec);
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression);
        }
        public async Task<List<T>> ToListAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }
    }
}
