using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jusfr.Persistent.NHibernate;

namespace PersistentDemo.MySql {
    class Program {
        internal static void Retrive_via_primaryKey_medium_scale() {
            using (var context = new PubsContext()) {
                var repo = new NHibernateRepository<Roysched>(context);
                var allKeys = repo.All.Select(x => x.Title_id).ToArray();
                Console.WriteLine("Keys.Length={0}", allKeys.Length);
                var allItems = repo.Retrive(x => x.Title_id, allKeys).ToArray();
                Console.WriteLine("Items.Length={0}", allItems.Length);
                allItems = repo.All.Where(r => allKeys.Contains(r.Title_id)).ToArray();
                Console.WriteLine("Items.Length={0}", allItems.Length);
            }
        }

        internal static void Save_entity_new_and_exists() {
            var theOne = new Job {
                Job_id = (short)Guid.NewGuid().GetHashCode(),
                Job_desc = Guid.NewGuid().ToString(),
            };

            using (var context = new PubsContext()) {
                var repo = new NHibernateRepository<Job>(context);
                repo.Save(theOne);

                theOne.Min_lvl++;
                repo.Update(theOne);
                //context.Flush();
            }

            using (var context = new PubsContext()) {
                var repo = new NHibernateRepository<Job>(context);
                theOne.Max_lvl--;
                repo.Save(theOne);
                //context.Flush();
            }
        }

        internal static void Insert_with_dapper() {
            var stopwatch = Stopwatch.StartNew();
            const Int32 count = 1000;

            using (var db = new PetaPoco.Database("PubsMysql")) {
                db.BeginTransaction();
                var maxId = db.ExecuteScalar<Int32>("select ifnull( max(id),0) from person;");
                stopwatch.Start();
                for (int i = 0; i < count; i++) {
                    var person = new Person {
                        Id = ++maxId,
                        Name = Guid.NewGuid().ToString().Substring(0, 8),
                        Address = Guid.NewGuid().ToString(),
                        Birth = DateTime.Now,
                        Job_id = Math.Abs(Guid.NewGuid().GetHashCode() % 100)
                    };
                    db.Insert(person);
                }
                db.CompleteTransaction();
            }
            stopwatch.Stop();
            Console.WriteLine("Dapper insert {0}, take {1} sec., {2:f2}/sec.",
                count, stopwatch.Elapsed, count / stopwatch.Elapsed.TotalSeconds);
        }

        internal static void Insert_with_nhibernate() {
            const Int32 count = 1000;
            var stopwatch = Stopwatch.StartNew();

            using (var context = new PubsContext()) {
                stopwatch.Restart(); // delay record
                context.Begin();
                var repo = new NHibernateRepository<Person>(context);
                var maxId = repo.All.Max(x => x.Id);
                for (int i = 0; i < count; i++) {
                    var person = new Person {
                        Id = ++maxId,
                        Name = Guid.NewGuid().ToString().Substring(0, 8),
                        Address = Guid.NewGuid().ToString(),
                        Birth = DateTime.Now,
                        Job_id = Math.Abs(Guid.NewGuid().GetHashCode() % 100)
                    };
                    repo.Create(person);
                }
                //context.Flush();
                context.Commit();
            }
            stopwatch.Stop();
            Console.WriteLine("Nhibernate insert {0}, take {1} sec., {2:f2}/sec.",
                count, stopwatch.Elapsed, count / stopwatch.Elapsed.TotalSeconds);
        }
    }
}
