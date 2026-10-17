using Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IUnitOfWork
    {
        DbContext dbContext { get; }
        Task<int> Save(CancellationToken cancellationToken);
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
    }
}
