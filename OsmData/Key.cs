using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmImportToSqlServer.Config;

namespace OsmImportToSqlServer.OsmData
{
    public class Key
    {
        public int Id { get; set; }
        public TypeValueTag TypeValue { get; set; }
    }
}
