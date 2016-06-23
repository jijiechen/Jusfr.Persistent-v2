using System;
using System.Collections.Generic;
using System.Linq;
using Jusfr.Persistent.Mongo;
using MongoDB.Bson;
using PersistentDemo.Mongo;
using Xunit;

namespace Jusfr.Persistent.Tests {
    public class MongoRepositoryTest_ {
        private const Int32 Hirange = 2000;
        private readonly PubsContext _context;

        public MongoRepositoryTest_() {
            _context = new PubsContext();
            _context.Database.DropCollection("Roysched");
            var repo = new MongoRepository<Roysched, String>(_context);
            repo.Create(new Roysched {
                Id = "PC8888",
                Hirange = Hirange
            });
        }

        [Fact]
        public void Retrive_via_primaryKey() {
            var repo = new MongoRepository<Roysched, String>(_context);
            var theFirstOne = repo.Retrive("PC8888");
            Assert.NotNull(theFirstOne);
        }

        [Fact]
        public void Retrive_via_primaryKey_list() {
            var repo = new MongoRepository<Roysched, String>(_context);
            var allKeys = repo.All.Select(x => x.Id).ToArray();
            var allItems = repo.Retrive(allKeys).ToArray();

            Assert.NotEmpty(allKeys);
            Assert.Equal(allKeys.Length, allItems.Length);
        }

        [Fact]
        public void Retrive_via_field() {
            var repo = new MongoRepository<Roysched, String>(_context);
            var theSpecials = repo.Retrive("Hirange", Hirange);
            Assert.NotEmpty(theSpecials);
        }

        [Fact]
        public void Retrive_via_field_list() {
            var repo = new MongoRepository<Roysched, String>(_context);
            var allKeys = repo.All.Select(x => x.Id).ToArray();
            var allItems = repo.Retrive(x => x.Id, allKeys).ToArray();

            Assert.NotEmpty(allKeys);
            Assert.Equal(allKeys.Length, allItems.Length);
        }

        [Fact]
        public void Retrive_via_expression() {
            var repo = new MongoRepository<Roysched, String>(_context);
            var theSpecials = repo.Retrive(j => j.Hirange, Hirange);
            Assert.NotEmpty(theSpecials);
        }

        [Fact]
        public void Retrive_via_expression_contains() {
            var repo = new MongoRepository<Roysched, String>(_context);
            var allKeys = repo.All.Select(x => x.Id).ToArray();
            var allItems = repo.All.Where(r => allKeys.Contains(r.Id)).ToArray();

            Assert.NotEmpty(allKeys);
            Assert.Equal(allKeys.Length, allItems.Length);
        }

        [Fact]
        public void Retrive_via_queryable() {
            var repo = new MongoRepository<Roysched, String>(_context);
            var query = repo.All.Where(r => r.Hirange == Hirange);
            var theSpecials = repo.Fetch(_ => query);
            Assert.NotEmpty(theSpecials);
        }
    }
}
