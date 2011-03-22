using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;
using OSMPBF;
using ProtoBuf;

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
                    ReadDenseNodes(primitiveGroup.dense, latOffset, lonOffset, granularity);
                }
                if (primitiveGroup.ways != null)
                {
                    //ReadWays(primitiveGroup.ways);
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

        /// <summary>
        /// Reads and parses a dense nodes (DenseNodes)
        /// </summary>
        /// <param name="denseNodes">Dense nodes to parse</param>
        /// <param name="latOffset">Offset latitude = .000000001 * primitiveBlock.lat_offset</param>
        /// <param name="lonOffset">Longitude offset = .000000001 * primitiveBlock.lon_offset</param>
        /// <param name="granularity">Granularity = .000000001 * primitiveBlock.granularity</param>
        private void ReadDenseNodes(DenseNodes denseNodes, double latOffset, double lonOffset, double granularity)
        {
            int l = 0;
            long deltaid = 0;
            long deltalat = 0;
            long deltalon = 0;
            long deltatimestamp = 0;
            long deltachangeset = 0;
            long deltauid = 0;
            long deltauser_sid = 0;

            for (int k = 0; k < denseNodes.id.Count; k++)
            {
                int has_tags = 0;
                deltaid += denseNodes.id[k];
                deltalat += denseNodes.lat[k];
                deltalon += denseNodes.lon[k];

                _nodes.AddNode(deltaid, latOffset + (deltalat * granularity),
                              lonOffset + (deltalon * granularity));

                if (denseNodes.denseinfo != null)
                {
                    DenseInfo denseinfo = denseNodes.denseinfo;

                    deltatimestamp += denseinfo.timestamp[k];
                    deltachangeset += denseinfo.changeset[k];
                    deltauid += denseinfo.uid[k];
                    deltauser_sid += denseinfo.user_sid[k];

                    //Console.WriteLine("version {0}", denseinfo.version[k]);
                    //Console.WriteLine("changeset {0}", deltachangeset);
                    //if (deltauid != -1)
                    //{ // osmosis makes anonymous users -1, ignoring
                    //    Console.WriteLine(primitiveBlock.stringtable.s[Convert.ToInt32(deltauser_sid)]);
                    //    Console.WriteLine("uid {0}", deltauid);
                    //}
                    //Console.WriteLine("timestamp {0}", deltatimestamp);
                }

                if (l < denseNodes.keys_vals.Count)
                {
                    while (denseNodes.keys_vals[l] != 0 && l < denseNodes.keys_vals.Count)
                    {
                        if (has_tags < 1)
                        {
                            has_tags++;
                        }



                        //key =
                        //    UTF8Encoding.UTF8.GetString(
                        //        primitiveBlock.stringtable.s[dense.keys_vals[l]]);
                        //val =
                        //    UTF8Encoding.UTF8.GetString(
                        //        primitiveBlock.stringtable.s[dense.keys_vals[l + 1]]);

                        //importConfigurator.GetTypeValueTag(key);

                        //Console.Write("key = {0} || ", key);
                        //Console.WriteLine("val = {0}", val);

                        //Console.Write("key = {0} || ", key.GetHashCode());
                        //Console.WriteLine("val = {0}", UTF8Encoding.UTF8.(
                        //        primitiveBlock.stringtable.s[dense.keys_vals[l + 1]]));
                        l += 2;
                    }
                    l += 1;
                }
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

        private void HashAndCheckTagsValues(string tag, string value, out int hashTag, out int hashValue, out TypeValueTag typeValueTag)
        {
            hashTag = OsmImportUtilites.GetHash(tag);
            hashValue = OsmImportUtilites.GetHash(value);
            typeValueTag = _importConfigurator.GetTypeValueTag(hashTag);
            if (!_hashTagsValuesOsmString.ContainsKey(hashTag)) _hashTagsValuesOsmString.Add(hashTag, value);

        }

        /// <summary>
        /// Determines whether to import into a database object based on configuration file import
        /// </summary>
        /// <param name="geo">Node, Way or Relation</param>
        /// <returns></returns>
        private bool IsImportAsGeoInDb(OsmPrimitive osmPrimitive)
        {
            bool imported = false;
            if (osmPrimitive.HashTags != null)
            {
                foreach (int hashTag in osmPrimitive.HashTags)
                {
                    if (_importConfigurator.GetTypeValueTag(hashTag) != TypeValueTag.NoImport)
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
            return imported;
        }

        private GeoNodes _nodes = new GeoNodes();
        /// <summary>
        /// Stores hashes of tags / values​​ and their OSM names
        /// </summary>
        private Dictionary<int, string> _hashTagsValuesOsmString;
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
