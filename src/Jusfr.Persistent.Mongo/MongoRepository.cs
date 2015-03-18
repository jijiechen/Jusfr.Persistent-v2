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
        private readonly IMongoEntryMapper _entryMapper = new MongoEntryMapper();

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
                    .GetCollection<TEntry>(_entryMapper.Map<TEntry>()).AsQueryable();
            }
        }

        public override TEntry Retrive(int id) {
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(_entryMapper.Map<TEntry>());
            return docs.FindOneById(id);
        }

        public override IEnumerable<TEntry> Retrive<TKey>(String field, IList<TKey> keys) {
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(_entryMapper.Map<TEntry>());
            //return docs.Find(Query<TEntry>.In(r => r.Id, keys));
            return docs.Find(Query.In(field, keys.Select(k => BsonValue.Create(k)))).AsEnumerable();
        }

        public override void Create(TEntry entry) {
            var entryName = _entryMapper.Map<TEntry>();
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(entryName);
            entry.Id = _autoincrementGenerator.GetNewId(entryName);
            docs.Insert(entry);
        }

        public override void Update(TEntry entry) {
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(_entryMapper.Map<TEntry>());
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
            var docs = _context.DatabaseFactory().GetCollection<TEntry>(_entryMapper.Map<TEntry>());
            docs.Remove(Query<TEntry>.EQ(r => r.Id, entry.Id), RemoveFlags.Single);
        }

        public override void Delete(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Delete(entry);
            }
        }
    }
}
