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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            var context = GetMongoRepositoryContext();
            var repository = new MongoRepository<Person>(context);
            var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin"
                .Split('、', ' ');

            var alax = new Person { Name = "Alax", Job = new Job { Title = "C#", Salary = 4000 } };
            repository.Create(alax);
            Console.WriteLine("Alax, {0} {1}", alax.Job.Title, alax.Job.Salary);

            alax.Job.Title = "Java";
            alax.Job.Salary = 4500;
            repository.Update(alax);
            foreach (var entry in repository.All) {
                Console.WriteLine("{0,-10} {1}", entry.Name, entry.Job.Salary);
            }

            repository.Delete(alax);
            Console.WriteLine(repository.All.Count());


            //for (int i = 0; i < names.Length; i++) {
            //    var entry = new Person {
            //        Name = names[i],
            //        Address = Guid.NewGuid().ToString().Substring(0, 8),
            //        Birth = DateTime.UtcNow,
            //        Job = new Job {
            //            Title = Guid.NewGuid().ToString().Substring(0, 8),
            //            Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000)
            //        }
            //    };
            //    repository.Create(entry);
            //}
            //foreach (var entry in repository.All.Where(r => r.Job.Salary > 3000)) {
            //    Console.WriteLine("{0,-10} {1}", entry.Name, entry.Job.Salary);
            //}
        }

        #region MongoDB

        private static MongoRepositoryContext GetMongoRepositoryContext() {
            var conStr = "mongodb://localhost";
            var database = "migrate";
            return new MongoRepositoryContext(conStr, database);
        }

        #endregion

        #region NHibernate

        private static ISessionFactory GetNHibernateSessionFactory() {
            var dbConStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var dbFluentConfig = Fluently.Configure()
                   .Database(MsSqlConfiguration.MsSql2012.ConnectionString(dbConStr))
                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Program>());
            var dbConfig = dbFluentConfig.BuildConfiguration();
            //dbConfig.SetInterceptor(new SqlStatementInterceptor());
            var dbFactory = dbConfig.BuildSessionFactory();
            return dbFactory;
        }

        private static void CRUD(ISessionFactory dbFactory) {
            var entry = new Person {
                Name = Guid.NewGuid().ToString().Substring(0, 8),
                Birth = DateTime.UtcNow.AddDays(Guid.NewGuid().GetHashCode() % 365),
                Address = Guid.NewGuid().ToString().Substring(0, 8),
                Job = null,
            };

            using (var dbContext = new NHibernateRepositoryContext(dbFactory)) {
                var repository = new NHibernateRepository<Person>(dbContext);
                Console.WriteLine("New Entry with name {0}", entry.Name);
                repository.Create(entry);
                Console.WriteLine("Entry created with id {0}", entry.Id);

                entry.Name = Guid.NewGuid().ToString().Substring(0, 8);
                repository.Update(entry);

                entry = repository.Retrive(entry.Id);
                Console.WriteLine("Entry updated with name {0}", entry.Name);

                Console.WriteLine("Paging Entry");
                var page = repository.All.OrderByDescending(r => r.Id).Paging(1, 10);
                foreach (var item in page.Items) {
                    Console.WriteLine("{0} {1}", item.Id, item.Name);
                }
            }
        }

        private static void CountingNoDispose(ISessionFactory dbFactory) {
            Task.Factory.StartNew(() => {
                new Task(() => {
                    for (int i = 0; i < 100; i++) {
                        var dbContext = new NHibernateRepositoryContext(dbFactory);
                        var repository = new NHibernateRepository<Person>(dbContext);
                        var query = repository as IQueryRepository<Person>;
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
                            var repository = new NHibernateRepository<Person>(dbContext);
                            var query = repository as IQueryRepository<Person>;
                            //dbContext.Begin();
                            var list = query.All.ToArray();
                        }
                    }
                }, TaskCreationOptions.AttachedToParent).Start();
            }).Wait();
        }

        private static void PrepareData(ISessionFactory factory) {
            var persons = Enumerable.Range(0, 60000)
                .Select(i => new Person {
                    Name = Guid.NewGuid().ToString(),
                    Birth = DateTime.UtcNow.AddDays(Guid.NewGuid().GetHashCode() % 365),
                    Address = Guid.NewGuid().ToString(),
                    Job = null,
                });
            using (var dbContext = new NHibernateRepositoryContext(factory)) {
                var repository = new NHibernateRepository<Person>(dbContext);
                foreach (var person in persons) {
                    repository.Create(person);
                }
            }
        }

        #endregion
    }
}
