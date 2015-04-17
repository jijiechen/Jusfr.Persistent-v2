using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent.Mongo {

    public class BsonDocumentDeclarationAttribute : Attribute {
        public String Document { get; private set; }
        public BsonDocumentDeclarationAttribute(String document) {
            Document = document;
        }
    }
}
