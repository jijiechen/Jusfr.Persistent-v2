using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jusfr.Persistent {
    public class MockRepository<TEntry> : Repository<TEntry>
        where TEntry : class, IAggregate {
        public const String IdField = "Id";
        private static Int32 _id = 0;
        private static readonly ConcurrentBag<TEntry> _all = new ConcurrentBag<TEntry>();

        public MockRepository()
            : base(null) {
        }

        public override void Create(TEntry entity) {
            Interlocked.Increment(ref _id);
            _all.Add(entity);
        }

        public override void Delete(TEntry entity) {
            _all.TryTake(out entity);
        }


        public override void Delete(IEnumerable<TEntry> entries) {
            TEntry current;
            foreach (var entry in entries) {
                current = entry;
                _all.TryPeek(out current);
            }
        }

        public override void Update(TEntry entry) {

        }

        public override void Update(IEnumerable<TEntry> entries) {

        }

        public override TEntry Retrive(int key) {
            foreach (var entry in _all) {
                if (entry.Id == key) {
                    return entry;
                }
            }
            return null;
        }

        public override IEnumerable<TEntry> Retrive(String field, IList<int> keys) {
            foreach (var entry in _all) {
                if (keys.Contains(entry.Id)) {
                    yield return entry;
                }
            }
        }

        public override IQueryable<TEntry> All {
            get { return _all.AsQueryable(); }
        }
    }
}
