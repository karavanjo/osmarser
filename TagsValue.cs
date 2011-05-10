using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmImportToSqlServer
{
    public struct TagValue
    {
        public int TagHash;
        public int ValueHash;
        //public bool IsValueHash;
    }

    public enum TagValueTransType
    {
        OnlyTag,
        OnlyValue,
        TagAndValue
    }
}
