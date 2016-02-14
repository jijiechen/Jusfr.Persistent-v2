﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Jusfr.Persistent.Mongo {

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

    public class MongoEntryMapperFactory {
        public static IMongoEntryMapper Default = new MongoEntryMapper();

        //todo, implement
    }
}
