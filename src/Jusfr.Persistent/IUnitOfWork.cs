using System;

namespace Jusfr.Persistent
{
    public interface IUnitOfWork {
        bool DistributedTransactionSupported { get; }
        void Begin();
        void Rollback();
        void Commit();
    }
}
