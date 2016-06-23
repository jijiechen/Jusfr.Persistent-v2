using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;

namespace Jusfr.Persistent.NHibernate {
    public interface INHibernateRepository {
        void Evict<IEntry>(params IEntry[] entries);
        void Flush();
    }

    public class NHibernateRepositoryContext : IRepositoryContext, INHibernateRepository {
        private static Int32 _count = 0;
        private readonly Guid _id = Guid.NewGuid();
        private readonly ISessionFactory _sessionFactory;
        private ISession _session;
        private Boolean _suspendTransaction = false;
        private Boolean _alwaysCommit = false;

        public Guid ID {
            get { return _id; }
        }

        public Boolean DistributedTransactionSupported {
            get { return true; }
        }

        public virtual IQueryable<TEntry> Of<TEntry>() {
            return EnsureSession().Query<TEntry>();
        }

        internal ISessionFactory SessionFactory {
            get { return _sessionFactory; }
        }

        public NHibernateRepositoryContext(ISessionFactory sessionFactory) {
            _sessionFactory = sessionFactory;

            var alwaysCommit = System.Configuration.ConfigurationManager.AppSettings
                .Get("NHibernate:alwaysCommit");
            _alwaysCommit = "true".Equals(alwaysCommit, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose() {
            if (_session == null) {
                return;
            }
            try {
                if (_alwaysCommit) {
                    if (_session.Transaction.IsActive) {
                        Commit();
                    }
                    else {
                        Flush();
                    }
                }
                else {
                    Rollback();
                }
            }
            finally {
                Debug.WriteLine("(NH:Session dispose, left {0})",
                    Interlocked.Decrement(ref _count));
                _session.Close();
                _session.Dispose();
                _session = null;
            }
        }

        public virtual ISession EnsureSession() {
            if (_session != null) {
                return _session;
            }
            Debug.WriteLine("(NH:Session open, count {0})",
                Interlocked.Increment(ref _count));
            _session = _sessionFactory.OpenSession();
            if (_suspendTransaction && !_session.Transaction.IsActive) {
                Debug.WriteLine("(NH:Transaction begin)");
                _session.BeginTransaction();
            }
            return _session;
        }

        //仅在Session对象已创建, 但事务未创建或已提交的情况下开启新事务
        public virtual void Begin() {
            _suspendTransaction = true;
            if (_session != null && !_session.Transaction.IsActive) {
                Debug.WriteLine("(NH:Transaction begin)");
                _session.BeginTransaction();
            }
        }

        //仅在事务已创建且处于活动中时回滚事务
        public virtual void Rollback() {
            if (_session != null && _session.Transaction.IsActive) {
                Debug.WriteLine("(NH:Transaction rollback)");
                _session.Transaction.Rollback();
                _session.Clear();
            }
        }

        //仅在事务已创建且处于活动中时提交事务
        public virtual void Commit() {
            if (_session == null) {
                return;
            }
            try {
                if (_session.Transaction.IsActive) {
                    Debug.WriteLine("(NH:Transaction commit)");
                    _session.Transaction.Commit();
                }
            }
            catch {
                if (_session.Transaction.IsActive) {
                    Debug.WriteLine("(NH:Transaction rollback)");
                    _session.Transaction.Rollback();
                }
                _session.Clear();
                throw;
            }
            finally {
                if (_session.Transaction.IsActive) {
                    Debug.WriteLine("(NH:Transaction dispose)");
                    _session.Transaction.Dispose();
                }
            }
        }

        public void Evict<TEntry>(params TEntry[] entries) {
            if (_session != null) {
                foreach (var entry in entries) {
                    _session.Evict(entry);
                }
            }
        }

        public void Flush() {
            if (_session != null) {
                _session.Flush();
            }
        }
    }
}
