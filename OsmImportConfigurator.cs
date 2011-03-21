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
            }
            else
            {
                throw new FileNotFoundException("Import the configuration file not found!");
            }
        }

        /// <summary>
        /// Handles section "database" section and fills _tags
        /// </summary>
        /// <param name="databaseSection"></param>
        public void ProcessDatabaseSection(XElement databaseSection)
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
                        tableNameValues = xConnString.Attribute("name").Value;
                    }
                    else
                    {
                        throw new XmlException("Element <values>  not found or does not contain attribute 'name'");
                    }

                    XElement xTableNameGeo = xTables.Element("geo");
                    if (XmlUtility.IsExistAttributesInXElement(xTableNameGeo))
                    {
                        tableNameGeo = xTableNameGeo.Attribute("geo").Value;
                    }
                    else
                    {
                        throw new XmlException("Element <geo>  not found or does not contain attribute 'name'");
                    }
                }
                else
                {
                    throw new XmlException("Element <tables>  not found or does not contain elements");
                }

                if (String.IsNullOrEmpty(connectionString) && String.IsNullOrEmpty(tableNameGeo) && String.IsNullOrEmpty(tableNameValues))
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

        public DatabaseConfig DataBaseConfig { get; set; }

        /// <summary>
        /// Handles section "tags" section and fills _tags
        /// </summary>
        /// <param name="tagsSection">XElement section "tags" configuration file</param>
        private void ProcessTagsSection(XElement tagsSection)
        {
            _tags = new Dictionary<string, TypeValueTag>();
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
                    _tags.Add(typeXElement.Name.ToString(), typeValueTag);
                }
            }
        }

        /// <summary>
        /// Returns the type in which to store the tag value
        /// </summary>
        /// <param name="tag">Tag Name</param>
        /// <returns>Enumerator value ("Hash", "String", "Int" etc.)</returns>
        public TypeValueTag GetTypeValueTag(int tagHash)
        {
            if (_tags.Keys.Contains(tagHash))
            {
                return _tags[tagHash];
            }
            else
            {
                return TypeValueTag.Hash;
            }
        }


        /// <summary>
        /// Stores the tag values ​​from tag data types and possible / impossible to import
        /// </summary>
        private Dictionary<int , TypeValueTag> _tags;
        /// <summary>
        /// Stores all the configuration file
        /// </summary>
        private XDocument _osmConfig;
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

    public class DatabaseConfig
    {
        public string ConnectionStringName { get; set; }
        public string TableNameGeo { get; set; }
        public string TableNameValues { get; set; }
    }


}
