using Jusfr.Persistent.Mongo;
using Jusfr.Persistent.NH;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Jusfr.Persistent.Demo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            //MongoBasicCrud();
            //NHibernateBasicCrud();
            //Null_could_evict();
            //Dupliate_entity_update_need_evict();
            Dupliate_entity_mock_web_cache();
        }

        private static void Dupliate_entity_mock_web_cache() {
            ISessionFactory sessionFactory = PubsContext.DbFactory;
            Job cache;

            using (var session = sessionFactory.OpenSession())
            /*using (session.BeginTransaction())*/ {
                var job1 = session.Get<Job>(1);
                var job2 = session.Get<Job>(1);

                Console.WriteLine("job1 == job2 ? {0}", job1 == job2); // true, 1级缓存默认生效
                cache = job1; //模仿 HttpRuntime.Cache
                //或者模仿 Memcached 等分布式 Cache
                //cache = new Job {
                //    Guid   = job1.Guid,
                //    Id     = job1.Id,
                //    Salary = job1.Salary,
                //    Title  = job1.Title
                //};
                Console.WriteLine();
            }

            using (var session = sessionFactory.OpenSession())
            using (session.BeginTransaction()) {
                //假设某场景仍然查出了 Job#1，
                var job = session.Get<Job>(1);
                Console.WriteLine("job == cache ? {0}", job == cache); // false，没有启用2级缓存
                Console.WriteLine();

                //但更新操作针对的是 cache, 此时抛出异常
                try {
                    session.Update(cache);
                    Debug.Fail("Should failed for NHibernate.NonUniqueObjectException");
                }
                catch (NHibernate.NonUniqueObjectException ex) {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Got NHibernate.NonUniqueObjectException with Entity {0}", ex.EntityName);
                    Console.ResetColor();
                }

                //Evict cache 没有用
                session.Evict(cache); 
                try {
                    session.Update(cache);
                    Debug.Fail("Should failed for NHibernate.NonUniqueObjectException");
                }
                catch (NHibernate.NonUniqueObjectException ex) {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Got NHibernate.NonUniqueObjectException with Entity {0}", ex.EntityName);
                    Console.ResetColor();
                }

                //Evict Job#1 才有效
                session.Evict(job);
                session.Update(cache);
            }
        }

        private static void ShowSessionFactoryStatistics(ISessionFactory sessionFactory) {
            var props = sessionFactory.Statistics.GetType().GetProperties();
            foreach (var prop in props) {
                if (prop.PropertyType == typeof(Int64)) {
                    Console.WriteLine("{0, 30} = {1}",
                        prop.Name, prop.GetValue(sessionFactory.Statistics));
                }
            }
        }

        private static void Dupliate_entity_update_need_evict() {
            var context = new PubsContext();
            var jobRepo = new NHibernateRepository<Job>(context);

            var j1 = jobRepo.Retrive(1);
            var j2 = jobRepo.Retrive(1);
            Console.WriteLine("j1 == j2? {0}", j1 == j2); // True

            var session = context.EnsureSession();
            Console.WriteLine("before evict, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            //j1 == j2, 都指向了Job#1，Evict后是游离态
            session.Evict(j2);
            Console.WriteLine("after  evict, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            //Job#1 这个实体并没有任何持久态存在，以下2句完全相同
            session.Update(j1);
            session.Update(j2);

            Console.WriteLine("after update, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            var j3 = new Job {
                Id = j1.Id,
                Salary = j2.Salary,
                Title = j2.Title
            };

            try {
                j3.Salary += 1;
                jobRepo.Update(j3); //Failed
                //a different object with the same identifier value was already associated with the session: 1, of entity: Jusfr.Persistent.Demo.Job
                Debug.Fail("Should failed for NHibernate.NonUniqueObjectException");
            }
            catch (NHibernate.NonUniqueObjectException ex) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Got NHibernate.NonUniqueObjectException with Entity {0}", ex.EntityName);
                Console.ResetColor();
            }

            Console.WriteLine("before evict, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);
            session.Evict(j1);  //移除j1的游离态，对j3的更新操作才能完成

            Console.WriteLine("after  evict, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            jobRepo.Update(j3); //Pass
            Console.WriteLine("j1.Salary: {0}, j3.Salary: {1}", j1.Salary, j3.Salary);
            // j1.Salary: 2760.00000, j3.Salary: 2761.00000

            Console.WriteLine("after update, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            Console.WriteLine("j1 == j3? {0}", j1 == j3); // True
            //j1 和 j3 还是不同，但1个处于持久态时，更新另1个必然导致 NonUniqueObjectException 异常
            Console.WriteLine();
        }

        private static void Null_could_evict() {
            var context = new PubsContext();
            var jobRepo = new NHibernateRepository<Job>(context);

            var session = context.EnsureSession();
            session.Evict(null);

            var jobNotExist = jobRepo.Retrive(-100);
            session.Evict(jobNotExist);
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
            foreach (var entry in employeeRepo.All.Where(r => r.Job.Salary > 3000)) {
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
            context.Dispose();
        }
    }
}
