using System;
using System.Collections.Generic;
using System.Linq;
using OsmImportToSqlServer.Config;

namespace OsmImportToSqlServer.OsmData
{
    public struct Point
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public abstract class OsmPrimitive
    {
        public Int64 Id
        {
            get { return _id; }
        }

        public DateTime DateStamp
        {
            get { return _dateStamp; }
        }

        public GeoType GeoType
        { get; set; }

        protected Int64 _id;
        protected DateTime _dateStamp;
    }

    /// <summary>
    /// Class stores structural information node
    /// </summary>
    public class Node : OsmPrimitive
    {
        public Node(Int64 id, double lat, double lon, DateTime dateTime)
        {
            base._id = id;
            this._point = new Point() { Lat = lat, Lon = lon };
            base._dateStamp = dateTime;
        }

        public Node(Int64 id)
        {
            base._id = id;
        }

        public void GetLatLon(out double lat, out double lon)
        {
            lat = _point.Lat;
            lon = _point.Lon;
        }

        public double Latitude
        {
            get { return _point.Lat; }
        }

        public double Longtitude
        {
            get { return _point.Lon; }
        }

        private Point _point;
    }

    /// <summary>
    /// Class stores structural information way
    /// </summary>
    public class Way : OsmPrimitive
    {
        public Way(Int64 id, DateTime dateTime)
        {
            base._id = id;
            this._nodes = new List<Node>();
            base._dateStamp = dateTime;
        }

        public void AddNode(Node node)
        {
            _nodes.Add(node);
        }

        public bool IsPolygon()
        {
            if (_nodes.First().Id == _nodes.Last().Id && _nodes.Count > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<Node> Nodes
        {
            get { return _nodes; }
        }

        private List<Node> _nodes;
    }

    public class Relation : OsmPrimitive
    {
        
    }
    

    /// <summary>
    /// Class class describes the settings for importing geography object
    /// </summary>
    public class GeoType
    {
        public GeoType(GeoTypeOGC geoTypeOgc)
        {
            _geoTypeOgc = geoTypeOgc;
        }

        private GeoTypeOGC _geoTypeOgc;
        public GeoTypeOGC GeoTypeOGC
        {
            get { return _geoTypeOgc; }
        }
    }

    public enum OsmPrimitiveType
    {
        Node,
        Way,
        Relation
    }
}