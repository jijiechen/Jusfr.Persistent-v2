using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Jusfr.Persistent;
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
            var dbConStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var dbFluentConfig = Fluently.Configure()
                   .Database(MsSqlConfiguration.MsSql2012.ConnectionString(dbConStr))
                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Program>());
            var dbConfig = dbFluentConfig.BuildConfiguration();
            //dbConfig.SetInterceptor(new SqlStatementInterceptor());
            var dbFactory = dbConfig.BuildSessionFactory();

            //CountingWithDispose(dbFactory);
            CRUD(dbFactory);
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
    }
}
