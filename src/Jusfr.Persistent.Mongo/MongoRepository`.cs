using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;

namespace Jusfr.Persistent.Mongo {
    public class MongoRepository<TEntry> : Repository<TEntry> where TEntry : class, IAggregate {
        private readonly MongoRepositoryContext _context = null;
        private readonly MongoAutoincrementGenerator _autoincrementGenerator;

        public MongoRepositoryContext MGContext {
            get { return _context; }
        }

        public MongoRepository(IRepositoryContext context)
            : base(context) {
            _context = context as MongoRepositoryContext;
            if (_context == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect MongoRepositoryContext but provided " + context.GetType().FullName);
            }
            _autoincrementGenerator = new MongoAutoincrementGenerator(_context);
        }


        public override IQueryable<TEntry> All {
            get {
                return _context.DatabaseFactory()
                    .GetCollection<TEntry>(typeof(TEntry).Name).AsQueryable();
            }
        }

        public override TEntry Retrive(int id) {
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(typeof(TEntry).Name);
            return docs.FindOneById(id);
        }

        public override IEnumerable<TEntry> Retrive(String filed, IList<Int32> keys) {
            throw new NotImplementedException();
        }

        public override void Create(TEntry entry) {
            var entryName = typeof(TEntry).Name;
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(entryName);
            entry.Id = _autoincrementGenerator.GetNewId(entryName);
            docs.Insert(entry);
        }

        public override void Update(TEntry entry) {
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(typeof(TEntry).Name);
            docs.Update(Query<TEntry>.EQ(r => r.Id, entry.Id),
                Update<TEntry>.Replace(entry),
                UpdateFlags.Upsert);
        }

        public override void Update(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Update(entry);
            }
        }

        public override void Delete(TEntry entry) {
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(typeof(TEntry).Name);
            docs.Remove(Query<TEntry>.EQ(r => r.Id, entry.Id), RemoveFlags.Single);
        }

        public override void Delete(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Delete(entry);
            }
        }
    }
}
