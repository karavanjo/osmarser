using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using OsmImportToSqlServer.Config;
using OsmImportToSqlServer.Helpers;
using OsmImportToSqlServer.Helpers.Geo;
using OsmImportToSqlServer.Helpers.Import;
using OsmImportToSqlServer.Helpers.ReadPdb;
using OsmImportToSqlServer.Importers.OsmImportPdbRowClasses;
using OsmImportToSqlServer.OsmData;
using OsmImportToSqlServer.Repositories;
using ProtoBuf;
using Node = OsmImportToSqlServer.OsmData.Node;
using Relation = OsmImportToSqlServer.Helpers.ReadPdb.Relation;
using Way = OsmImportToSqlServer.OsmData.Way;

namespace OsmImportToSqlServer.Importers
{
    /// <summary>
    /// Class that performs the import file .pdb in EMPTY database SQL Server
    /// </summary>
    public class OsmImportPdbRow
    {
        /// <summary>
        /// The constructor remembers the path to the file
        /// </summary>
        /// <param name="pathFilePdb">Absolute path to the file. pdb</param>
        public OsmImportPdbRow(string pathFilePdb)
        {
            if (File.Exists(pathFilePdb))
            {
                _pathFilePdb = pathFilePdb;
            }
            else
            {
                throw new FileNotFoundException("File .pdb data osm was not found!");
            }
        }

        /// <summary>
        /// Starts reading data from a file. pdb to the database
        /// </summary>
        /// <param name="osmImportConfigurator">The configurator process import</param>
        public void Import(OsmImportConfigurator osmImportConfigurator)
        {
            _importConfigurator = osmImportConfigurator;
            this.InitializeImporter();
            ReadFile();
            this.ImportTagsTranslate(OsmRepository.TagsRepository);
        }

        private void InitializeImporter()
        {
            _importer = new ImporterInSqlServer();
            _connectionString = ConfigurationManager.ConnectionStrings[_importConfigurator.DataBaseConfig.ConnectionStringName].ToString();
        }

        private void ReadFile()
        {
            //_tagsValueRepository = new TagsValueRepository();
            this.ReadFilePdb();
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
                this.UploadRemainingOsmData();
            }
        }

        private void UploadRemainingOsmData()
        {
            if (_nodesOsm.Count > 0) ImportPrimitiveNodesToDb();
            if (_waysOsm.Count > 0) ImportPrimitiveWaysToDb();
            if (_tagsValues.Rows.Count > 0) this.ImportDataTableInDb(ref _tagsValues, new GetTable(GetTableTagsValue));
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

            string key;
            string val;
            int hashTag;
            int hashValue;

            int idKey;
            int idValue;

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

                Node node = new Node(deltaid, latOffset + (deltalat * granularity),
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
                        key = UTF8Encoding.UTF8.GetString(
                            primitiveBlock.stringtable.s[denseNodes.keys_vals[l]]);
                        val = UTF8Encoding.UTF8.GetString(
                                primitiveBlock.stringtable.s[denseNodes.keys_vals[l + 1]]);

                        OsmRepository.TagsRepository.AddTag(key, val, out idKey, out idValue, out typeValueTag);

                        if (typeValueTag != TypeValueTag.NoImport)
                        {
                            IsImportToDb = true;
                            this.InsertTagsAndValue(node.Id, idKey, idValue, val, typeValueTag);
                        }
                        l += 2;
                    }
                    l += 1;
                }

                // Check whether there is a tags of an object 
                if (IsImportToDb)
                {
                    node.GeoType = new GeoType(GeoTypeOGC.Point);
                    //_nodesOsm.Add(node.Id, node);
                    this.AddNode(node);
                }
                else
                {
                    //_nodesOsm.Add(node.Id, node);
                    this.AddNode(node);
                }

            }

        }

        /// <summary>
        /// Reads and parses a ways (OSMPBF.Way)
        /// </summary>
        /// <param name="ways">List of ways</param>
        private void ReadWays(PrimitiveBlock primitiveBlock, List<Helpers.ReadPdb.Way> ways)
        {
            int dateGranularity = primitiveBlock.date_granularity;

            long second = 0;

            string key, val;
            int idKey, idValue;

            TypeValueTag typeValueTag;

            // Is it possible to import on the basis of the presence of significant tags
            bool IsImportToDb;

            for (int wayIndex = 0; wayIndex < ways.Count; wayIndex++)
            {
                Helpers.ReadPdb.Way osmpbfWay = ways[wayIndex];

                DateTime stamp = new DateTime();

                if (osmpbfWay.info != null)
                {
                    Info info = osmpbfWay.info;
                    second = (info.timestamp * dateGranularity) / 1000;
                    stamp = this.timeEpoche.AddSeconds(second);
                }
                long deltaref = 0;

                var way = new WaySimple(osmpbfWay.id, stamp);
                for (int nodeRef = 0; nodeRef < osmpbfWay.refs.Count; nodeRef++)
                {
                    deltaref += osmpbfWay.refs[nodeRef];
                    way.Nodes.Add(this.AddNodeSimpleToNodesRefs(deltaref));
                }

                IsImportToDb = false;
                GeoType geoType = null;

                if (osmpbfWay.keys.Count != 0 || osmpbfWay.keys.Count != 0)
                {
                    for (int keyId = 0; keyId < osmpbfWay.keys.Count; keyId++)
                    {
                        key = UTF8Encoding.UTF8.GetString(
                            primitiveBlock.stringtable.s[Convert.ToInt32(osmpbfWay.keys[keyId])]);
                        val = UTF8Encoding.UTF8.GetString(
                                primitiveBlock.stringtable.s[Convert.ToInt32(osmpbfWay.vals[keyId])]);

                        OsmRepository.TagsRepository.AddTag(key, val, out idKey, out idValue, out typeValueTag);

                        if (typeValueTag != TypeValueTag.NoImport)
                        {
                            IsImportToDb = true;
                            this.InsertTagsAndValue(way.Id, idKey, idValue, val, typeValueTag);
                            if (_importConfigurator.GetTypeOGC(Type.GetType("OsmImportToSqlServer.OsmData.Way"),
                                key, out geoType))
                                way.GeoType = geoType;
                        }
                    }
                }

                //// DEBUG
                //Console.WriteLine(DateTime.Now + " Way - " + way.Id + ", " + way.Nodes.Count + " nodes");
                if (IsImportToDb)
                {
                    if (way.GeoType != null)
                    {
                        this.AddWay(way);
                    }
                    else
                    {
                        way.GeoType = this.WayIsPolygonOrLine(way);
                        this.AddWay(way);
                    }
                }
                else
                {
                    this.AddWay(way);
                }


            }
        }

        private GeoType WayIsPolygonOrLine(WaySimple way)
        {
            if (way.IsPolygon())
            {
                return new GeoType(GeoTypeOGC.Polygon);
            }
            else
            {
                return new GeoType(GeoTypeOGC.LineString);
            }
        }

        /// <summary>
        /// Added new NodeSimple in _nodesRefs and getting this
        /// </summary>
        /// <param name="idNodeInWay"></param>
        /// <returns></returns>
        private NodeSimple AddNodeSimpleToNodesRefs(Int64 idNodeInWay)
        {
            NodeSimple nodeSimple;
            if (_nodesRefs.TryGetValue(idNodeInWay, out nodeSimple))
            {
                return nodeSimple;
            }
            else
            {
                nodeSimple = new NodeSimple(idNodeInWay);
                _nodesRefs.Add(idNodeInWay, nodeSimple);
                return nodeSimple;
            }
        }

        /// <summary>
        /// Reads and parses a relations (OSMPBF.Relation)
        /// </summary>
        /// <param name="relations">An array of relations</param>
        private void ReadRelations(List<Relation> relations)
        {
            for (int r = 0; r < relations.Count; r++)
            {

            }
        }

        /// <summary>
        /// Insert row in DataTable _tagsValues
        /// </summary>
        /// <param name="idGeo">Id OsmPrimitive, which save as geography type in database</param>
        /// <param name="idKey">Hash tag</param>
        /// <param name="idValue">Yash value tag</param>
        /// <param name="value">String value tag value</param>
        /// <param name="typeValueTag">Type value tag</param>
        private void InsertTagsAndValue(long idGeo, int idKey, int idValue, string value, TypeValueTag typeValueTag)
        {
            if (_tagsValues == null)
                _tagsValues = this.GetTableTagsValue();
            DataRow row = _tagsValues.NewRow();
            row["idGeo"] = idGeo;
            row["tag"] = idKey;

            switch (typeValueTag)
            {
                case TypeValueTag.Id:
                    row["vHash"] = idValue;
                    row["vType"] = Convert.ToInt16(TypeValueTag.Id);
                    break;
                case TypeValueTag.Int:
                    row["vHash"] = Int32.MaxValue;
                    int valueInt = 0;
                    if (int.TryParse(value, out valueInt))
                    {
                        row["vInt"] = valueInt;
                        row["vType"] = Convert.ToInt16(TypeValueTag.Int);
                    }
                    else
                    {
                        row["vInt"] = -9999;
                        row["vType"] = Convert.ToInt16(TypeValueTag.Int);
                        //throw new NotImplementedException("Parsed value type Int FAILED!");
                    }
                    break;
                case TypeValueTag.String:
                    row["vHash"] = Int32.MaxValue;
                    row["vString"] = value;
                    row["vType"] = Convert.ToInt16(TypeValueTag.String);
                    break;
            }
            _tagsValues.Rows.Add(row);

            if (_tagsValues.Rows.Count == COUNT_ROW)
            {
                this.ImportDataTableInDb(ref _tagsValues, new GetTable(GetTableTagsValue));
            }
        }

        private DataTable GetTableTagsValuesTrans()
        {
            ConstructDataTable constructDataTable = new ConstructDataTable("dbo.TagsValuesTrans");
            constructDataTable.AddColumn("tagHash", TypeDataTable.Int32);
            constructDataTable.AddColumn("valueHash", TypeDataTable.Int32);
            constructDataTable.AddColumn("tagTrans", TypeDataTable.String);
            constructDataTable.AddColumn("valTrans", TypeDataTable.String);
            constructDataTable.AddColumn("LCID", TypeDataTable.Int16);
            constructDataTable.AddColumn("typeTrans", TypeDataTable.Byte);

            return constructDataTable.GetDataTable();
        }

        private void ImportTagsTranslate(TagsRepository tagsRepository)
        {
            DataTable tableTagsTranslate = this.GetTableTagsValuesTrans();

            TagCompleteRowEnumerator enumerator = tagsRepository.GetEnumerator();
            while (enumerator.MoveNext())
            {
                TagCompleteRow tagCompleteRow = enumerator.Current;
                DataRow row = tableTagsTranslate.NewRow();
                row["tagHash"] = tagCompleteRow.KeyId;
                if (tagCompleteRow.TypeValue == TypeValueTag.Id)
                {
                    row["typeTrans"] = TagValueTransType.TagAndValue;
                    row["valueHash"] = tagCompleteRow.ValueId;
                    row["valTrans"] = tagCompleteRow.ValueName;
                }
                else
                {
                    row["valueHash"] = Int32.MaxValue;
                    row["typeTrans"] = TagValueTransType.OnlyTag;
                }
                row["tagTrans"] = tagCompleteRow.KeyName;
                row["LCID"] = -1;
                tableTagsTranslate.Rows.Add(row);
            }
            //if (_tagsValuesTrans.Rows.Count == COUNT_ROW)
            //{
            this.ImportDataTableInDb(ref tableTagsTranslate, new GetTable(GetTableTagsValuesTrans));
            //}
        }

        /// <summary>
        /// Creates a table for storing tags and their values
        /// </summary>
        /// <returns>Table for storing tags and their values</returns>
        private DataTable GetTableTagsValue()
        {
            ConstructDataTable constructDataTable = new ConstructDataTable(_importConfigurator.DataBaseConfig.TableNameValues);
            constructDataTable.AddColumn("idGeo", TypeDataTable.Int64);
            constructDataTable.AddColumn("tag", TypeDataTable.Int32);
            constructDataTable.AddColumn("vType", TypeDataTable.Int16);
            constructDataTable.AddColumn("vHash", TypeDataTable.Int32);
            constructDataTable.AddColumn("vString", TypeDataTable.String);
            constructDataTable.AddColumn("vInt", TypeDataTable.Int32);
            return constructDataTable.GetDataTable();
        }

        private void AddNode(Node node)
        {
            _nodesOsm.Add(node);
            if (_nodesOsm.Count == COUNT_ROW)
            {
                ImportPrimitiveNodesToDb();
            }
        }

        private DataTable _tableNodes = TablesTemplates.GetTableNodes();
        /// <summary>
        /// Imports Nodes in Sql Server
        /// </summary>
        private void ImportPrimitiveNodesToDb()
        {
            for (int i = 0; i < _nodesOsm.Count; i++)
            {
                DataRow rowNode = _tableNodes.NewRow();
                Node nodeSelect = _nodesOsm[i];
                rowNode["id"] = nodeSelect.Id;
                rowNode["lat"] = nodeSelect.Latitude;
                rowNode["lon"] = nodeSelect.Longtitude;
                rowNode["times"] = nodeSelect.DateStamp;
                _tableNodes.Rows.Add(rowNode);
                if (_nodesOsm[i].GeoType != null) FillGeoNodesTable(nodeSelect);
            }
            this.ImportDataTableInDb(ref _tableNodes, TablesTemplates.GetTableNodes);
            _nodesOsm.Clear();
        }

        private DataTable _geos = TablesTemplates.GetTableGeo();
        private void FillGeoNodesTable(Node node)
        {
            DataRow dataRow = _geos.NewRow();
            dataRow["idGeo"] = node.Id;
            dataRow["bin"] = GeoProcessing.GeoProcessingNode(node);
            dataRow["typeGeo"] = (byte)GeoTypeOGC.Point;
            _geos.Rows.Add(dataRow);



            if (_geos.Rows.Count == COUNT_ROW) this.ImportDataTableInDb(ref _geos, TablesTemplates.GetTableGeo);
        }

        private void AddGeoToGeoTable(DataRow rowGeoTable)
        {
            _geos.Rows.Add(rowGeoTable);
            if (_geos.Rows.Count > COUNT_ROW)
            {
                this.ImportGeoTableInDb();
            }
        }

        private void ImportGeoTableInDb()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("CreateGeoFromTableWKTBin"))
                {
                    SqlParameter geoTable = new SqlParameter("binwkt", _geos);
                    geoTable.SqlDbType = SqlDbType.Udt;
                    command.Parameters.Add(geoTable);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddWay(WaySimple way)
        {
            _waysOsm.Add(way);
            if (_nodesRefs.Count > COUNT_ROW)
            {
                ImportPrimitiveWaysToDb();
            }
        }

        private DataTable _tableWays = TablesTemplates.GetTableWays();
        /// <summary>
        /// Imports Ways in Sql Server
        /// </summary>
        private void ImportPrimitiveWaysToDb()
        {
            for (int w = 0; w < _waysOsm.Count; w++)
            {
                DataRow rowWay = _tableWays.NewRow();
                rowWay["id"] = _waysOsm[w].Id;
                if (_waysOsm[w].GeoType != null)
                    rowWay["typeGeo"] = Convert.ToByte(_waysOsm[w].GeoType.GeoTypeOGC);
                rowWay["times"] = _waysOsm[w].DateStamp;
                _tableWays.Rows.Add(rowWay);
                ImportRefsWays(_waysOsm[w]);
            }
            this.ImportDataTableInDb(ref _tableWays, TablesTemplates.GetTableWays);
            _waysOsm.Clear();
            this.ImportDataTableInDb(ref _tableWaysRefs, TablesTemplates.GetTableWaysRefs);
            _nodesRefs.Clear();
        }

        private DataTable _tableWaysRefs = TablesTemplates.GetTableWaysRefs();
        /// <summary>
        /// Imports WaysRefs in Sql Server
        /// </summary>
        private void ImportRefsWays(WaySimple waySimple)
        {
            int orderNode = 0;
            for (int i = 0; i < waySimple.Nodes.Count; i++)
            {
                orderNode++;
                DataRow rowWayRefs = _tableWaysRefs.NewRow();
                rowWayRefs["idWay"] = waySimple.Id;
                rowWayRefs["idNode"] = waySimple.Nodes[i].Id;
                rowWayRefs["orders"] = orderNode;
                _tableWaysRefs.Rows.Add(rowWayRefs);
            }
        }

        /// <summary>
        /// Returns the template table for imports ways in Sql Server
        /// </summary>
        /// <returns>Template table for saving way</returns>
        private DataTable GetTableWays()
        {
            ConstructDataTable ways = new ConstructDataTable("dbo.Ways");
            ways.AddColumn("id", TypeDataTable.Int64);
            ways.AddColumn("typeGeo", TypeDataTable.Byte);
            ways.AddColumn("orders", TypeDataTable.Int16);
            ways.AddColumn("idNode", TypeDataTable.Int64);
            ways.AddColumn("times", TypeDataTable.DateTime);
            return ways.GetDataTable();
        }

        private delegate DataTable GetTable();

        private void ImportDataTableInDb(ref DataTable dataTableImportable, GetTable callGetTableImportableTemplate)
        {
            if (_importer == null) this.InitializeImporter();
            _importer.UploadTableInSqlServer(dataTableImportable, _connectionString);
            dataTableImportable.Dispose();
            dataTableImportable = callGetTableImportableTemplate();
            GC.Collect();
        }



        /// <summary>
        /// Stores nodes labeled as the storage of geography objects to the database
        /// </summary>
        private List<Node> _nodesOsm = new List<Node>();

        /// <summary>
        /// Stores nodesRefs
        /// </summary>
        private Dictionary<Int64, NodeSimple> _nodesRefs = new Dictionary<long, NodeSimple>();
        /// <summary>
        /// Stores ways labeled as the storage of geography objects to the database
        /// </summary>
        private List<WaySimple> _waysOsm = new List<WaySimple>();
        /// <summary>
        /// DataTable which store tags and value for froup OsmPrimitive
        /// </summary>
        private DataTable _tagsValues;
        /// <summary>
        /// DataTable which store Openstreetmaps value tags / tags value
        /// </summary>
        private DataTable _tagsValuesTrans;
        /// <summary>
        /// Stores a pair of hashes tags / values
        /// </summary>
        private Dictionary<TagValue, bool> _tagsValuesHash = new Dictionary<TagValue, bool>();

        //private TagsValueRepository _tagsValueRepository;
        /// <summary>
        /// Store connection string
        /// </summary>
        private string _connectionString;
        /// <summary>
        /// Stores a reference to the current configurator imports
        /// </summary>
        private OsmImportConfigurator _importConfigurator;
        /// <summary>
        /// Sevices class for import data as a DataTable in Sql Server
        /// </summary>
        private ImporterInSqlServer _importer;
        /// <summary>
        /// Absolute path to the file. pdb
        /// </summary>
        private string _pathFilePdb;

        private const int COUNT_ROW = 250000;
    }

    //class WaySimple
    //{
    //    public WaySimple()
    //    {
    //        this.Nodes = new List<long>();
    //    }

    //    public Int64 Id { get; set; }
    //    public DateTime Date { get; set; }
    //    public GeoType TypeGeo { get; set; }
    //    public List<Int64> Nodes { get; set; }

    //    public bool IsPolygon()
    //    {
    //        if (Nodes[0] == Nodes[Nodes.Count - 1] && Nodes.Count > 1)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //}
}
