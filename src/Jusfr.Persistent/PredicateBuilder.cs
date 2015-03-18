using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent {
    public static partial class PredicateBuilder {
        public static Paging<TEntry> Paging<TEntry>(this IQueryable<TEntry> query, Int32 currentPage = 1, Int32 itemsPerpage = 10) {
            Int32 skip = Math.Max((currentPage - 1) * itemsPerpage, 0);
            return new Paging<TEntry>(
                query.Skip(skip).Take(itemsPerpage),
                currentPage,
                itemsPerpage,
                query.LongCount());
        }

        public static IEnumerable<Paging<TEntry>> EnumPaging<TEntry>(this IQueryable<TEntry> query, Int32 itemsPerpage = 100) {
            var paging = Paging(query, 1, itemsPerpage);
            while (paging.CurrentPage <= paging.TotalPages) {
                yield return paging;
                paging = Paging(query, paging.CurrentPage + 1, itemsPerpage);
            }
        }

        //按属性唯一性过滤
        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector) {
            var dict = new ConcurrentDictionary<TKey, Object>();
            return source.Where(item => dict.TryAdd(selector(item), null));
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IEqualityComparer<TKey> comparer) {
            var dict = new ConcurrentDictionary<TKey, Object>(comparer);
            return source.Where(item => dict.TryAdd(selector(item), null));
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, String propertyName) {
            return QueryableHelper<T>.OrderBy(queryable, propertyName, false);
        }
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, String propertyName, bool desc) {
            return QueryableHelper<T>.OrderBy(queryable, propertyName, desc);
        }

        internal static class QueryableHelper<T> {
            private static readonly ConcurrentDictionary<String, LambdaExpression> cache
                = new ConcurrentDictionary<String, LambdaExpression>();

            public static IQueryable<T> OrderBy(IQueryable<T> queryable, string propertyName, bool desc) {
                LambdaExpression keySelector = GetLambdaExpression(propertyName);
                var query = desc
                    ? Queryable.OrderByDescending(queryable, (dynamic)keySelector)
                    : Queryable.OrderBy(queryable, (dynamic)keySelector);
                return (IQueryable<T>)query;
            }

            private static LambdaExpression GetLambdaExpression(string propertyName) {
                return cache.GetOrAdd(propertyName, prop => {
                    var param = Expression.Parameter(typeof(T));
                    var body = Expression.Property(param, prop);
                    var keySelector = Expression.Lambda(body, param);
                    return keySelector;
                });
            }
        }
    }
}
