using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.XmlUtility;
using OsmImportToSqlServer.OsmData;

namespace OsmImportToSqlServer.Config
{
    /// <summary>
    /// Class that handles and stores the settings section 'geo' of transforming objects into geography
    /// </summary>
    public class GeoTypeConfig
    {
        public GeoTypeConfig(XElement xGeo)
        {
            if (XmlUtility.IsExistElementsInXElement(xGeo))
            {
                this.InitializePrivateField();
                foreach (XElement xGeoOsm in xGeo.Elements())
                {
                    switch (xGeoOsm.Name.ToString())
                    {
                        case "nodes":
                            this.ProcessSectionGeoTypeOsm(xGeoOsm, _nodes);
                            break;
                        case "ways":
                            this.ProcessSectionGeoTypeOsm(xGeoOsm, _ways);
                            break;
                        case "relations":
                            this.ProcessSectionGeoTypeOsm(xGeoOsm, _relations);
                            break;
                    }
                }
            }
            else
            {
                throw new XmlException(@"Element osm/geo not found or does not contain elements");
            }
        }

        private void InitializePrivateField()
        {
            _nodes = new Dictionary<int, GeoTypeOGC>();
            _ways = new Dictionary<int, GeoTypeOGC>();
            _relations = new Dictionary<int, GeoTypeOGC>();
        }

        private void ProcessSectionGeoTypeOsm(XElement xGeoOsm, Dictionary<int, GeoTypeOGC> geoOsm)
        {
            if (XmlUtility.IsExistAttributesInXElement(xGeoOsm))
            {
                foreach (XElement xTags in xGeoOsm.Elements())
                {
                    if (xTags.Attribute("geography") != null)
                    {
                        GeoTypeOGC geoTypeOgc = this.GetTypeOGCFromSectionGeo(xTags.Attribute("geography").Value);
                        geoOsm.Add(OsmImportUtilites.GetHash(xTags.Name.ToString()), geoTypeOgc);
                    }
                    else
                    {
                        throw new XmlException("A tag " + xGeoOsm.Name.ToString() +
                            @"/" + xTags.Name.ToString() + " attribute 'geography' is not found");
                    }
                }
            }
        }

        private GeoTypeOGC GetTypeOGCFromSectionGeo(string geography)
        {
            switch (geography)
            {
                case "POINT":
                    return GeoTypeOGC.Point;
                case "MULTIPOINT":
                    return GeoTypeOGC.MultiPoint;
                case "LINESTRING":
                    return GeoTypeOGC.LineString;
                case "MULTILINESTRING":
                    return GeoTypeOGC.MultiLineString;
                case "POLYGON":
                    return GeoTypeOGC.Polygon;
                case "MULTIPOLYGON":
                    return GeoTypeOGC.MultiPolygon;
                default:
                    throw new XmlException("Geography type '" + geography + "' unknown");
            }
        }

        public GeoType GetGeoTypeTagWay(int hashTag)
        {
            GeoTypeOGC geoTypeOgc;
            if (_ways.TryGetValue(hashTag, out geoTypeOgc))
            {
                return new GeoType(geoTypeOgc);
            }
            else
            {
                return null;
            }
        }

        public GeoType GetGeoTypeTagRealtion(int hashTag)
        {
            GeoTypeOGC geoTypeOgc;
            if (_relations.TryGetValue(hashTag, out geoTypeOgc))
            {
                return new GeoType(geoTypeOgc);
            }
            else
            {
                return null;
            }
        }

        private Dictionary<int, GeoTypeOGC> _nodes;
        private Dictionary<int, GeoTypeOGC> _ways;
        private Dictionary<int, GeoTypeOGC> _relations;
    }
}
