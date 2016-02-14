﻿using MongoDB.Driver;
using System;
using System.Text.RegularExpressions;

namespace Jusfr.Persistent.Mongo {
    public class MongoRepositoryContext : DisposableObject, IRepositoryContext {
        private readonly Guid _id;
        private readonly MongoClient _client;

        public IMongoDatabase Database { get; private set; }

        public Guid ID {
            get { return _id; }
        }

        public Boolean DistributedTransactionSupported {
            get { return false; }
        }

        public void Begin() {
            throw new NotImplementedException();
        }

        public void Rollback() {
            throw new NotImplementedException();
        }

        public void Commit() {
            throw new NotImplementedException();
        }

        public MongoRepositoryContext(String mongoUrl) {
            _id = Guid.NewGuid();
            var mongoUri = new MongoUrl(mongoUrl);
            _client = new MongoClient(mongoUri);
            Database = _client.GetDatabase(mongoUri.DatabaseName);
        }
    }
}
