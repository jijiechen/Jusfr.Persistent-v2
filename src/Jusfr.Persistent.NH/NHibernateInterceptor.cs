using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.SqlCommand;

namespace Jusfr.Persistent.NHibernate {
    public class NHibernateInterceptor : EmptyInterceptor, IInterceptor {
        public override SqlString OnPrepareStatement(SqlString sql) {
#if DEBUG
            Debug.WriteLine(sql);
#endif
            return base.OnPrepareStatement(sql);
        }
    }
}
