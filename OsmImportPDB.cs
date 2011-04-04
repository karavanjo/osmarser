using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using OSMPBF;
using ProtoBuf;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace Osm
{
    /// <summary>
    /// Class that performs the import file .pdb in EMPTY database SQL Server
    /// </summary>
    public class OsmImportPDB
    {
        /// <summary>
        /// The constructor remembers the path to the file
        /// </summary>
        /// <param name="pathFilePDB">Absolute path to the file. pdb</param>
        public OsmImportPDB(string pathFilePDB)
        {
            if (File.Exists(pathFilePDB))
            {
                _pathFilePdb = pathFilePDB;
            }
            else
            {
                throw new FileNotFoundException("File .pdb data osm was not found!");
            }
        }

        /// <summary>
        /// Starts reading data from a file. pdb to the database
        /// </summary>
        /// <param name="pathFileConfig">The path to the file XML import configuration</param>
        public void Import(string pathFileConfig)
        {
            _importConfigurator = new OsmImportConfigurator(pathFileConfig);
            ReadFilePdb();
            this.GeoProcessingNode();
            //this.StartUploadTagsValueInDB(_tagsValues);
        }


        /// <summary>
        /// Deserializes to the level of primitive blocks (PrimitiveBlock)
        /// </summary>
        private void ReadFilePdb()
        {
            using (var file = File.OpenRead(_pathFilePdb))
            {
                int length, blockCount = 0;
                while (Serializer.TryReadLengthPrefix(file, PrefixStyle.Fixed32, out length))
                {
                    length = LowLevelReader.IntLittleEndianToBigEndian((uint)length);
                    BlobHeader header;
                    using (var tmp = new LimitedStream(file, length))
                    {
                        try
                        {
                            header = Serializer.Deserialize<BlobHeader>(tmp);
                        }
                        catch (Exception e)
                        {
                            throw new FileLoadException("Invalid file format .pdb", e);
                        }
                    }
                    Blob blob;
                    using (var tmp = new LimitedStream(file, header.datasize))
                    {
                        blob = Serializer.Deserialize<Blob>(tmp);
                    }
                    if (blob.zlib_data == null) throw new NotSupportedException("I'm only handling zlib here!");
                    HeaderBlock headerBlock;
                    PrimitiveBlock primitiveBlock;
                    using (var ms = new MemoryStream(blob.zlib_data))
                    using (var zlib = new ZLibStream(ms))
                    {
                        if (header.type == "OSMHeader")
                            headerBlock = Serializer.Deserialize<HeaderBlock>(zlib);
                        if (header.type == "OSMData")
                        {
                            primitiveBlock = Serializer.Deserialize<PrimitiveBlock>(zlib);
                            if (primitiveBlock != null)
                            {
                                ReadPrimitiveBlock(primitiveBlock);
                            }
                        }
                    }
                    blockCount++;
                }
            }
        }

        /// <summary>
        /// Reads and parses a primitive block (PrimitiveBlock)
        /// </summary>
        /// <param name="primitiveBlock">Primitive block to parse</param>
        private void ReadPrimitiveBlock(PrimitiveBlock primitiveBlock)
        {
            double latOffset = .000000001 * primitiveBlock.lat_offset;
            double lonOffset = .000000001 * primitiveBlock.lon_offset;
            double granularity = .000000001 * primitiveBlock.granularity;

            foreach (PrimitiveGroup primitiveGroup in primitiveBlock.primitivegroup)
            {
                if (primitiveGroup.dense != null)
                {
                    ReadDenseNodes(primitiveBlock, primitiveGroup.dense, latOffset, lonOffset, granularity);
                }
                if (primitiveGroup.ways != null)
                {
                    ReadWays(primitiveBlock, primitiveGroup.ways);
                }
                if (primitiveGroup.relations != null)
                {
                    //ReadRelations(primitiveGroup.relations);
                }

                //wayes = wayes + primitiveGroup.ways.Count;
                //relations = relations + primitiveGroup.relations.Count;
                //Console.WriteLine("\t\t Read primitiveGroup Nodes: {0}", primitiveGroup.dense.id.Count);
                //Console.WriteLine("\t\t Read primitiveGroup Wayes: {0}", primitiveGroup.ways.Count);
                //Console.WriteLine("\t\t Read primitiveGroup Relations: {0}", primitiveGroup.relations.Count);
            }
        }

        private DateTime timeEpoche = new DateTime(1970, 1, 1);

        /// <summary>
        /// Reads and parses a dense nodes (DenseNodes)
        /// </summary>
        /// <param name="denseNodes">Dense nodes to parse</param>
        /// <param name="latOffset">Offset latitude = .000000001 * primitiveBlock.lat_offset</param>
        /// <param name="lonOffset">Longitude offset = .000000001 * primitiveBlock.lon_offset</param>
        /// <param name="granularity">Granularity = .000000001 * primitiveBlock.granularity</param>
        private void ReadDenseNodes(PrimitiveBlock primitiveBlock, DenseNodes denseNodes, double latOffset, double lonOffset, double granularity)
        {
            int l = 0;
            long deltaid = 0;
            long deltalat = 0;
            long deltalon = 0;
            long deltatimestamp = 0;
            long deltachangeset = 0;
            long deltauid = 0;
            long deltauser_sid = 0;
            int dateGranularity = primitiveBlock.date_granularity;

            string tag;
            string val;
            int hashTag;
            int hashValue;
            TypeValueTag typeValueTag;

            // Is it possible to import on the basis of the presence of significant tags
            bool IsImportToDb;

            for (int k = 0; k < denseNodes.id.Count; k++)
            {
                int has_tags = 0;
                deltaid += denseNodes.id[k];
                deltalat += denseNodes.lat[k];
                deltalon += denseNodes.lon[k];
                DateTime stamp = new DateTime();

                if (denseNodes.denseinfo != null)
                {
                    DenseInfo denseinfo = denseNodes.denseinfo;

                    deltatimestamp += denseinfo.timestamp[k];
                    //deltachangeset += denseinfo.changeset[k];
                    //deltauid += denseinfo.uid[k];
                    //deltauser_sid += denseinfo.user_sid[k];

                    stamp = this.timeEpoche.AddSeconds(deltatimestamp * dateGranularity / 1000);

                }

                Osm.Node node = new Node(deltaid, latOffset + (deltalat * granularity),
                                             lonOffset + (deltalon * granularity), stamp);

                IsImportToDb = false;

                if (l < denseNodes.keys_vals.Count)
                {
                    while (denseNodes.keys_vals[l] != 0 && l < denseNodes.keys_vals.Count)
                    {
                        if (has_tags < 1)
                        {
                            has_tags++;
                        }
                        tag = UTF8Encoding.UTF8.GetString(
                            primitiveBlock.stringtable.s[denseNodes.keys_vals[l]]);
                        val = UTF8Encoding.UTF8.GetString(
                                primitiveBlock.stringtable.s[denseNodes.keys_vals[l + 1]]);
                        this.GetHashAndCheckTagsValues(tag, val, out hashTag, out hashValue, out typeValueTag);
                        if (typeValueTag != TypeValueTag.NoImport)
                        {
                            if (!_hashTagsValuesOsmString.ContainsKey(hashTag)) _hashTagsValuesOsmString.Add(hashTag, tag);
                            if (typeValueTag == TypeValueTag.Hash && !_hashTagsValuesOsmString.ContainsKey(hashValue))
                                _hashTagsValuesOsmString.Add(hashValue, val);
                            IsImportToDb = true;
                            this.InsertTagsAndValueInTableTagsValues(node.Id, hashTag, hashValue, val, typeValueTag);
                        }
                        l += 2;
                    }
                    l += 1;
                }

                // Check whether there is a tags of an object 
                _nodesOsm.Add(node, IsImportToDb);
            }

        }

        /// <summary>
        /// Reads and parses a ways (OSMPBF.Way)
        /// </summary>
        /// <param name="ways">List of ways</param>
        private void ReadWays(PrimitiveBlock primitiveBlock, List<OSMPBF.Way> ways)
        {
            int dateGranularity = primitiveBlock.date_granularity;

            long second = 0;

            string tag;
            string val;
            int hashTag;
            int hashValue;
            long deltaref = 0;
            TypeValueTag typeValueTag;

            // Is it possible to import on the basis of the presence of significant tags
            bool IsImportToDb;

            for (int wayIndex = 0; wayIndex < ways.Count; wayIndex++)
            {
                OSMPBF.Way osmpbfWay = ways[wayIndex];

                DateTime stamp = new DateTime();

                if (osmpbfWay.info != null)
                {
                    OSMPBF.Info info = osmpbfWay.info;
                    second = (info.timestamp * dateGranularity) / 1000;
                    stamp = this.timeEpoche.AddSeconds(second);
                }

                Osm.Way way = new Way(osmpbfWay.id, stamp);
                for (int nodeRef = 0; nodeRef < osmpbfWay.refs.Count; nodeRef++)
                {
                    deltaref += osmpbfWay.refs[nodeRef];
                    way.AddIdNode(deltaref);
                }

                IsImportToDb = false;

                if (osmpbfWay.keys.Count != 0 || osmpbfWay.keys.Count != 0)
                {
                    for (int keyId = 0; keyId < osmpbfWay.keys.Count; keyId++)
                    {
                        tag = UTF8Encoding.UTF8.GetString(
                            primitiveBlock.stringtable.s[Convert.ToInt32(osmpbfWay.keys[keyId])]);
                        val = UTF8Encoding.UTF8.GetString(
                                primitiveBlock.stringtable.s[Convert.ToInt32(osmpbfWay.vals[keyId])]);
                        this.GetHashAndCheckTagsValues(tag, val, out hashTag, out hashValue, out typeValueTag);
                        if (typeValueTag != TypeValueTag.NoImport)
                        {
                            IsImportToDb = true;
                            if (!_hashTagsValuesOsmString.ContainsKey(hashTag)) _hashTagsValuesOsmString.Add(hashTag, tag);
                            if (typeValueTag == TypeValueTag.Hash && !_hashTagsValuesOsmString.ContainsKey(hashValue))
                                _hashTagsValuesOsmString.Add(hashValue, val);
                            this.InsertTagsAndValueInTableTagsValues(way.Id, hashTag, hashValue, val, typeValueTag);
                        }
                    }
                }
                _waysOsm.Add(way, IsImportToDb);
            }
        }



        /// <summary>
        /// Reads and parses a relations (OSMPBF.Relation)
        /// </summary>
        /// <param name="relations">An array of relations</param>
        private void ReadRelations(List<OSMPBF.Relation> relations)
        {
            for (int r = 0; r < relations.Count; r++)
            {

            }
        }

        /// <summary>
        /// Insert row in DataTable _tagsValues
        /// </summary>
        /// <param name="idGeo">Id OsmPrimitive, which save as geography type in database</param>
        /// <param name="hashTag">Hash tag</param>
        /// <param name="hashValue">Yash value tag</param>
        /// <param name="value">String value tag value</param>
        /// <param name="typeValueTag">Type value tag</param>
        private void InsertTagsAndValueInTableTagsValues(long idGeo, int hashTag, int hashValue, string value, TypeValueTag typeValueTag)
        {
            DataRow row = _tagsValues.NewRow();
            row["idGeo"] = idGeo;
            row["tag"] = hashTag;

            switch (typeValueTag)
            {
                case TypeValueTag.Hash:
                    row["vHash"] = hashValue;
                    row["vType"] = Convert.ToInt16(TypeValueTag.Hash);
                    break;
                case TypeValueTag.Int:
                    int valueInt = 0;
                    if (int.TryParse(value, out valueInt))
                    {
                        row["vInt"] = valueInt;
                        row["vType"] = Convert.ToInt16(TypeValueTag.Int);
                    }
                    else
                    {
                        throw new NotImplementedException("Parsed value type Int FAILED!");
                    }
                    break;
                case TypeValueTag.String:
                    row["vString"] = value;
                    row["vType"] = Convert.ToInt16(TypeValueTag.String);
                    break;
            }
            _tagsValues.Rows.Add(row);
        }

        /// <summary>
        /// Starts a thread which loads the data on tags and their values ​​to the database
        /// </summary>
        /// <param name="tagsValue">Table tags and values</param>
        private void StartUploadTagsValueInDB(DataTable tagsValue)
        {
            Thread threadUploadTagsValueInDB = new Thread(
                new ParameterizedThreadStart(this.UploadTagsValueInDB));
            threadUploadTagsValueInDB.Start(tagsValue);
        }

        /// <summary>
        /// Loads the data on tags and their values ​​to the database
        /// </summary>
        /// <param name="tagsValue_DataTable">Table tags and values</param>
        private void UploadTagsValueInDB(object tagsValue_DataTable)
        {
            DataTable tagsValue = (DataTable)tagsValue_DataTable;
            string connectionString = ConfigurationManager.ConnectionStrings[_importConfigurator.DataBaseConfig.ConnectionStringName].ToString();

            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionString))
            {
                foreach (DataColumn dataColumn in tagsValue.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                }
                sqlBulkCopy.BulkCopyTimeout = 100;
                sqlBulkCopy.DestinationTableName = _importConfigurator.DataBaseConfig.TableNameValues;

                sqlBulkCopy.WriteToServer(tagsValue);
            }
        }

        /// <summary>
        /// Checks the type of tag values​​, calculates the hash value of tags and their values
        /// </summary>
        /// <param name="tag">OSM tag</param>
        /// <param name="value">OSM tag value</param>
        /// <param name="hashTag">Hash OSM tag</param>
        /// <param name="hashValue">Hash OSM tag value</param>
        /// <param name="typeValueTag">Type tag value</param>
        private void GetHashAndCheckTagsValues(string tag, string value, out int hashTag, out int hashValue, out TypeValueTag typeValueTag)
        {
            hashTag = OsmImportUtilites.GetHash(tag);
            hashValue = 0;
            typeValueTag = _importConfigurator.GetTypeValueTag(tag);
            if (typeValueTag == TypeValueTag.Hash) hashValue = OsmImportUtilites.GetHash(value);
        }

        /// <summary>
        /// Creates a table for storing tags and their values
        /// </summary>
        /// <returns>Table for storing tags and their values</returns>
        private static DataTable GetTableTagsValue()
        {
            DataColumn idGeo = new DataColumn("idGeo", Type.GetType("System.Int32"));
            DataColumn tag = new DataColumn("tag", Type.GetType("System.Int32"));
            DataColumn vType = new DataColumn("vType", Type.GetType("System.Int16"));
            DataColumn vHash = new DataColumn("vHash", Type.GetType("System.Int32"));
            DataColumn vString = new DataColumn("vString", Type.GetType("System.String"));
            DataColumn vInt = new DataColumn("vInt", Type.GetType("System.Int32"));

            DataTable tagsValues = new DataTable();

            tagsValues.Columns.Add(idGeo);
            tagsValues.Columns.Add(tag);
            tagsValues.Columns.Add(vType);
            tagsValues.Columns.Add(vHash);
            tagsValues.Columns.Add(vString);
            tagsValues.Columns.Add(vInt);

            return tagsValues;
        }

        private void GeoProcessingNode()
        {
            if (_nodesOsm.Count > 0)
            {
                DataTable _nodesGeo = this.CreateTableGeoNodes();

                foreach (KeyValuePair<Node, bool> keyValuePair in _nodesOsm)
                {
                    if (keyValuePair.Value == true)
                    {
                        SqlGeography geo = new SqlGeography();
                        SqlGeographyBuilder geographyBuilder = new SqlGeographyBuilder();
                        geographyBuilder.SetSrid(4326);
                        geographyBuilder.BeginGeography(OpenGisGeographyType.Point);
                        geographyBuilder.BeginFigure(keyValuePair.Key.Latitude, keyValuePair.Key.Longtitude);
                        geographyBuilder.EndFigure();
                        geographyBuilder.EndGeography();
                        geo = geographyBuilder.ConstructedGeography;
                        
                        //SqlBytes sqlBytes = geo.STAsBinary();

                        DataRow dataRow = _nodesGeo.NewRow();
                        dataRow["idGeo"] = keyValuePair.Key.Id;
                        dataRow["bin"] = geo.STAsBinary();
                        _nodesGeo.Rows.Add(dataRow);
                    }
                }
            }
        }

        private DataTable CreateTableGeoNodes()
        {
            DataTable geoNodes = new DataTable();

            DataColumn idGeo = new DataColumn("idGeo", Type.GetType("System.Int64"));
            DataColumn bin = new DataColumn("bin", Type.GetType("System.Byte[]"));

            geoNodes.Columns.Add(idGeo);
            geoNodes.Columns.Add(bin);

            return geoNodes;
        }

        /// <summary>
        /// Stores nodes labeled as the storage of geography objects to the database
        /// </summary>
        private Dictionary<Node, bool> _nodesOsm = new Dictionary<Node, bool>();
        /// <summary>
        /// Stores ways labeled as the storage of geography objects to the database
        /// </summary>
        private Dictionary<Way, bool> _waysOsm = new Dictionary<Way, bool>();
        /// <summary>
        /// DataTable which store tags and value for froup OsmPrimitive
        /// </summary>
        private DataTable _tagsValues = OsmImportPDB.GetTableTagsValue();
        /// <summary>
        /// Stores hashes of tags / values​​ and their OSM names
        /// </summary>
        private Dictionary<int, string> _hashTagsValuesOsmString = new Dictionary<int, string>();
        /// <summary>
        /// Stores a reference to the current configurator imports
        /// </summary>
        private OsmImportConfigurator _importConfigurator;
        /// <summary>
        /// Absolute path to the file. pdb
        /// </summary>
        private string _pathFilePdb;
    }
}
