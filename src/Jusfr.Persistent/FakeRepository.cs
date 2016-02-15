﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jusfr.Persistent {

    public class FakeRepository<TEntry> : Repository<TEntry, Int32> where TEntry : class, IAggregate<Int32> {
        private static Int32 _id = 0;
        private readonly List<TEntry> _all = new List<TEntry>();
        private readonly Boolean _autoIncrement;

        protected virtual Int32 CreateNewId(TEntry entry) {
            return Interlocked.Increment(ref _id);
        }

        public FakeRepository()
            : this(true) {
        }

        public FakeRepository(Boolean autoIncrement)
            : base(null) {
            _autoIncrement = autoIncrement;
        }

        public override void Create(TEntry entry) {
            entry.Id = _autoIncrement ? CreateNewId(entry) : entry.Id;
            _all.Add(entry);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Delete(TEntry entry) {
            _all.Remove(entry);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Delete(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                _all.Remove(entry);
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Update(TEntry entry) {
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Update(IEnumerable<TEntry> entries) {
        }

        public override void Save(TEntry entry) {
            if (entry.Id == 0) {
                if (!_autoIncrement) {
                    throw new InvalidOperationException("Auto increment disabled");
                }
                Create(entry);
            }
            else {
                _all.Add(entry);
            }
        }

        public override void Save(IEnumerable<TEntry> entries) {
            entries.ToList().ForEach(Save);
        }

        public override TEntry Retrive(int key) {
            return _all.FirstOrDefault(r => r.Id == key);
        }

        public override IEnumerable<TEntry> Retrive(params Int32[] keys) {
            return _all.Where(r => keys.Contains(r.Id));
        }

        public override IEnumerable<TEntry> Retrive<TMember>(String field, params TMember[] keys) {
            throw new NotImplementedException();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys) {
            var predicate = selector.Compile();
            return _all.Where(r => keys.Contains(predicate(r))).ToList();
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates) {
            IQueryable<TEntry> left = All;
            foreach (var predicate in predicates) {
                left = left.Where(predicate);
            }
            return left.Any();
        }

        public override IQueryable<TEntry> All {
            get { return _all.AsQueryable(); }
        }

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query) {
            return query(_all.AsQueryable());
        }
    }
}
