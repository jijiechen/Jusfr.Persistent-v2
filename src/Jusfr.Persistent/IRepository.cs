using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent {
    public interface IRepository<in TEntry, out TResult> {
        void Create(TEntry entry);
        void Update(TEntry entry);
        void Delete(TEntry entry);

        IQueryable<TResult> All { get; }
        TReutrn Fetch<TReutrn>(Func<IQueryable<TResult>, TReutrn> query);
        TResult Retrive(Object id);
        IEnumerable<TResult> Retrive(Object[] keys);
        IEnumerable<TResult> Retrive<TKey>(String field, params TKey[] keys);
    }
}
