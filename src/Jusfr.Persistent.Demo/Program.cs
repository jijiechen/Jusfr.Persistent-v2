using Jusfr.Persistent.Mongo;
using Jusfr.Persistent.NH;
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
            Dupliate_entity_update_need_evict();
        }

        private static void Dupliate_entity_update_need_evict() {
            var context = new PubsContext();
            var jobRepo = new NHibernateRepository<Job>(context);

            var j1 = jobRepo.Retrive(1);
            var j2 = jobRepo.Retrive(1);
            Console.WriteLine("j1 == j2? {0}", j1 == j2); // True
            
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
            catch (NHibernate.NonUniqueObjectException ex){
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Got NHibernate.NonUniqueObjectException with Entity {0}", ex.EntityName);
                Console.ResetColor();
            }

            var session = context.EnsureSession();
            Console.WriteLine("EntityCount before evict {0}", session.Statistics.EntityCount);
            session.Evict(j1);  //移除j1的游离态，对j3的更新操作才能完成
            Console.WriteLine("EntityCount after evict {0}", session.Statistics.EntityCount);

            jobRepo.Update(j3); //Pass
            Console.WriteLine("j1.Salary: {0}, j3.Salary: {1}", j1.Salary, j3.Salary);
            // j1.Salary: 2760.00000, j3.Salary: 2761.00000
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
