using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Jusfr.Persistent.Mongo {
    public class MongoRepository<TEntry> : MongoRepository<TEntry, Int32> where TEntry : class, IAggregate<Int32> {
        public MongoRepository(IRepositoryContext context)
         : base(context) {
        }
    }
}