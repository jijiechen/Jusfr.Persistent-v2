using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Jusfr.Persistent.Mongo;
using MongoDB.Driver;

namespace PersistentDemo.Mongo {
    public class PubsContext : MongoRepositoryContext {
        private static MongoUrl _mongoUrl;

        static PubsContext() {
            var url = ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString;
            _mongoUrl = new MongoUrl(url);
        }

        public PubsContext()
            : base(_mongoUrl) {
        }
    }
}
