using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent {
    public class Paging {
        public Int32 CurrentPage { get; set; }
        public Int32 ItemsPerPage { get; set; }
        public Int64 TotalItems { get; set; }
        public Int32 TotalPages { get; set; }

    }

    public class Paging<T> : Paging {
        public IList<T> Items { get; set; }

        public Paging(IList<T> items, Int32 currentPage, Int32 itemsPerPage, Int64 totalItems) {
            Items = items;
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalItems = totalItems;
            TotalPages = (Int32)Math.Ceiling((Double)totalItems / itemsPerPage);
        }

        public Paging(IList<T> items, Paging pageBase) {
            Items = items;
            CurrentPage = pageBase.CurrentPage;
            ItemsPerPage = pageBase.ItemsPerPage;
            TotalItems = pageBase.TotalItems;
            TotalPages = pageBase.TotalPages;
        }
    }
}
