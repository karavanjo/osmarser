using System.Data;

namespace OsmImportToSqlServer.Helpers.Import
{
    public class TablesTemplates
    {
        /// <summary>
        /// Returns the template table for imports geos as a varbinary(max) in Sql Server
        /// </summary>
        /// <returns>Template table for saving geos as a varbinary(max)</returns>
        public static DataTable GetTableGeo()
        {
            ConstructDataTable constructDataTable = new ConstructDataTable("dbo.Geo");
            constructDataTable.AddColumn("idGeo", TypeDataTable.Int64);
            constructDataTable.AddColumn("bin", TypeDataTable.ByteArray);
            constructDataTable.AddColumn("typeGeo", TypeDataTable.Int16);
            return constructDataTable.GetDataTable();
        }

        public static DataTable GetTableTagsValuesTrans()
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

        /// <summary>
        /// Creates a table for storing tags and their values
        /// </summary>
        /// <returns>Table for storing tags and their values</returns>
        public static DataTable GetTableTagsValue()
        {
            ConstructDataTable constructDataTable = new ConstructDataTable("dbo.TagsValues");
            constructDataTable.AddColumn("idGeo", TypeDataTable.Int64);
            constructDataTable.AddColumn("tag", TypeDataTable.Int32);
            constructDataTable.AddColumn("vType", TypeDataTable.Int16);
            constructDataTable.AddColumn("vHash", TypeDataTable.Int32);
            constructDataTable.AddColumn("vString", TypeDataTable.String);
            constructDataTable.AddColumn("vInt", TypeDataTable.Int32);
            return constructDataTable.GetDataTable();
        }

        /// <summary>
        /// Returns the template table for imports nodes in Sql Server
        /// </summary>
        /// <returns>Template table for saving node</returns>
        public static DataTable GetTableNodes()
        {
            ConstructDataTable nodes = new ConstructDataTable("dbo.Nodes");
            nodes.AddColumn("id", TypeDataTable.Int64);
            nodes.AddColumn("lat", TypeDataTable.Double);
            nodes.AddColumn("lon", TypeDataTable.Double);
            nodes.AddColumn("times", TypeDataTable.DateTime);
            return nodes.GetDataTable();
        }

        /// <summary>
        /// Returns the template table for imports ways in Sql Server
        /// </summary>
        /// <returns>Template table for saving way</returns>
        public static DataTable GetTableWays()
        {
            ConstructDataTable ways = new ConstructDataTable("dbo.Ways");
            ways.AddColumn("id", TypeDataTable.Int64);
            ways.AddColumn("typeGeo", TypeDataTable.Byte);
            ways.AddColumn("times", TypeDataTable.DateTime);
            return ways.GetDataTable();
        }

        /// <summary>
        /// Returns the template table for imports ways and nodes in Sql Server
        /// </summary>
        /// <returns>Template table for saving way and refs (nodes in way)</returns>
        public static DataTable GetTableWaysRefs()
        {
            ConstructDataTable waysRefs = new ConstructDataTable("dbo.WaysRefs");
            waysRefs.AddColumn("idWay", TypeDataTable.Int64);
            waysRefs.AddColumn("idNode", TypeDataTable.Int64);
            waysRefs.AddColumn("orders", TypeDataTable.Int16);
            return waysRefs.GetDataTable();
        }
    }
}
