using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent {
    public class MockRepository<TEntry> : Repository<TEntry> where TEntry : class, IAggregate {
        private Int32 _id = 0;
        private readonly ConcurrentBag<TEntry> _all = new ConcurrentBag<TEntry>();

        public MockRepository()
            : base(null) {
        }

        public override void Create(TEntry entry) {
            System.Threading.Interlocked.Increment(ref _id);
            _all.Add(entry);
        }

        public override void Delete(TEntry entry) {
            _all.TryTake(out entry);
        }

        public override void Delete(IEnumerable<TEntry> entries) {
            TEntry entry;
            foreach (var entry2 in entries) {
                entry = entry2;
                _all.TryPeek(out entry);
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
            return default(TEntry);
        }

        public override IEnumerable<TEntry> Retrive(String field, IList<Int32> keys) {
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
