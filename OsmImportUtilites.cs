using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osm
{
    public class OsmImportUtilites
    {
        public static int GetHash(string hashedString)
        {
            return hashedString.GetHashCode();
        }
    }
}
