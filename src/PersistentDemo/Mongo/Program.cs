using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jusfr.Persistent.Mongo;
using MongoDB.Bson;

namespace PersistentDemo.Mongo {
    class Program {
        internal static void Retrive_via_primaryKey_medium_scale() {
            using (var context = new PubsContext()) {
                var repo = new MongoRepository<Roysched, String>(context);
                var allKeys = repo.All.Select(x => x.Id).ToArray();
                Console.WriteLine("Keys.Length={0}", allKeys.Length);
                var allItems = repo.Retrive(x => x.Id, allKeys).ToArray();
                Console.WriteLine("Items.Length={0}", allItems.Length);
                allItems = repo.All.Where(r => allKeys.Contains(r.Id)).ToArray();
                Console.WriteLine("Items.Length={0}", allItems.Length);
            }
        }

        internal static void Save_entity_new_and_exists() {
            var theOne = new Job {
                Id = ObjectId.GenerateNewId(),
                Job_id = (short)Guid.NewGuid().GetHashCode(),
                Job_desc = Guid.NewGuid().ToString(),
            };

            using (var context = new PubsContext()) {
                var repo = new MongoRepository<Job>(context);
                repo.Save(theOne);

                theOne.Min_lvl++;
                repo.Update(theOne);
            }

            using (var context = new PubsContext()) {
                var repo = new MongoRepository<Job>(context);
                theOne.Max_lvl--;
                repo.Save(theOne);
            }
        }

    }
}
