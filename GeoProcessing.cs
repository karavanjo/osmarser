using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Osm
{
    public class GeoProcessing
    {
        public static byte[] GeoProcessingNode(Node node)
        {
            SqlGeometry geo = new SqlGeometry();
            SqlGeometryBuilder GeometryBuilder = new SqlGeometryBuilder();
            GeometryBuilder.SetSrid(4326);
            GeometryBuilder.BeginGeometry(OpenGisGeometryType.Point);
            GeometryBuilder.BeginFigure(node.Latitude, node.Longtitude);
            GeometryBuilder.EndFigure();
            GeometryBuilder.EndGeometry();
            geo = GeometryBuilder.ConstructedGeometry;
            return geo.STAsBinary().Buffer;
        }

        public static byte[] GeoProcessingWay(Way way, GeoTypeOGC geoTypeOgc)
        {
            switch (geoTypeOgc)
            {
                case GeoTypeOGC.LineString:
                    return GeoProcessing.ConstructLineString(way.Nodes, way);
                    break;
                case GeoTypeOGC.Polygon:
                    return GeoProcessing.ConstructPolygon(way.Nodes, way);
                    break;
            }
            throw new TypeLoadException("Geo type " + geoTypeOgc + " using Way not supported");
        }

        public static byte[] GeoProcessingRelation()
        {
            throw new NotImplementedException("Not Implement GeoProcessingRelation()");
        }

        private static byte[] ConstructLineString(List<Node> nodes, OsmPrimitive osmPrimitive)
        {
            if (nodes != null)
            {
                try
                {
                    SqlGeometry line;
                    SqlGeometryBuilder lineBuilder = new SqlGeometryBuilder();
                    lineBuilder.SetSrid(4326);
                    lineBuilder.BeginGeometry(OpenGisGeometryType.LineString);
                    lineBuilder.BeginFigure(nodes.First().Latitude, nodes.First().Longtitude);
                    for (int i = 1; i < nodes.Count; i++)
                    {
                        lineBuilder.AddLine(nodes[i].Latitude, nodes[i].Longtitude);
                    }

                    lineBuilder.EndFigure();
                    lineBuilder.EndGeometry();
                    line = lineBuilder.ConstructedGeometry;

                    if (line.STIsValid())
                    {
                        return line.STAsBinary().Buffer;
                    }
                    else
                    {
                        Log.Write(osmPrimitive.GetType() + " " + osmPrimitive.Id + "not valid");
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error constructed Geometry LINE STRING: " + e.Message);
                    return null;
                }
            }
            else
            {
                throw new NullReferenceException("Construct polygon failed. Nodes not found.");
            }
        }

        private static byte[] ConstructPolygon(List<Node> nodes, OsmPrimitive osmPrimitive)
        {
            if (nodes != null)
            {
                try
                {
                    SqlGeometry polygon;
                    SqlGeometryBuilder polygonBuilder = new SqlGeometryBuilder();
                    polygonBuilder.SetSrid(4326);
                    polygonBuilder.BeginGeometry(OpenGisGeometryType.Polygon);
                    polygonBuilder.BeginFigure(nodes.First().Latitude, nodes.First().Longtitude);
                    for (int i = 1; i < nodes.Count; i++)
                    {
                        polygonBuilder.AddLine(nodes[i].Latitude, nodes[i].Longtitude);
                    }

                    polygonBuilder.EndFigure();
                    polygonBuilder.EndGeometry();

                    polygon = polygonBuilder.ConstructedGeometry;
                    if(polygon.STIsValid())
                    {
                        return polygon.STAsBinary().Buffer;
                    }
                    else
                    {
                        Log.Write(osmPrimitive.GetType() + " " + osmPrimitive.Id + "not valid");
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error constructed Geometry POLYGON: " + e.Message);
                    return null;
                }
            }
            else
            {
                throw new NullReferenceException("Construct polygon failed. Nodes not found.");
            }
        }
    }


    //public class GeoProcessing
    //{
    //    public static byte[] GeoProcessingNode(Node node)
    //    {
    //        SqlGeometry geo = new SqlGeometry();
    //        SqlGeometryBuilder GeometryBuilder = new SqlGeometryBuilder();
    //        GeometryBuilder.SetSrid(4326);
    //        GeometryBuilder.BeginGeometry(OpenGisGeometryType.Point);
    //        GeometryBuilder.BeginFigure(node.Latitude, node.Longtitude);
    //        GeometryBuilder.EndFigure();
    //        GeometryBuilder.EndGeometry();
    //        geo = GeometryBuilder.ConstructedGeometry;
    //        return geo.STAsBinary().Buffer;
    //    }

    //    public static byte[] GeoProcessingWay(Way way, GeoTypeOGC geoTypeOgc)
    //    {
    //        switch (geoTypeOgc)
    //        {
    //            case GeoTypeOGC.LineString:
    //                return GeoProcessing.ConstructLineString(way.Nodes);
    //                break;
    //            case GeoTypeOGC.Polygon:
    //                return GeoProcessing.ConstructPolygon(way.Nodes);
    //                break;
    //        }
    //        throw new TypeLoadException("Geo type " + geoTypeOgc + " using Way not supported");
    //    }

    //    public static byte[] GeoProcessingRelation()
    //    {
    //        throw new NotImplementedException("Not Implement GeoProcessingRelation()");
    //    }

    //    private static byte[] ConstructLineString(List<Node> nodes)
    //    {
    //        if (nodes != null)
    //        {
    //            try
    //            {
    //                SqlGeometry line;

    //                SqlGeometryBuilder lineBuilder = new SqlGeometryBuilder();
    //                lineBuilder.SetSrid(4326);
    //                lineBuilder.BeginGeometry(OpenGisGeometryType.LineString);
    //                lineBuilder.BeginFigure(nodes.First().Latitude, nodes.First().Longtitude);
    //                for (int i = 1; i < nodes.Count; i++)
    //                {
    //                    lineBuilder.AddLine(nodes[i].Latitude, nodes[i].Longtitude);
    //                }

    //                lineBuilder.EndFigure();
    //                lineBuilder.EndGeometry();
    //                line = lineBuilder.ConstructedGeometry;
    //                return line.STAsBinary().Buffer;
    //            }
    //            catch (Exception e)
    //            {
    //                Console.WriteLine("Error constructed Geometry LINE STRING: " + e.Message);
    //                return null;
    //            }
    //        }
    //        else
    //        {
    //            throw new NullReferenceException("Construct polygon failed. Nodes not found.");
    //        }
    //    }

    //    private static byte[] ConstructPolygon(List<Node> nodes)
    //    {
    //        if (nodes != null)
    //        {
    //            try
    //            {
    //            //SqlGeometry polygon;
    //            //SqlGeometryBuilder polygonBuilder = new SqlGeometryBuilder();
    //            //polygonBuilder.SetSrid(4326);
    //            //polygonBuilder.BeginGeometry(OpenGisGeometryType.Polygon);
    //            //polygonBuilder.BeginFigure(nodes.First().Latitude, nodes.First().Longtitude);
    //            //for (int i = 1; i < nodes.Count; i++)
    //            //{
    //            //    polygonBuilder.AddLine(nodes[i].Latitude, nodes[i].Longtitude);
    //            //}
                    
    //            //polygonBuilder.EndFigure();
    //            //polygonBuilder.EndGeometry();
                    
    //            //polygon = polygonBuilder.ConstructedGeometry;

    //            //return polygon.STAsBinary().Buffer;

    //                SqlGeometry polygon;
    //                SqlGeometryBuilder polygonBuilder = new SqlGeometryBuilder();
    //                polygonBuilder.SetSrid(4326);
    //                polygonBuilder.BeginGeometry(OpenGisGeometryType.Polygon);
    //                polygonBuilder.BeginFigure(nodes.First().Latitude, nodes.First().Longtitude);
    //                for (int i = 1; i < nodes.Count; i++)
    //                {
    //                    polygonBuilder.AddLine(nodes[i].Latitude, nodes[i].Longtitude);
    //                }

    //                polygonBuilder.EndFigure();
    //                polygonBuilder.EndGeometry();

    //                polygon = polygonBuilder.ConstructedGeometry;

    //                return polygon.STAsBinary().Buffer;
    //            }
    //            catch (Exception e)
    //            {
    //                Console.WriteLine("Error constructed Geometry POLYGON: " + e.Message);
    //                return null;
    //            }
    //        }
    //        else
    //        {
    //            throw new NullReferenceException("Construct polygon failed. Nodes not found.");
    //        }
    //    }
    //}
}
