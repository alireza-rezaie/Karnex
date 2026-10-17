using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Specification
{
    public class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
    {
        //x=> x.title = title && x.CrerateDate > DateTime.Now || &&
        public Expression<Func<T, bool>> Predicate { get; }

        //List<Expression<Func<T, object>>> ISpecification<T>.Includes => throw new NotImplementedException();

        public List<Expression<Func<T, object>>> Includes { get; } = new();

        public Expression<Func<T, object>> OrderBy { get; private set; }

        public Expression<Func<T, object>> OrderByDesc { get; private set; }

        public int Take { get; set; }

        public int Skip { get; set; }

        public bool IsPagingEnabled { get; set; } = false;

        public BaseSpecification()
        {
            
        }
        public BaseSpecification(Expression<Func<T, bool>> predicate)
        {
            Predicate = predicate;
        }
        protected void AddInclude(Expression<Func<T, object>> include)
        {
            Includes.Add(include);
        }

        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void AddOrderByDesc(Expression<Func<T, object>> orderByExpressionDesc)
        {
            OrderByDesc = orderByExpressionDesc;
        }
        protected void ApplyPaging(int skip,int take,bool isPagingEnabled = true)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = isPagingEnabled;

        }
    }
}
