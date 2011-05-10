using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmImportToSqlServer.Config
{
    /// <summary>
    /// Lists the types of tag values ​​OSM
    /// </summary>
    public enum TypeValueTag
    {
        /// <summary>
        /// Tag value will be loaded id (INT)
        /// </summary>
        Id,
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
}
