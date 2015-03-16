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

            CountingWithDispose(dbFactory);
        }

        private static void Crud() {
            var entry = new Person {
                Name = Guid.NewGuid().ToString(),
                Birth = DateTime.UtcNow.AddDays(Guid.NewGuid().GetHashCode() % 365),
                Address = Guid.NewGuid().ToString(),
                Job = null,
            };
        }

        private static void CountingNoDispose(ISessionFactory factory) {
            Task.Factory.StartNew(() => {
                new Task(() => {
                    for (int i = 0; i < 100; i++) {
                        var dbContext = new NHibernateRepositoryContext(factory);
                        var repository = new NHibernateRepository<Person>(dbContext);
                        var query = repository as IQueryRepository<Person>;
                        //dbContext.Begin();
                        var list = query.All.ToArray();
                    }
                }, TaskCreationOptions.AttachedToParent).Start();
            }).Wait();
        }

        private static void CountingWithDispose(ISessionFactory factory) {
            Task.Factory.StartNew(() => {
                new Task(() => {
                    for (int i = 0; i < 100; i++) {
                        using (var dbContext = new NHibernateRepositoryContext(factory)) {
                            var repository = new NHibernateRepository<Person>(dbContext);
                            var query = repository as IQueryRepository<Person>;
                            //dbContext.Begin();
                            var list = query.All.ToArray();
                        }
                    }
                }, TaskCreationOptions.AttachedToParent).Start();
            }).Wait();
        }

        private static void PrepareData(IRepository<Person> repository) {
            var persons = Enumerable.Range(0, 60000)
                .Select(i => new Person {
                    Name = Guid.NewGuid().ToString(),
                    Birth = DateTime.UtcNow.AddDays(Guid.NewGuid().GetHashCode() % 365),
                    Address = Guid.NewGuid().ToString(),
                    Job = null,
                });

            foreach (var person in persons) {
                repository.Create(person);
            }
        }
    }
}
