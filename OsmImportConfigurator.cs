using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.XmlUtility;

namespace Osm
{
    /// <summary>
    /// Class to work with the configuration import pdb to SQL Server
    /// </summary>
    class OsmImportConfigurator
    {
        #region Constructor
        /// <summary>
        /// Constructor OsmImportConfigurator
        /// </summary>
        /// <param name="pathOsmFileConfig">Path to the XML configuration file</param>
        public OsmImportConfigurator(string pathOsmFileConfig)
        {
            if (File.Exists(pathOsmFileConfig))
            {
                _osmConfig = XDocument.Load(pathOsmFileConfig);
                ProcessDatabaseSection(_osmConfig.Root.Element("database"));
                ProcessTagsSection(_osmConfig.Root.Element("tags"));
                ProcessGeoSection(_osmConfig.Root.Element("geo"));
            }
            else
            {
                throw new FileNotFoundException("Import the configuration file not found!");
            }
        }
        #endregion

        /// <summary>
        /// Returns the type in which to store the tag value
        /// </summary>
        /// <param name="tag">Tag Name</param>
        /// <returns>Enumerator value ("Hash", "String", "Int" etc.)</returns>
        public TypeValueTag GetTypeValueTag(string tag)
        {
            foreach (KeyValuePair<string, TypeValueTag> keyValuePair in _tagsContains)
            {
                if (tag.Contains(keyValuePair.Key)) return keyValuePair.Value;
            }
            TypeValueTag typeValueTag;
            if (_tags.TryGetValue(OsmImportUtilites.GetHash(tag), out typeValueTag))
            {
                return typeValueTag;
            }
            else
            {
                return TypeValueTag.Hash;
            }

        }

        #region Private methods
        /// <summary>
        /// Handles section "database" section and fills _tags
        /// </summary>
        /// <param name="databaseSection"></param>
        private void ProcessDatabaseSection(XElement databaseSection)
        {
            if (XmlUtility.IsExistElementsInXElement(databaseSection))
            {
                string connectionString, tableNameValues, tableNameGeo;

                XElement xConnString = databaseSection.Element("connectionString");
                if (XmlUtility.IsExistAttributesInXElement(xConnString))
                {
                    connectionString = xConnString.Attribute("name").Value;
                }
                else
                {
                    throw new XmlException("Element <connectionString>  not found or does not contain attribute 'name'");
                }

                XElement xTables = databaseSection.Element("tables");
                if (XmlUtility.IsExistElementsInXElement(xTables))
                {
                    XElement xTableNameValues = xTables.Element("values");
                    if (XmlUtility.IsExistAttributesInXElement(xTableNameValues))
                    {
                        tableNameValues = xTableNameValues.Attribute("name").Value;
                    }
                    else
                    {
                        throw new XmlException("Element <values> not found or does not contain attribute 'name'");
                    }

                    XElement xTableNameGeo = xTables.Element("geo");
                    if (XmlUtility.IsExistAttributesInXElement(xTableNameGeo))
                    {
                        tableNameGeo = xTableNameGeo.Attribute("name").Value;
                    }
                    else
                    {
                        throw new XmlException("Element <geo> not found or does not contain attribute 'name'");
                    }
                }
                else
                {
                    throw new XmlException("Element <tables> not found or does not contain elements");
                }

                if (!String.IsNullOrEmpty(connectionString) || !String.IsNullOrEmpty(tableNameGeo) || !String.IsNullOrEmpty(tableNameValues))
                {
                    DataBaseConfig = new DatabaseConfig()
                                         {
                                             ConnectionStringName = connectionString,
                                             TableNameGeo = tableNameGeo,
                                             TableNameValues = tableNameValues
                                         };
                }
                else
                {
                    throw new XmlException("Error in the section <database>. Perhaps one of the values ​​blank.");
                }
            }
        }

        /// <summary>
        /// Handles section "geo" section and fills _geoConfig
        /// </summary>
        /// <param name="geoSection"></param>
        private void ProcessGeoSection(XElement geoSection)
        {
            _geoConfig = new GeoTypeConfig(geoSection);
        }

        /// <summary>
        /// Handles section "tags" section and fills _tags
        /// </summary>
        /// <param name="tagsSection">XElement section "tags" configuration file</param>
        private void ProcessTagsSection(XElement tagsSection)
        {
            if (tagsSection != null && tagsSection.Elements().Count() > 0)
            {
                // Loads a tags to be imported and their types
                XElement importXElement = tagsSection.Element("import");
                if (importXElement != null && importXElement.Elements().Count() > 0)
                {
                    UploadTagWithTypeValue(importXElement.Element("string"), TypeValueTag.String);
                    UploadTagWithTypeValue(importXElement.Element("int"), TypeValueTag.Int);
                    UploadTagWithTypeValue(importXElement.Element("bit"), TypeValueTag.Bit);
                }
                // Loads a tags that can not be imported and their types
                UploadTagWithTypeValue(tagsSection.Element("noimport"), TypeValueTag.NoImport);
            }
        }

        /// <summary>
        /// Loads a group of tags, reading them with a section of the configuration file with a certain type of values
        /// </summary>
        /// <param name="typeSection">XElement section "string" or "hash" or "int" configuration file</param>
        /// <param name="typeValueTag"></param>
        private void UploadTagWithTypeValue(XElement typeSection, TypeValueTag typeValueTag)
        {
            if (typeSection != null && typeSection.Elements().Count() > 0)
            {
                foreach (XElement typeXElement in typeSection.Elements())
                {
                    if (typeXElement.Attribute("contain") != null)
                    {
                        _tagsContains.Add(typeXElement.Attribute("contain").Value, typeValueTag);
                    }
                    if (typeXElement.Attribute("tag") != null)
                    {
                        _tags.Add(OsmImportUtilites.GetHash(typeXElement.Attribute("tag").Value), typeValueTag);
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Stores the configuration database
        /// </summary>
        public DatabaseConfig DataBaseConfig { get; set; }
        #endregion

        #region Private Fields
        /// <summary>
        /// Stores configuration data section osm/geo
        /// </summary>
        private GeoTypeConfig _geoConfig;
        /// <summary>
        /// Stores the tag values ​​from tag data types and possible / impossible to import
        /// </summary>
        private Dictionary<int, TypeValueTag> _tags = new Dictionary<int, TypeValueTag>();
        /// <summary>
        /// Stores the tag values CONTAINES ​​from tag data types and possible / impossible to import
        /// </summary>
        private Dictionary<string, TypeValueTag> _tagsContains = new Dictionary<string, TypeValueTag>();
        /// <summary>
        /// Stores all the configuration file
        /// </summary>
        private XDocument _osmConfig;
        #endregion
    }


    /// <summary>
    /// Lists the types of tag values ​​OSM
    /// </summary>
    public enum TypeValueTag
    {
        /// <summary>
        /// Tag value will be loaded hash (INT)
        /// </summary>
        Hash,
        /// <summary>
        /// Tag value will be loaded VARCHAR
        /// </summary>
        String,
        /// <summary>
        /// Tag value will be loaded INT
        /// </summary>
        Int,
        /// <summary>
        /// Tag value will be loaded BIT
        /// </summary>
        Bit,
        /// <summary>
        /// The tag is not loaded into the database
        /// </summary>
        NoImport
    }

    /// <summary>
    /// List the types geography Sql Server according to the OGC
    /// </summary>
    public enum GeoTypeOGC
    {
        Point,
        MultiPoint,
        LineString,
        MultiLineString,
        Polygon,
        MultiPolygon,
        GeometryCollection
    }

    /// <summary>
    /// The class that maps the configuration file in section 'database'
    /// </summary>
    public class DatabaseConfig
    {
        public string ConnectionStringName { get; set; }
        public string TableNameGeo { get; set; }
        public string TableNameValues { get; set; }
    }

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

        private Dictionary<int, GeoTypeOGC> _nodes;
        private Dictionary<int, GeoTypeOGC> _ways;
        private Dictionary<int, GeoTypeOGC> _relations;
    }
}
