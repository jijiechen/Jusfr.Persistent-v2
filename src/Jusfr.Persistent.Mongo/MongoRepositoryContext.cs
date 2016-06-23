using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Jusfr.Persistent.Mongo {
    public class MongoRepositoryContext : IRepositoryContext {
        private readonly Guid _id;
        private readonly IMongoDatabase _database;

        public bool DistributedTransactionSupported {
            get { return false; }
        }

        public Guid ID {
            get { return _id; }
        }
        public IMongoDatabase Database {
            get { return _database; }
        }

        public MongoRepositoryContext(MongoUrl mongoUrl) {
            _id = Guid.NewGuid();
            var client = new MongoClient(mongoUrl);
            _database = client.GetDatabase(mongoUrl.DatabaseName);
        }

        public void Begin() {
            throw new NotImplementedException();
        }

        public void Commit() {
            throw new NotImplementedException();
        }

        public void Dispose() {

        }

        public void Rollback() {
            throw new NotImplementedException();
        }
    }
}
