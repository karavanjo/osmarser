using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Osm
{
    public class OsmImportChangesXML
    {
        public OsmImportChangesXML(string pathToFileChangesXml)
        {
            if (File.Exists(pathToFileChangesXml))
            {
                this._pathToFileChangesXml = pathToFileChangesXml;
            }
            else
            {
                throw new FileNotFoundException("File diffs XML not found!");
            }
        }

        public void StartImport()
        {
            using (StreamReader readerToFileChanges = new StreamReader(_pathToFileChangesXml))
            {
                using (XmlTextReader xmlTextReaderFileChange = new XmlTextReader(readerToFileChanges))
                {
                    while (xmlTextReaderFileChange.Read())
                    {
                        if (xmlTextReaderFileChange.NodeType != XmlNodeType.Whitespace)
                        {
                            switch (xmlTextReaderFileChange.Name)
                            {
                                case "create":
                                    this.ReadOsmPrimitive(xmlTextReaderFileChange, ChangesType.Created);
                                    break;
                                case "modify":
                                    this.ReadOsmPrimitive(xmlTextReaderFileChange, ChangesType.Updated);
                                    break;
                                case "delete":
                                    this.ReadOsmPrimitive(xmlTextReaderFileChange, ChangesType.Deleted);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void ReadOsmPrimitive(XmlTextReader readerBlockModify, ChangesType changesType)
        {
            while (readerBlockModify.NodeType != XmlNodeType.EndElement)
            {
                readerBlockModify.Read();
                if (readerBlockModify.NodeType != XmlNodeType.Whitespace)
                {
                    switch (readerBlockModify.Name)
                    {
                        case "node":
                            this.ReadNode(readerBlockModify);
                            break;
                        case "way":
                            this.ReadWay(readerBlockModify);
                            break;
                        case "relation":
                            this.ReadRelation(readerBlockModify);
                            break;
                    }
                }
            }
        }

        private void ReadNode(XmlTextReader readerNode)
        {
            long id = Convert.ToInt64(readerNode.GetAttribute("id"));
            double lat = Convert.ToDouble(readerNode.GetAttribute("lat"));
            double lon = Convert.ToDouble(readerNode.GetAttribute("lon"));
            DateTime timestamp = this.ConvertTimestampToDateTime(readerNode.GetAttribute("timestamp"));

            if (id != 0 & lat != 0.0 & lon != 0.0)
            {
                Node node = new Node(id, lat, lon, timestamp);

                if (!readerNode.IsEmptyElement)
                {
                    while (readerNode.NodeType != XmlNodeType.EndElement)
                    {
                        readerNode.Read();
                        if (readerNode.NodeType != XmlNodeType.Whitespace & readerNode.Name == "tag")
                        {
                            this.ReadTags(readerNode);
                        }
                    }
                }
            }
        }

        private void ReadTags(XmlTextReader readerTags)
        {

            //            string key = xmlTextReaderOsmFile.GetAttribute("k");
            //            if (!String.IsNullOrEmpty(key) && tags.ContainsKey(key))
            //            {
            //                DataRow rowTag = tableTags.NewRow();
            //                rowTag["idTag"] = tags[key];
            //                rowTag["idGeography"] = Convert.ToInt32(id);
            //                rowTag["vString"] = xmlTextReaderOsmFile.GetAttribute("v");
            //                tableTags.Rows.Add(rowTag);
            //            }
        }

        private void ReadWay(XmlTextReader readerWay)
        {

        }

        private void ReadRelation(XmlTextReader readerRelation)
        {

        }

        private DateTime ConvertTimestampToDateTime(string timestamp)
        {
            CultureInfo culture = new CultureInfo("en-US", true);
            culture.DateTimeFormat.FullDateTimePattern = "yyyy-MM-ddTHH:mm:ssZ";
            return DateTime.Parse(timestamp, culture);
        }

        private string _pathToFileChangesXml;
    }
}


