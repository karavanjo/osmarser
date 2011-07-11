using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.XmlUtility;
using Ninject;
using OsmImportToSqlServer.Helpers;
using OsmImportToSqlServer.Helpers.Import;
using OsmImportToSqlServer.OsmData;

namespace OsmImportToSqlServer.Config
{
    /// <summary>
    /// Class to work with the configuration import pdb to SQL Server
    /// </summary>
    public sealed class OsmImportConfigurator
    {
        public static void CreateNewInstance (string pathOsmFileConfig)
        {
            if (Instance == null)
            {
                Instance = new OsmImportConfigurator(pathOsmFileConfig);
            }
            else
            {
                throw new ActivationException("Instance of class OsmImportConfigurator created.");
            }
        }

        public static OsmImportConfigurator Instance { get; private set; }

        #region Constructor
        /// <summary>
        /// Constructor OsmImportConfigurator
        /// </summary>
        /// <param name="pathOsmFileConfig">Path to the XML configuration file</param>
        OsmImportConfigurator(string pathOsmFileConfig)
        {
            if (File.Exists(pathOsmFileConfig))
            {
                _osmConfig = XDocument.Load(pathOsmFileConfig);
                
                ProcessDatabaseSection(_osmConfig.Root.Element("database"));
                ProcessTagsSection(_osmConfig.Root.Element("tags"));
                ProcessGeoSection(_osmConfig.Root.Element("geo"));
                ProcessRepositoriesSection(_osmConfig.Root.Element("repositories"));
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
                return TypeValueTag.Id;
            }

        }

        /// <summary>
        /// Determines whether specific rules for the preservation of the OsmPrimitiveType as an Geography for certain tags
        /// </summary>
        /// <param name="osmPrimitiveType">Type OsmPrimitive (Way or Relation)</param>
        /// <param name="hashTag">Tag hash</param>
        /// <param name="geoType">Out geoType if rule tag if there or NULL is not</param>
        /// <returns>True if exist rul tag</returns>
        public bool GetTypeOGC(Type osmPrimitiveType, string tag, out GeoType geoType)
        {
            if (osmPrimitiveType is Way)
            {
                geoType = this._geoConfig.GetGeoTypeTagWay(tag);
                return true;
            }
            if (osmPrimitiveType is Relation)
            {
                geoType = this._geoConfig.GetGeoTypeTagRealtion(tag);
                return true;
            }
            geoType = null;
            return false;
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

        private void ProcessRepositoriesSection(XElement repositoriesSection)
        {
            this.RepositoriesConfig = new RepositoriesConfig(repositoriesSection);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Stores the configuration database
        /// </summary>
        public DatabaseConfig DataBaseConfig { get; set; }

        public RepositoriesConfig RepositoriesConfig { get; set; }
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
}
