using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Jusfr.Persistent.NH;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using FluentNHibernate.Mapping;

namespace Demo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            //NHibernateOperateOnCobar(); return;
            //PetaPocoOperateOnCobar(); return;
            //PetaPocoOperateOnPort3366(); return;
            //NHibernateOperateOnPort3366(); return;
            ConventionTest();
        }

        private static void ConventionTest() {
            using (var context = new PubsContext()) {
                var jobRepo = new NHibernateRepository<Job>(context);

                // 逆变, 编译通过, 由于 NHibernate 的 Mapping 机制抛出下列异常
                // NHibernate.MappingException {"No persister for: Demo.Program+MyClass"}
                JobEx j1 = new JobEx() { Title = "new" };
                jobRepo.Create(j1); 

                var jobs = jobRepo.All;
                IQueryable<IAggregate> roots = jobs; //协变

                foreach (var root in roots) {
                    Console.WriteLine("{0,2} {1}", root.Id, ((Job)root).Title);
                }
            }
        }

        

        private static void NHibernateOperateOnCobar() {
            using (var context = new PubsContext()) {
                //context.Begin();
                context.EnsureSession().CreateSQLQuery("DELETE FROM Job;").ExecuteUpdate();
                context.EnsureSession().CreateSQLQuery("DELETE FROM Employee;").ExecuteUpdate();

                var jobRepo = new NHibernateRepository<Job>(context);
                var jobs = EnumJobs().ToArray();
                var index = 0;
                foreach (var entry in jobs) {
                    entry.Id = ++index;
                    jobRepo.Create(entry);
                }

                index = 0;
                var empRepo = new NHibernateRepository<Employee>(context);
                var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin".Split('、', ' ');
                foreach (var name in names) {
                    var entry = new Employee {
                        Id = ++index,
                        Name = name,
                        Address = Guid.NewGuid().ToString().Substring(0, 8),
                        Birth = DateTime.UtcNow,
                        Job = jobs[Math.Abs(Guid.NewGuid().GetHashCode() % jobs.Length)],
                    };
                    empRepo.Create(entry);
                }
                context.EnsureSession().Flush();
            }
            using (var context = new PubsContext()) {
                var jobRepo = new NHibernateRepository<Job>(context);
                var jobs = jobRepo.All;
                foreach (var entry in jobs) {
                    entry.Salary += 100;
                    jobRepo.Update(entry);
                }

                jobs = jobRepo.All;
                Console.WriteLine("Query all jobs");
                foreach (var job in jobs) {
                    Console.WriteLine("{0,2} {1,10} {2:f2}", job.Id, job.Title, job.Salary);
                }

                var empRepo = new NHibernateRepository<Employee>(context);
                var emps = empRepo.All;
                Console.WriteLine("Query all employee");
                foreach (var entry in emps) {
                    Console.WriteLine("{0,2} {1,10} {2}",
                        entry.Id, entry.Name, entry.Address);
                }
            }
        }

        private static void PetaPocoOperateOnCobar() {
            using (var db = new PetaPoco.Database("TestDb")) {
                db.Execute("DELETE FROM Job;");
                db.Execute("DELETE FROM Employee;");

                var index = 0;
                foreach (var entry in EnumJobs()) {
                    entry.Id = ++index;
                    db.Insert(entry);
                }

                var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin".Split('、', ' ');
                for (int i = 0; i < names.Length; i++) {
                    var entry = new Employee {
                        Id = i,
                        Name = names[i],
                        Address = Guid.NewGuid().ToString().Substring(0, 8),
                        Birth = DateTime.UtcNow,
                        //Job = null
                    };
                    db.Insert(entry);
                }
            }

            using (var db = new PetaPoco.Database("TestDb")) {
                var jobs = db.Fetch<Job>("SELECT * FROM Job");
                foreach (var entry in jobs) {
                    entry.Salary += 100;
                    db.Update(entry);
                }

                jobs = db.Fetch<Job>("SELECT * FROM Job");
                foreach (var job in jobs) {
                    Console.WriteLine("{0,2} {1,10} {2:f2}", job.Id, job.Title, job.Salary);
                }

                var emps = db.Query<Employee>("SELECT * FROM Employee");
                foreach (var entry in emps) {
                    Console.WriteLine("{0,2} {1,10} {2}",
                        entry.Id, entry.Name, entry.Address);
                }


            }
        }

        private static void NHibernateOperateOnPort3366() {
            using (var context = new PubsContext()) {
                var jobRepo = new NHibernateRepository<Job>(context);
                var jobs = EnumJobs().ToArray();
                foreach (var entry in jobs) {
                    jobRepo.Create(entry);
                }

                var empRepo = new NHibernateRepository<Employee>(context);
                var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin".Split('、', ' ');
                for (int i = 0; i < names.Length; i++) {
                    var entry = new Employee {
                        Name = names[i],
                        Address = Guid.NewGuid().ToString().Substring(0, 8),
                        Birth = DateTime.UtcNow,
                        Job = jobs[Math.Abs(Guid.NewGuid().GetHashCode() % jobs.Length)],
                    };
                    empRepo.Create(entry);
                }
            }
            using (var context = new PubsContext()) {
                var jobRepo = new NHibernateRepository<Job>(context);
                var jobs = jobRepo.All;
                Console.WriteLine("Query all jobs");
                foreach (var job in jobs) {
                    Console.WriteLine("{0,2} {1,10} {2:f2}", job.Id, job.Title, job.Salary);
                }

                var empRepo = new NHibernateRepository<Employee>(context);
                var emps = empRepo.All;
                Console.WriteLine("Query all employee");
                foreach (var entry in emps) {
                    Console.WriteLine("{0,2} {1,10} {2}",
                        entry.Id, entry.Name, entry.Address);
                }
            }
        }

        private static void PetaPocoOperateOnPort3366() {
            using (var db = new PetaPoco.Database("TestDb")) {
                Console.WriteLine("Delete all jobs");
                db.Execute("DELETE FROM Job;");
                Console.WriteLine("Delete all employee");
                db.Execute("DELETE FROM Employee;");

                foreach (var entry in EnumJobs()) {
                    db.Insert(entry);
                }

                var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin".Split('、', ' ');
                for (int i = 0; i < names.Length; i++) {
                    var entry = new Employee {
                        Name = names[i],
                        Address = Guid.NewGuid().ToString().Substring(0, 8),
                        Birth = DateTime.UtcNow,
                        //Job = null
                    };
                    db.Insert(entry);
                }
            }
            using (var db = new PetaPoco.Database("TestDb")) {
                var jobs = db.Query<Job>("SELECT * FROM Job");
                Console.WriteLine("Query all jobs");
                foreach (var job in jobs) {
                    Console.WriteLine("{0,2} {1,10} {2:f2}", job.Id, job.Title, job.Salary);
                }

                var emps = db.Query<Employee>("SELECT * FROM Employee");
                Console.WriteLine("Query all employee");
                foreach (var entry in emps) {
                    Console.WriteLine("{0,2} {1,10} {2}",
                        entry.Id, entry.Name, entry.Address);
                }
            }
        }

        private static IEnumerable<Job> EnumJobs() {
            yield return new Job { Title = "C#", Salary = 4000 };
            yield return new Job { Title = "Java", Salary = 5000 };
            yield return new Job { Title = "JavaScript", Salary = 3000 };
            yield return new Job { Title = "Perl", Salary = 4800 };
            yield return new Job { Title = "Python", Salary = 4900 };
            yield return new Job { Title = "C++", Salary = 5900 };
            yield return new Job { Title = "Objective-C", Salary = 5900 };
        }

        private static void BulkCopyTest() {
            var conStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var bcpHelper = new BulkCopyHelper(conStr);
            bcpHelper.Insert(typeof(Employee).Name, GenerateEmployee(100000L));
        }

        #region NHibernate

        static IEnumerable<Employee> GenerateEmployee(Int64 count) {
            for (int i = 0; i < count; i++) {
                yield return new Employee {
                    Name = Guid.NewGuid().ToString("n"),
                    Address = Guid.NewGuid().ToString("n"),
                    Birth = DateTime.Now,
                };
            }
        }

        private static void TransactionTest() {
            var factory = BuildSessionFactory();
            using (var context = new NHibernateRepositoryContext(factory)) {
                context.AutoTransaction = true;
                var repository = new NHibernateRepository<Employee>(context);
                repository.Delete(repository.All.AsEnumerable());
                var entry = new Employee {
                    Name = Guid.NewGuid().ToString("n"),
                    Address = Guid.NewGuid().ToString("n"),
                    Birth = DateTime.Now,
                };
                repository.Create(entry);
                context.Commit();

                entry.Name = "Josie";
                repository.Update(entry);
                context.Commit();

                entry.Address = "Wuhan";
                repository.Update(entry);
                context.Rollback();
            }
        }

        private static void SQLTest() {
            var factory = BuildSessionFactory();
            using (var context = new NHibernateRepositoryContext(factory)) {
                var builder = new StringBuilder();
                builder.AppendLine("UPDATE dbo.Employee SET JobId = JobId + 1;");
                builder.AppendLine("UPDATE dbo.Employee SET JobId = JobId + 1;");
                var query = context.EnsureSession()
                    .CreateSQLQuery(builder.ToString())
                    .ExecuteUpdate();
            }
        }

        private static void PagingDemo() {
            var factory = BuildSessionFactory();
            using (var context = new NHibernateRepositoryContext(factory)) {
                var jobRepository = new NHibernateRepository<Job>(context);
                //context.EnsureSession().CreateSQLQuery("TRUNCATE TABLE [Job]").ExecuteUpdate();
                var pagings = jobRepository.All.EnumPaging(20, true);
                foreach (var paging in pagings) {
                    Console.WriteLine("Paging {0}/{1}, Items {2}",
                        paging.CurrentPage, paging.TotalPages, paging.Items.Count());
                    paging.CurrentPage = 100;
                }
            }
        }

        private static void PagingSelectorDemo() {
            var factory = BuildSessionFactory();
            using (var context = new NHibernateRepositoryContext(factory)) {
                var jobRepository = new NHibernateRepository<Employee>(context);
                //context.EnsureSession().CreateSQLQuery("TRUNCATE TABLE [Job]").ExecuteUpdate();
                //var paging = jobRepository.All.Paging(r => r.Name, 1, 10);
                //Console.WriteLine("Paging {0}/{1}",
                //    paging.CurrentPage, paging.TotalPages);
                //foreach (var p in paging.Items) {
                //    Console.WriteLine(p);
                //}

                var pagings = jobRepository.All.EnumPaging(r => r.Name, 100, false);
                foreach (var paging in pagings) {
                    Console.WriteLine("Paging {0}/{1}",
                        paging.CurrentPage, paging.TotalPages);
                    foreach (var p in paging.Items) {
                        Console.WriteLine(p);
                    }
                    break;
                }
            }
        }

        private static void NHibernateRepositoryCURD() {
            var factory = BuildSessionFactory();
            using (var context = new NHibernateRepositoryContext(factory)) {
                //var query = context.EnsureSession().CreateSQLQuery("SELECT Id, Name AS Title, JobId AS Salary FROM dbo.Employee");
                //var jobs = query.UniqueResult<Job>();

                var jobRepository = new NHibernateRepository<Job>(context);
                var employeeRepository = new NHibernateRepository<Employee>(context);
                foreach (var entry in jobRepository.All) {
                    jobRepository.Delete(entry);
                }
                foreach (var entry in employeeRepository.All) {
                    employeeRepository.Delete(entry);
                }

                var CShape = new Job {
                    Title = "C#", Salary = 4
                };
                jobRepository.Create(CShape);
                var Java = new Job {
                    Title = "Java", Salary = 5
                };
                jobRepository.Create(Java);
                var Javascript = new Job {
                    Title = "Javascript", Salary = 3
                };
                jobRepository.Create(Javascript);

                var Aimee = new Employee {
                    Name = "Aimee", Address = "Los Angeles", Birth = DateTime.Now,
                    Job = CShape
                };
                employeeRepository.Create(Aimee);
                var Becky = new Employee {
                    Name = "Becky", Address = "Bejing", Birth = DateTime.Now,
                    Job = Java
                };
                employeeRepository.Create(Becky);
                var Carmen = new Employee {
                    Name = "Carmen", Address = "Salt Lake City", Birth = DateTime.Now,
                    Job = Javascript
                };
                employeeRepository.Create(Carmen);

                Console.WriteLine("Employee all");
                foreach (var entry in employeeRepository.All) {
                    Console.WriteLine("{0,-10} {1} {2}",
                        entry.Name, entry.Job.Salary, entry.Address);
                }
                Console.WriteLine();

                Carmen = employeeRepository.Retrive(Carmen.Id);
                Carmen.Job = Java;
                employeeRepository.Update(Carmen);

                Console.WriteLine("Employee live in USA");
                foreach (var entry in employeeRepository.Retrive("Address", new[] { "Los Angeles", "Salt Lake City" })) {
                    Console.WriteLine("{0,-10} {1} {2}",
                       entry.Name, entry.Job.Salary, entry.Address);
                }
                Console.WriteLine();

                employeeRepository.Delete(Carmen);
                Console.WriteLine("Employee left {0}", employeeRepository.All.Count());
            }
        }

        private static void CountingNoDispose(ISessionFactory dbFactory) {
            Task.Factory.StartNew(() => {
                new Task(() => {
                    for (int i = 0; i < 100; i++) {
                        var dbContext = new NHibernateRepositoryContext(dbFactory);
                        var repository = new NHibernateRepository<Employee>(dbContext);
                        //dbContext.Begin();
                        var list = repository.All.ToArray();
                    }
                }, TaskCreationOptions.AttachedToParent).Start();
            }).Wait();
        }

        private static void CountingWithDispose(ISessionFactory dbFactory) {
            Task.Factory.StartNew(() => {
                new Task(() => {
                    for (int i = 0; i < 100; i++) {
                        using (var dbContext = new NHibernateRepositoryContext(dbFactory)) {
                            var repository = new NHibernateRepository<Employee>(dbContext);
                            //dbContext.Begin();
                            var list = repository.All.ToArray();
                        }
                    }
                }, TaskCreationOptions.AttachedToParent).Start();
            }).Wait();
        }

        private static ISessionFactory BuildSessionFactory() {
            var dbConStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var dbFluentConfig = Fluently.Configure()
                   .Database(MsSqlConfiguration.MsSql2012.ConnectionString(dbConStr))
                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Program>());
            var dbConfig = dbFluentConfig.BuildConfiguration();
            dbConfig.SetInterceptor(new NHibernateInterceptor());
            return dbConfig.BuildSessionFactory();
        }

        #endregion

        #region MongoDB

        private static void PrepareMongo() {
            var conStr = ConfigurationManager.ConnectionStrings["Local"].ConnectionString;
            var context = new MongoRepositoryContext(conStr, "Comment");
            var repository = new MongoRepository<Employee>(context);
            var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin"
               .Split('、', ' ');
            for (int i = 0; i < names.Length; i++) {
                var entry = new Employee {
                    Name = names[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                    Job = new Job {
                        Title = Guid.NewGuid().ToString().Substring(0, 8),
                        Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000)
                    }
                };
                repository.Create(entry);
            }
            foreach (var entry in repository.All.Where(r => r.Job.Salary > 3000)) {
                Console.WriteLine("{0,-10} {1}", entry.Name, entry.Job.Salary);
            }
        }

        private static void MongoRepositoryCURD() {
            var conStr = ConfigurationManager.ConnectionStrings["Local"].ConnectionString;
            var context = new MongoRepositoryContext(conStr, "Comment");
            var repository = new MongoRepository<Employee>(context);

            foreach (var entry in repository.All) {
                repository.Delete(entry);
            }

            var Aimee = new Employee {
                Name = "Aimee", Address = "Los Angeles", Birth = DateTime.Now,
                Job = new Job {
                    Title = "C#", Salary = 4
                }
            };
            repository.Create(Aimee);
            var Becky = new Employee {
                Name = "Becky", Address = "Bejing", Birth = DateTime.Now,
                Job = new Job {
                    Title = "Java", Salary = 5
                }
            };
            repository.Create(Becky);
            var Carmen = new Employee {
                Name = "Carmen", Address = "Salt Lake City", Birth = DateTime.Now,
                Job = new Job {
                    Title = "Javascript", Salary = 3
                }
            };
            repository.Create(Carmen);

            Console.WriteLine("Employee all");
            foreach (var entry in repository.All) {
                Console.WriteLine("{0,-10} {1} {2}",
                    entry.Name, entry.Job.Salary, entry.Address);
            }
            Console.WriteLine();

            Carmen = repository.Retrive(Carmen.Id);

            Carmen.Job.Title = "Java";
            Carmen.Job.Salary = 5;
            repository.Update(Carmen);

            Console.WriteLine("Employee live in USA");
            foreach (var entry in repository.Retrive("Address", new[] { "Los Angeles", "Salt Lake City" })) {
                Console.WriteLine("{0,-10} {1} {2}",
                   entry.Name, entry.Job.Salary, entry.Address);
            }
            Console.WriteLine();

            repository.Delete(Carmen);
            Console.WriteLine("Employee left {0}", repository.All.Count());
        }

        private static void UpsertTest() {
            var conStr = ConfigurationManager.ConnectionStrings["Local"].ConnectionString;
            var context = new MongoRepositoryContext(conStr, "Comment");
            var repository = new MongoRepository<Employee>(context);
            var docs = context.DatabaseFactory().GetCollection<Employee>("Employee");

            Console.WriteLine("Remove all");
            docs.RemoveAll();

            var Aimee = new Employee {
                Name = "Aimee", Address = "Los Angeles", Birth = DateTime.Now,
                Job = new Job {
                    Title = "C#", Salary = 4
                }
            };
            Console.WriteLine("Insert Aimee");
            repository.Create(Aimee);

            var Becky = new Employee {
                Name = "Becky", Address = "Bejing", Birth = DateTime.Now,
                Job = new Job {
                    Title = "Java", Salary = 5
                }
            };
            Console.WriteLine("Insert Becky");
            repository.Create(Becky);

            var Carmen = new Employee {
                Name = "Carmen", Address = "Salt Lake City", Birth = DateTime.Now,
                Job = new Job {
                    Title = "Javascript", Salary = 3
                }
            };
            Console.WriteLine("Insert Carmen");
            repository.Create(Carmen);

            Aimee.Job.Title = "C++";
            Console.WriteLine("Insert Aimee");
            repository.Update(Aimee);
        }

        #endregion
    }
}
