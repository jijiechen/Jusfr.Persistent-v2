using Jusfr.Persistent.Mongo;
using Jusfr.Persistent.NH;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Jusfr.Persistent.Demo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            //MongoBasicCrud();
            //NHibernateBasicCrud();
            //HybridPrepareData();
            HybridBasicCrud();
        }

        private static void MongoBasicCrud() {
            var conStr = ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString;
            var context = new MongoRepositoryContext(conStr);
            var employeeRepo = new MongoRepository<Employee>(context);

            Console.WriteLine("Remove all employee");
            context.Database.GetCollection<Employee>().RemoveAll();
            Console.WriteLine();

            var jobTitles = new[] { "Java", "C", "C++", "Objective-C", "C#", "JavaScript", "PHP", "Python" };
            var employeeNames = new[] { "Charles", "Mark", "Bill", "Vincent", "William", "Joseph", "James", "Henry", "Gary", "Martin" };

            for (int i = 0; i < employeeNames.Length; i++) {
                var entry = new Employee {
                    Name = employeeNames[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                    Job = new Job {
                        Title = jobTitles[Math.Abs(Guid.NewGuid().GetHashCode() % jobTitles.Length)],
                        Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000)
                    }
                };
                employeeRepo.Create(entry);
            }

            Console.WriteLine("Query all employee");
            var query = employeeRepo.All.Where(r => r.Job.Salary > 3000);
            foreach (var entry in query) {
                Console.WriteLine("{0,-10} {1}", entry.Name, entry.Job.Salary);
            }
            Console.WriteLine();
        }

        private static void NHibernateBasicCrud() {
            var context = new PubsContext();
            var jobRepo = new NHibernateRepository<Job>(context);
            var jobTitles = new[] { "Java", "C", "C++", "Objective-C", "C#", "JavaScript", "PHP", "Python" };

            context.Begin();
            Console.WriteLine("Remove all jobs");
            context.EnsureSession().CreateSQLQuery("delete from Employee").ExecuteUpdate();

            for (int i = 0; i < jobTitles.Length; i++) {
                var job = new Job {
                    Title = jobTitles[i],
                    Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000 + 8000),
                };
                jobRepo.Create(job);
            }

            var jobs = jobRepo.All.ToList();
            var employeeRepo = new NHibernateRepository<Employee>(context);
            Console.WriteLine("Remove all employee");
            context.EnsureSession().CreateSQLQuery("delete from Employee").ExecuteUpdate();
            Console.WriteLine();

            var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin"
                .Split('、', ' ');
            for (int i = 0; i < names.Length; i++) {
                var entry = new Employee {
                    Name = names[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                    Job = jobs[Math.Abs(Guid.NewGuid().GetHashCode() % jobs.Count)],
                };
                employeeRepo.Create(entry);
            }

            Console.WriteLine("Query all employee");
            foreach (var entry in employeeRepo.All.Where(r => r.Job.Salary > 3000)) {
                Console.WriteLine("{0,-10} {1}", entry.Name, entry.Job.Salary);
            }
            Console.WriteLine();

            context.Commit();
        }

        private static void HybridPrepareData() {
            var nhibernateContext = new PubsContext();
            var nhibernateEmployeeRepo = new NHibernateRepository<Employee>(nhibernateContext);

            var mongoContext = new MongoRepositoryContext(ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString);
            var mongoMmployeeRepo = new MongoRepository<Employee>(mongoContext);

            Console.WriteLine("Remove all employee");
            mongoContext.Database.GetCollection<Employee>().RemoveAll();
            nhibernateContext.EnsureSession().CreateSQLQuery("delete from employee").ExecuteUpdate();
            Console.WriteLine();

            var employeeNames = new[] { "Charles", "Mark", "Bill", "Vincent", "William", "Joseph", "James", "Henry", "Gary", "Martin" };
            for (int i = 0; i < employeeNames.Length; i++) {
                var entry = new Employee {
                    Name = employeeNames[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                };
                nhibernateEmployeeRepo.Create(entry);
            }
        }

        private static void HybridBasicCrud() {
            var nhibernateContext = new PubsContext();
            var nhibernateEmployeeRepo = new NHibernateRepository<Employee>(nhibernateContext);
            var hybridEmployeeRepo = new HybridRepository<Employee>(nhibernateEmployeeRepo);
            nhibernateContext.Begin();

            var list = hybridEmployeeRepo.Fetch(x => x.Where(r => r.Id > 215).OrderByDescending(r => r.Id).Take(5));

            Console.WriteLine("Create employee");
            var emp1 = new Employee {
                Name = "Aimee",
                Address = "Los Angeles",
                Birth = DateTime.Now,
            };
            hybridEmployeeRepo.Create(emp1);

            var emp2 = new Employee {
                Name = "Becky",
                Address = "Bejing",
                Birth = DateTime.Now,
            };
            hybridEmployeeRepo.Create(emp2);

            Console.WriteLine("Update employee");
            emp2.Birth = emp2.Birth.AddYears(-1);
            hybridEmployeeRepo.Update(emp2);
            Console.WriteLine();

            var emp3 = new Employee {
                Name = "Carmen",
                Address = "Salt Lake City",
                Birth = DateTime.Now,
            };
            hybridEmployeeRepo.Create(emp3);
            Console.WriteLine();

            Console.WriteLine("Delete employee");
            hybridEmployeeRepo.Delete(emp3);
            Console.WriteLine();

            Console.WriteLine("Employee in top 4");

            //var topArray = hybridEmployeeRepo.All.OrderByDescending(r => r.Id).Select(r => r.Id).Take(4).ToArray();
            var topArray = hybridEmployeeRepo.Fetch(x => x.OrderByDescending(r => r.Id).Take(4)).Select(x => x.Id).ToArray();
            foreach (var entry in hybridEmployeeRepo.Retrive(topArray)) {
                Console.WriteLine("{0,-10} {1}", entry.Name, entry.Address);
            }

            //var maxId = hybridEmployeeRepo.All.Max(r => r.Id);
            //for (int i = 0; i < 3; i++) {
            //    hybridEmployeeRepo.Retrive(maxId - i - 3);
            //}
        }
    }

    public class HybridRepository<TEntry> : MongoRepository<TEntry> where TEntry : class, IAggregate {
        Repository<TEntry> _mysqlRepo;

        private static IRepositoryContext GetMonogContext() {
            return new MongoRepositoryContext(ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString);
        }

public override IQueryable<TEntry> All {
    get {
        throw new NotSupportedException();
    }
}

        public HybridRepository(Repository<TEntry> mysqlRepo)
            : base(GetMonogContext()) {
            _mysqlRepo = mysqlRepo;
        }

        public override void Create(TEntry entry) {
            _mysqlRepo.Create(entry);
            base.Save(entry);
        }

        public override void Update(TEntry entry) {
            _mysqlRepo.Update(entry);
            base.Update(entry);
        }

        public override void Update(IEnumerable<TEntry> entries) {
            _mysqlRepo.Update(entries);
            base.Update(entries);
        }

        public override TEntry Retrive(int id) {
            var entry = base.Retrive(id);
            if (entry == null) {
                entry = _mysqlRepo.Retrive(id);
                if (entry != null) {
                    Save(entry);
                }
            }
            return entry;
        }

        public override IEnumerable<TEntry> Retrive(IList<int> keys) {
            return Retrive("_id", keys);
        }

        public override IEnumerable<TEntry> Retrive<TKey>(String field, IList<TKey> keys) {
            if (typeof(TKey) != typeof(Int32) || !field.ToLower().Contains("id")) {
                throw new NotSupportedException();
            }
            var array = keys.Cast<Int32>().ToArray();

            var entries = base.Retrive(field, keys).ToList();
            if (entries.Count < keys.Count) {
                var keysInMysql = array.Except(entries.Select(r => r.Id)).ToArray();
                var entriesInMysql = _mysqlRepo.Retrive(keysInMysql);
                foreach (var entry in entriesInMysql) {
                    Save(entry);
                }
                entries.AddRange(entriesInMysql);
            }
            return entries;
        }

        public override IEnumerable<TEntry> Retrive<TKey>(Expression<Func<TEntry, TKey>> selector, IList<TKey> keys) {
            var entries = base.Retrive<TKey>(selector, keys).ToList();
            if (entries.Count < keys.Count) {
                var selectorFunc = selector.Compile();
                var field = ExpressionBuilder.GetPropertyInfo(selector).Name;
                var keysInMysql = keys.Except(entries.Select(selectorFunc)).ToArray();
                var entriesInMysql = _mysqlRepo.Retrive(field, keysInMysql);
                foreach (var entry in entriesInMysql) {
                    Save(entry);
                }
                entries.AddRange(entriesInMysql);
            }
            return entries;
        }

        public override void Delete(TEntry entry) {
            _mysqlRepo.Delete(entry);
            base.Delete(entry);
        }

        public List<TEntry> Fetch(Func<IQueryable<TEntry>, IQueryable<TEntry>> predicate) {
            var entriesInMongo = predicate(base.All).ToList();
            var entriesInMysql = predicate(_mysqlRepo.All).ToList();
            var session = ((NHibernateRepositoryContext)_mysqlRepo.Context).EnsureSession();
            entriesInMongo.ForEach(r => session.Evict(r));

            var keysInMysql = entriesInMysql.Select(r => r.Id).Except(entriesInMongo.Select(r => r.Id)).ToArray();
            var extraInMysql = _mysqlRepo.Retrive(keysInMysql);
            foreach (var entry in extraInMysql) {
                Save(entry);
            }
            entriesInMongo.AddRange(entriesInMysql);
            return entriesInMongo;
        }
    }
}
