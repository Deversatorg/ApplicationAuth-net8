using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Threading.Tasks;

namespace ApplicationAuth.DAL.Abstract
{
    public interface IDataContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        IModel Model { get; }

        int SaveChanges();

        Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default);
    }
}
