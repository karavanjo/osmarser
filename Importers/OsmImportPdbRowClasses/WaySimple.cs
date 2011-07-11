using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmImportToSqlServer.OsmData;

namespace OsmImportToSqlServer.Importers.OsmImportPdbRowClasses
{
    public class WaySimple : OsmPrimitive
    {
        public WaySimple(Int64 id, DateTime dateTime)
        {
            base._id = id;
            this._nodesSimple = new List<NodeSimple>();
            base._dateStamp = dateTime;
        }

        public void AddNode(NodeSimple nodeSimple)
        {
            _nodesSimple.Add(nodeSimple);
        }

        public bool IsPolygon()
        {
            if (_nodesSimple.First().Id == _nodesSimple.Last().Id && _nodesSimple.Count > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<NodeSimple> Nodes
        {
            get { return _nodesSimple; }
        }

        private List<NodeSimple> _nodesSimple;
    }
}
