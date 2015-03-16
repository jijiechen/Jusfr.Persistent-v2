using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent {
    public interface IRepositoryContext : IUnitOfWork, IDisposable {
        Guid ID { get; }
    }
}
