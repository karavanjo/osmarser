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

    public abstract class OsmPrimitive
    {
        public Int64 Id
        {
            get { return _id; }
        }
        
        protected Int64 _id;
    }

    /// <summary>
    /// Class stores structural information node
    /// </summary>
    public class Node : OsmPrimitive
    {
        public Node(Int64 id)
        {
            base._id = id;
        }
    }

    /// <summary>
    /// Class stores structural information way
    /// </summary>
    public class Way : OsmPrimitive
    {
        public Way(Int64 id)
        {
            base._id = id;
            this._nodesId = new List<Int64>();
        }

        public void AddIdNode (Int64 idNode)
        {
            _nodesId.Add(idNode);
        }
        
        private List<Int64> _nodesId;
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
