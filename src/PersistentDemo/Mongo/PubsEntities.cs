using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Jusfr.Persistent.Mongo;

namespace PersistentDemo.Mongo {
    public class Job: IMongoId {
        public ObjectId Id { get; set; }
        public virtual Int16 Job_id { get; set; }
        public virtual String Job_desc { get; set; }
        public virtual Byte Min_lvl { get; set; }
        public virtual Byte Max_lvl { get; set; }
    }

    public class Roysched : IMongoId<String> {
        public virtual String Id { get; set; }
        public virtual Int32? Lorange { get; set; }
        public virtual Int32? Hirange { get; set; }
        public virtual Int32? Royalty { get; set; }
    }
}
