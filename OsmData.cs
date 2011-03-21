using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Osm
{
    public struct Point
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    /// <summary>
    /// Stores all the nodes geography data
    /// </summary>
    public class Nodes
    {
        public Nodes()
        {
            _nodes = new Dictionary<Int64, Point>();
        }

        public void AddNode(Int64 id, Point pointCoordinate)
        {
            _nodes.Add(id, pointCoordinate);
        }

        public void AddNode(Int64 id, double lat, double lon)
        {
            _nodes.Add(id, new Point(){Lat = lat, Lon = lon});
        }

        public Int64 CountNodes()
        {
            return _nodes.Count;
        }

        Dictionary<Int64, Point> _nodes;
    }

    /// <summary>
    /// Stores all the Ways geography data
    /// </summary>
    public class Ways
    {
        public Ways()
        {
            _ways = new Dictionary<long, List<int>>();
        }

        public void AddWay (Int64 id, List<int> nodes)
        {
            _ways.Add(id, nodes);
        }

        private Dictionary<Int64, List<int>> _ways;
    }

    /// <summary>
    /// Used to store the image geometry tables from the database and determine whether the different while creating an object from stored in database
    /// </summary>
    public class GeoDb
    {
        public GeoDb()
        {
            _geosDb = new Dictionary<long, long>();
        }

        public void AddGeoFromDb (Int64 idGeo, Int64 timestamp)
        {
            _geosDb.Add(idGeo, timestamp);
        }

        /// <summary>
        /// Checks whether the object GEOGRAPHY is changed in comparison with a database object
        /// </summary>
        /// <param name="idGeo">Object identifier</param>
        /// <param name="timestamp">Timestamp object</param>
        /// <returns>True - the object is changed, False - the object unchanged</returns>
        public bool IsChangedGeo(Int64 idGeo, Int64 timestamp)
        {
            if (_geosDb.ContainsKey(idGeo))
            {
                if (_geosDb[idGeo] == timestamp)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stores the image geometry table from the database - the identifier of the object and its timestamp
        /// </summary>
        private Dictionary<Int64, Int64> _geosDb;
    }

    /// <summary>
    /// The parent class for Nodes, Ways and Relations
    /// </summary>
    public class Geo
    {
        public Geo(Int64 id, Int64 timestamp, Int32 user)
        {
            this.Id = id;
            this.Timestamp = timestamp;
            this.User = user;
            this.HashTags = new List<int>();
        }

        public void AddTagHash(Int32 hashTag)
        {
            this.HashTags.Add(hashTag);
        }

        public Int64 Id { get; set; }
        public Int64 Timestamp { get; set; }
        public Int32 User { get; set; }
        public List<Int32> HashTags;
    }



    public class Tags
    {
        public Tags()
        {
            _tagsList = new SortedList<string, int>();
            _counter = 0;
        }

        public SortedList<string, int> TagsList
        {
            get { return _tagsList; }
            set { _tagsList = value; }
        }

        public void AddTag(string tagName)
        {
            if (!this.IsTagName(tagName))
                _tagsList.Add(tagName, _counter++);
        }

        public bool IsTagName(string tagName)
        {
            return _tagsList.ContainsKey(tagName);
        }

        public int IdTag(string tagName)
        {
            return _tagsList.Values[_tagsList.IndexOfKey(tagName)];
        }

        public DataTable GetTableTags()
        {
            DataTable tags = new DataTable();
            DataColumn idTag = new DataColumn("idTag", Type.GetType("System.Int32"));
            tags.Columns.Add(idTag);
            DataColumn keyTag = new DataColumn("keyTag", Type.GetType("System.String"));
            tags.Columns.Add(keyTag);
            for (int i = 0; i < _tagsList.Count; i++)
            {
                DataRow newRow = tags.NewRow();
                newRow["keyTag"] = _tagsList.Keys[i];
                newRow["idTag"] = _tagsList.Values[i];
            }
            return tags;
        }

        private SortedList<string, int> _tagsList;
        private int _counter;
    }

    public class TagValues
    {
        public TagValues()
        {
            _valuesConcreteTag = new SortedList<int, ValuesConcreteTag>();
        }

        public void AddValueTag(int idTag, string valueTag)
        {
            if (!_valuesConcreteTag.ContainsKey(idTag)) _valuesConcreteTag.Add(idTag, new ValuesConcreteTag());
            _valuesConcreteTag.Values[_valuesConcreteTag.IndexOfKey(idTag)].AddValue(valueTag);
        }

        public DataTable GetTableTagValues()
        {
            DataTable tagsValues = new DataTable();
            DataColumn idTag = new DataColumn("idTag", Type.GetType("System.Int32"));
            tagsValues.Columns.Add(idTag);
            DataColumn idValue = new DataColumn("idValue", Type.GetType("System.Int32"));
            tagsValues.Columns.Add(idValue);
            DataColumn value = new DataColumn("value", Type.GetType("System.String"));
            tagsValues.Columns.Add(value);
            for (int i = 0; i < _valuesConcreteTag.Count; i++)
            {
                for (int v = 0; v < _valuesConcreteTag[i].ValuesConcreteTagList.Count; v++)
                {
                    DataRow newRow = tagsValues.NewRow();
                    newRow["idTag"] = _valuesConcreteTag.Keys[i];
                    newRow["idValue"] = _valuesConcreteTag[i].ValuesConcreteTagList.Values[v];
                    newRow["value"] = _valuesConcreteTag[i].ValuesConcreteTagList.Keys[v];
                    tagsValues.Rows.Add(newRow);
                }
            }
            return tagsValues;
        }

        private SortedList<int, ValuesConcreteTag> _valuesConcreteTag;
    }

    public class ValuesConcreteTag
    {
        public ValuesConcreteTag()
        {
            _valuesConcreteTagList = new SortedList<string, int>();
            _counter = 0;
        }

        public SortedList<string, int> ValuesConcreteTagList
        {
            get { return _valuesConcreteTagList; }
            set { _valuesConcreteTagList = value; }
        }

        public void AddValue(string valueTag)
        {
            if (!this.IsValue(valueTag))
                _valuesConcreteTagList.Add(valueTag, _counter++);
        }

        public bool IsValue(string valueTag)
        {
            return _valuesConcreteTagList.ContainsKey(valueTag);
        }

        public int IdValue(string valueTag)
        {
            return _valuesConcreteTagList.Values[_valuesConcreteTagList.IndexOfKey(valueTag)];
        }

        private SortedList<string, int> _valuesConcreteTagList;
        private int _counter;
    }










    public class Tag
    {
        public int IdTag { get; set; }
        public string KeyTag { get; set; }
    }

    public class TagParameter
    {
        public int IdTag { get; set; }
        public int IdValue { get; set; }
        public string Value { get; set; }
    }

    public class GeoTags
    {
        public int IdGeo { get; set; }
        List<GeoTag> TagsAndValue { get; set; }
    }

    public class GeoTag
    {
        public int IdTag { get; set; }
        public int IdValue { get; set; }
        public string Value { get; set; }
    }

}
