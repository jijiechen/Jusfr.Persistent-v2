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

namespace Demo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //var context = BuildMongoRepositoryContext();
            //var repository = new MongoRepository<Employee>(context);

            BulkCopyTest();
        }

        private static void BulkCopyTest() {
            var conStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var bcpHelper = new BulkCopyHelper(conStr);
            bcpHelper.Insert(typeof(Employee).Name, GetEmployee(100000L));
        }

        static IEnumerable<Employee> GetEmployee(Int64 count) {
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

        #region NHibernate

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

        private static ISessionFactory PagingDemo() {
            var factory = BuildSessionFactory();
            using (var context = new NHibernateRepositoryContext(factory)) {
                var jobRepository = new NHibernateRepository<Job>(context);

                //context.EnsureSession().CreateSQLQuery("TRUNCATE TABLE [Job]").ExecuteUpdate();
                //for (int i = 0; i < 110; i++) {
                //    jobRepository.Create(new Job {
                //        Title = Guid.NewGuid().ToString("n").Substring(0, 8),
                //        Salary = Guid.NewGuid().GetHashCode(),
                //    });
                //}

                var pagings = jobRepository.All.EnumPaging(20, true);
                foreach (var paging in pagings) {
                    Console.WriteLine("Paging {0}/{1}, Items {2}",
                        paging.CurrentPage, paging.TotalPages, paging.Items.Count());
                    paging.CurrentPage = 100;
                }
            }
            return factory;
        }

        private static void NHibernateRepositoryCURD() {
            var factory = BuildSessionFactory();
            using (var context = new NHibernateRepositoryContext(factory)) {
                //var query = context.EnsureSession().CreateSQLQuery("SELECT Id, Name AS Title, JobId AS Salary FROM dbo.Employee");
                //var jobs = query.UniqueResult<Job>();

                var jobRepository = new NHibernateRepository<Job>(context);
                var employeeRepository = new NHibernateRepository<Employee>(context);
                foreach (var entry in ((IQueryRepository<Job>)jobRepository).All) {
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

        private static ISessionFactory BuildSessionFactory() {
            var dbConStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var dbFluentConfig = Fluently.Configure()
                   .Database(MsSqlConfiguration.MsSql2012.ConnectionString(dbConStr))
                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Program>());
            var dbConfig = dbFluentConfig.BuildConfiguration();
            dbConfig.SetInterceptor(new NHibernateInterceptor());
            return dbConfig.BuildSessionFactory();
        }

        private static void CountingNoDispose(ISessionFactory dbFactory) {
            Task.Factory.StartNew(() => {
                new Task(() => {
                    for (int i = 0; i < 100; i++) {
                        var dbContext = new NHibernateRepositoryContext(dbFactory);
                        var repository = new NHibernateRepository<Employee>(dbContext);
                        var query = repository as IQueryRepository<Employee>;
                        //dbContext.Begin();
                        var list = query.All.ToArray();
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
                            var query = repository as IQueryRepository<Employee>;
                            //dbContext.Begin();
                            var list = query.All.ToArray();
                        }
                    }
                }, TaskCreationOptions.AttachedToParent).Start();
            }).Wait();
        }

        private static void PrepareMssql(ISessionFactory factory) {
            var Employees = Enumerable.Range(0, 60000)
                .Select(i => new Employee {
                    Name = Guid.NewGuid().ToString(),
                    Birth = DateTime.UtcNow.AddDays(Guid.NewGuid().GetHashCode() % 365),
                    Address = Guid.NewGuid().ToString(),
                    Job = null,
                });
            using (var dbContext = new NHibernateRepositoryContext(factory)) {
                var repository = new NHibernateRepository<Employee>(dbContext);
                foreach (var Employee in Employees) {
                    repository.Create(Employee);
                }
            }
        }

        #endregion

        #region MongoDB

        private static MongoRepositoryContext BuildMongoRepositoryContext() {
            var conStr = "mongodb://localhost";
            var database = "migrate";
            return new MongoRepositoryContext(conStr, database);
        }

        private static void PrepareMongo() {
            var context = BuildMongoRepositoryContext();
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

        private static void MongoRepositoryCURD(IRepository<Employee> repository) {
            foreach (var entry in ((IQueryRepository<Employee>)repository).All) {
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
            foreach (var entry in ((IQueryRepository<Employee>)repository).All) {
                Console.WriteLine("{0,-10} {1} {2}",
                    entry.Name, entry.Job.Salary, entry.Address);
            }
            Console.WriteLine();

            Carmen = ((IQueryRepository<Employee>)repository).Retrive(Carmen.Id);

            Carmen.Job.Title = "Java";
            Carmen.Job.Salary = 5;
            repository.Update(Carmen);

            Console.WriteLine("Employee live in USA");
            foreach (var entry in ((IQueryRepository<Employee>)repository).Retrive("Address", new[] { "Los Angeles", "Salt Lake City" })) {
                Console.WriteLine("{0,-10} {1} {2}",
                   entry.Name, entry.Job.Salary, entry.Address);
            }
            Console.WriteLine();

            repository.Delete(Carmen);
            Console.WriteLine("Employee left {0}", ((IQueryRepository<Employee>)repository).All.Count());
        }

        #endregion
    }
}
