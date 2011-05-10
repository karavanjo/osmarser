using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmImportToSqlServer.OsmData
{
    public class MemberRelation
    {
        public MemberRelation(MemberRelationType type, int roleId, long refId)
        {
            
        }

        public MemberRelationType Type { get; set; }
        public int RoleId { get; set; }
        public long Ref { get; set; }
    }
}
