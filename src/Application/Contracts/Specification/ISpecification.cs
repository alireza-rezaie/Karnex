using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Specification
{
    public interface ISpecification<T> where T : BaseEntity
    {
        //x=>x.titel
        Expression<Func<T, bool>> Predicate { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDesc { get; }
        public int Take { get; }
        public int Skip { get; }
        public bool IsPagingEnabled { get; } 



    }
}
