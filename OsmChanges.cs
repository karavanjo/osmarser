using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmImportToSqlServer.OsmData;

namespace OsmImportToSqlServer
{
    /// <summary>
    /// Class that stores the OSM pimitive (nodes, ways and relations) with changes
    /// </summary>
    public class OsmChanges
    {
        /// <summary>
        /// Added Osm primitive
        /// </summary>
        /// <param name="osmPrimitive">Node, Way or Relation</param>
        /// <param name="changesType">Created, Updated or Deleted</param>
        public void AddOsmPrimitive(OsmPrimitive osmPrimitive, ChangesType changesType)
        {
            Type type = osmPrimitive.GetType();
            if (osmPrimitive is Node)
            {
                switch (changesType)
                {
                    case ChangesType.Created:
                        NodesCreated.Add((Node)osmPrimitive);
                        return;
                        break;
                    case ChangesType.Updated:
                        NodesUpdated.Add((Node)osmPrimitive);
                        return;
                        break;
                    case ChangesType.Deleted:
                        NodesDeleted.Add((Node)osmPrimitive);
                        return;
                        break;
                }
            }

            if (osmPrimitive is Way)
            {
                switch (changesType)
                {
                    case ChangesType.Created:
                        WaysCreated.Add((Way)osmPrimitive);
                        return;
                        break;
                    case ChangesType.Updated:
                        WaysUpdated.Add((Way)osmPrimitive);
                        return;
                        break;
                    case ChangesType.Deleted:
                        WaysDeleted.Add((Way)osmPrimitive);
                        return;
                        break;
                }
            }
        }


        public List<Node> NodesCreated
        {
            get { return _nodesCreated; }
        }

        public List<Node> NodesUpdated
        {
            get { return _nodesUpdated; }
        }

        public List<Node> NodesDeleted
        {
            get { return _nodesDeleted; }
        }

        public List<Way> WaysCreated
        {
            get { return _waysCreated; }
        }

        public List<Way> WaysUpdated
        {
            get { return _waysUpdated; }
        }

        public List<Way> WaysDeleted
        {
            get { return _waysDeleted; }
        }

        private List<Node> _nodesCreated = new List<Node>();
        private List<Node> _nodesUpdated = new List<Node>();
        private List<Node> _nodesDeleted = new List<Node>();

        private List<Way> _waysCreated = new List<Way>();
        private List<Way> _waysUpdated = new List<Way>();
        private List<Way> _waysDeleted = new List<Way>();
    }

    public enum ChangesType
    {
        Created,
        Updated,
        Deleted
    }
}
