using System;
using System.Collections.Generic;
using System.Linq;
using Jusfr.Persistent.NHibernate;
using PersistentDemo.MySql;
using Xunit;

namespace Jusfr.Persistent.Tests {
    public class NHibernateRepositoryTest {
        private const Byte Max_lvl_HasValue = (Byte)100;
        private readonly PubsContext _context;

        public NHibernateRepositoryTest() {
            _context = new PubsContext();
        }

        [Fact]
        public void Retrive_via_primaryKey() {
            var repo = new NHibernateRepository<Job>(_context);
            var theFirstOne = repo.Retrive((Int16)1);
        }

        [Fact]
        public void Retrive_via_primaryKey_list() {
            var repo = new NHibernateRepository<Roysched>(_context);
            var allKeys = repo.All.Select(x => x.Title_id).ToArray();
            var allItems = repo.Retrive(allKeys).ToArray();

            Assert.NotEmpty(allKeys);
            Assert.Equal(allKeys.Length, allItems.Length);
        }

        [Fact]
        public void Retrive_via_field() {
            var repo = new NHibernateRepository<Job>(_context);
            var theSpecials = repo.Retrive("Max_lvl", (Byte)Max_lvl_HasValue);
            Assert.NotEmpty(theSpecials);
        }

        [Fact]
        public void Retrive_via_field_list() {
            var repo = new NHibernateRepository<Roysched>(_context);
            var allKeys = repo.All.Select(x => x.Title_id).ToArray();
            var allItems = repo.Retrive(x => x.Title_id, allKeys).ToArray();

            Assert.NotEmpty(allKeys);
            Assert.Equal(allKeys.Length, allItems.Length);
        }

        [Fact]
        public void Retrive_via_expression() {
            var repo = new NHibernateRepository<Job>(_context);
            var theSpecials = repo.Retrive(j => j.Max_lvl, (Byte)Max_lvl_HasValue);
            Assert.NotEmpty(theSpecials);
        }

        [Fact]
        public void Retrive_via_expression_contains() {
            var repo = new NHibernateRepository<Roysched>(_context);
            var allKeys = repo.All.Select(x => x.Title_id).ToArray();
            var allItems = repo.All.Where(r => allKeys.Contains(r.Title_id)).ToArray();

            Assert.NotEmpty(allKeys);
            Assert.Equal(allKeys.Length, allItems.Length);
        }

        [Fact]
        public void Retrive_via_queryable() {
            var repo = new NHibernateRepository<Job>(_context);
            var query = repo.All.Where(r => r.Max_lvl == Max_lvl_HasValue);
            var theSpecials = repo.Fetch(_ => query);
            Assert.NotEmpty(theSpecials);
        }
    }
}
