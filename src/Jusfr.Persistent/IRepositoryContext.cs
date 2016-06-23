using System;

namespace Jusfr.Persistent
{
    public interface IRepositoryContext : IUnitOfWork, IDisposable {
        Guid ID { get; }
    }
}
