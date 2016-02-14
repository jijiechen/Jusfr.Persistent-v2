﻿using MongoDB.Driver;

namespace Jusfr.Persistent.Mongo {

    public static class MongoDatabaseExtension {
        public static IMongoCollection<TEntry> GetCollection<TEntry>(this IMongoDatabase mongoDatabase) {
            var collectionName = mongoDatabase.CollectionName<TEntry>();
            return mongoDatabase.GetCollection<TEntry>(collectionName);
        }
        
        public static string CollectionName<TEntry>(this IMongoDatabase mongoDatabase) {
            return MongoEntryMapperFactory.Default.Map<TEntry>();
        }

        public static void DropCollection<TEntry>(this IMongoDatabase mongoDatabase) {
            var docs = mongoDatabase.CollectionName<TEntry>();
            mongoDatabase.DropCollection(docs);
        }
    }
}
