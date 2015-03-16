using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Jusfr.Persistent.NH {
    public class NHibernateRepository<TEntry> : Repository<TEntry>
        where TEntry : class, IAggregate {
        private readonly NHibernateRepositoryContext _context = null;

        public NHibernateRepositoryContext NHContext {
            get { return _context; }
        }

        public NHibernateRepository(IRepositoryContext context)
            : base(context) {
            _context = context as NHibernateRepositoryContext;
            if (_context == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect NHibernateRepositoryContext but provided " + context.GetType().FullName);
            }
        }

        public override IQueryable<TEntry> All {
            get {
                return NHContext.Of<TEntry>();
            }
        }

        public override TEntry Retrive(Int32 id) {
            return NHContext.EnsureSession().Get<TEntry>(id);
            //return (TEntry)NHContext.EnsureSession().Get(typeof(TEntry), id);
        }

        public override IEnumerable<TEntry> Retrive(String field, IList<Int32> keys) {
            var session = NHContext.EnsureSession();
            ICriteria criteria = session.CreateCriteria<TEntry>()
                .Add(Restrictions.In(field, keys.ToList()));
            return criteria.List<TEntry>();
        }

        public override void Create(TEntry entry) {
            NHContext.EnsureSession().Save(entry);
        }

        public override void Update(TEntry entry) {
            NHContext.EnsureSession().Update(entry);
        }

        public override void Update(IEnumerable<TEntry> entries) {
            var session = NHContext.EnsureSession();
            foreach (var entry in entries) {
                session.Update(entry);
            }
            session.Flush();
        }

        public override void Delete(TEntry entry) {
            var session = NHContext.EnsureSession();
            session.Delete(entry);
            session.Flush();
        }

        public override void Delete(IEnumerable<TEntry> entries) {
            var session = NHContext.EnsureSession();
            foreach (var entry in entries) {
                session.Delete(entry);
            }
            session.Flush();
        }
    }
}
