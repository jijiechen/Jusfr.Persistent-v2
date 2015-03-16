using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent {
    public interface IRepository<in TEntry> where TEntry : IAggregate {
        void Create(TEntry entry);
        void Update(TEntry entry);
        void Update(IEnumerable<TEntry> entries);
        void Delete(TEntry entry);
        void Delete(IEnumerable<TEntry> entries);
    }

    public interface IQueryRepository<out TEntry> where TEntry : IAggregate {
        IQueryable<TEntry> All { get; }
        TEntry Retrive(Int32 id);
        IEnumerable<TEntry> Retrive(String field, IList<Int32> keys);
    }
}
