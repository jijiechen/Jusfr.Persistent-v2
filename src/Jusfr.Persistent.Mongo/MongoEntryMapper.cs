using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Jusfr.Persistent.Mongo {
    public class BsonDocumentDeclarationAttribute : Attribute {
        public String Document { get; private set; }
        public BsonDocumentDeclarationAttribute(String document) {
            Document = document;
        }
    }

    public interface IMongoEntryMapper {
        String Map(Object entry);
        String Map<TEntry>();
    }

    //Basic mapper
    public class MongoEntryMapper : IMongoEntryMapper {

        public String Map(Object entry) {
            return Map(entry.GetType());
        }

        public String Map<TEntry>() {
            var entryType = typeof(TEntry);
            return Map(entryType);
        }

        public String Map(Type entryType) {
            var document = entryType.GetCustomAttribute<BsonDocumentDeclarationAttribute>(false);
            if (document != null) {
                return document.Document;
            }
            return entryType.Name;
        }
    }
}
