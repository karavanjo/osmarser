using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmImportToSqlServer.Importers.OsmImportPdbRowClasses
{
    public class NodeSimple
    {
        private Int64 _id;

        public NodeSimple(Int64 id)
        {
            this._id = id;
        }

        public double Latitude { get; set; }
        public double Longtitude { get; set; }
    }
}
