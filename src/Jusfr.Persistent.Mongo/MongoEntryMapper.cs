using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent.Mongo {
    public interface IMongoEntryMapper {
        String Map(Object entry);
        String Map<TEntry>();
    }

    //Basic mapper
    public class MongoEntryMapper : IMongoEntryMapper {

        public String Map(Object entry) {
            return entry.GetType().Name;
        }

        public String Map<TEntry>() {
            return typeof(TEntry).Name;
        }
    }
}
