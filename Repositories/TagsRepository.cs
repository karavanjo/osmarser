using System;
using System.Collections.Generic;
using System.Data;
using OsmImportToSqlServer.Config;
using OsmImportToSqlServer.OsmData;
using OsmImportToSqlServer.OsmData.Tags;

namespace OsmImportToSqlServer.Repositories
{
    public abstract class TagsRepository
    {
        static TagsRepository _instance = null;

        public static TagsRepository Instance
        {
            get
            {
                return _instance ?? (_instance = (TagsRepository)Activator.CreateInstance(
                    OsmImportConfigurator.Instance.RepositoriesConfig.TagsValue.TypeRepository));
            }
        }

        public bool ImmediateLoading { get; set; }

        protected TagsRepository()
        {
            //_keys = new Dictionary<string, Key>();
            //_values = new Dictionary<string, int>();
            _freeIndexTag = Int32.MinValue;
            _freeIndexValue = Int32.MinValue;
        }

        public void AddKey(string keyString, out Key key)
        {
            if (_keys.TryGetValue(keyString, out key)) return;
            key = new Key
                      {
                          Id = 0,
                          Name = keyString,
                          TypeValue = OsmImportConfigurator.Instance.GetTypeValueTag(keyString)
                      };
            if (key.TypeValue == TypeValueTag.NoImport) return;
            key.Id = _freeIndexTag;
            _keys.Add(keyString, key);
            _freeIndexTag++;
        }

        public void AddValue(string valueString, out Value value)
        {
            if (_values.TryGetValue(valueString, out value))
            {
                return;
            }
            value = new Value() { Id = _freeIndexValue, Name = valueString };
            _values.Add(valueString, value);
            _freeIndexValue++;
        }

        public void AddTag(string key, string value, out int idKey, out int idValue, out TypeValueTag typeValueTag)
        {
            Key _Key;
            this.AddKey(key, out _Key);
            typeValueTag = _Key.TypeValue;
            if (typeValueTag == TypeValueTag.NoImport)
            {
                idKey = 0;
                idValue = 0;
                return;
            }
            else
            {
                Value _Value;
                this.AddValue(value, out _Value);
                var tag = new Tag() { Key = _Key, Value = _Value };
                if (!_tags.Contains(tag))
                {
                    _tags.Add(tag);
                }
                typeValueTag = _Key.TypeValue;
                idKey = _Key.Id;
                idValue = _Value.Id;
            }
        }

        public TagCompleteRowEnumerator GetEnumerator()
        {
            return new TagCompleteRowEnumerator(_tags);
        }

        private int _freeIndexTag;
        private int _freeIndexValue;

        private readonly Dictionary<string, Key> _keys = new Dictionary<string, Key>();
        private readonly Dictionary<string, Value> _values = new Dictionary<string, Value>();
        private readonly HashSet<Tag> _tags = new HashSet<Tag>();

        // Work for DB
        protected abstract void DownloadAllRolesDataFromDb();

        //protected void GetDataFromDb(IDataReader dataReader)
        //{
        //    int i = 0;
        //    do
        //    {
        //        switch (i)
        //        {
        //            case 0:
        //                AllRolesFromDb(dataReader);
        //                break;
        //            case 1:
        //                MaxIdFromId(dataReader);
        //                break;
        //        }
        //        i++;
        //    } while (dataReader.NextResult());
        //}

        //protected virtual void AllRolesFromDb(IDataReader dataReader)
        //{
        //    while (dataReader.Read())
        //    {
        //        _roles.Add(
        //            (string)dataReader["memberRole"],
        //            (int)dataReader["id"]
        //            );
        //    }
        //}

        //protected virtual int MaxIdFromId(IDataReader dataReader)
        //{
        //    while (dataReader.Read())
        //    {
        //        return (dataReader["id"] == DBNull.Value) ?
        //            Int32.MinValue : (int)dataReader["id"];
        //    }
        //    return Int32.MinValue;
        //}
    }

    public class TagCompleteRowEnumerator : IEnumerator<TagCompleteRow>
    {
        private int _currentIndex;
        private HashSet<Tag>.Enumerator _enumeratorTags;
        private HashSet<Tag> _tags;


        public TagCompleteRowEnumerator(HashSet<Tag> tags)
        {
            this._tags = tags;
            this._enumeratorTags = tags.GetEnumerator();
        }

        public TagCompleteRow Current
        {
            get { return this.ConstructTagCompleteRow(); }
        }

        public TagCompleteRow ConstructTagCompleteRow()
        {
            if (this.Current != null)
            {
                TagCompleteRow tagCompleteRow = new TagCompleteRow();
                tagCompleteRow.KeyId = this._enumeratorTags.Current.Key.Id;
                tagCompleteRow.KeyName = this._enumeratorTags.Current.Key.Name;
                tagCompleteRow.ValueId = this._enumeratorTags.Current.Value.Id;
                tagCompleteRow.ValueName = this._enumeratorTags.Current.Value.Name;
                tagCompleteRow.TypeValue = this._enumeratorTags.Current.Key.TypeValue;
                return tagCompleteRow;
            }
            else
            {
                throw new NullReferenceException("Property 'Current' in enumerator TagCompleteRowEnumerator is null.");
            }
        }

        void IDisposable.Dispose() { }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            return _enumeratorTags.MoveNext();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class TagCompleteRow
    {
        public string KeyName { get; set; }
        public string ValueName { get; set; }
        public int KeyId { get; set; }
        public int ValueId { get; set; }
        public TypeValueTag TypeValue { get; set; }
    }

    //public class BoxEnumerator : IEnumerator<TagsRepository>
    //{
    //    private BoxCollection _collection;
    //    private int curIndex;
    //    private Box curBox;


    //    public BoxEnumerator(BoxCollection collection)
    //    {
    //        _collection = collection;
    //        curIndex = -1;
    //        curBox = default(Box);

    //    }

    //    public bool MoveNext()
    //    {
    //        //Avoids going beyond the end of the collection.
    //        if (++curIndex >= _collection.Count)
    //        {
    //            return false;
    //        }
    //        else
    //        {
    //            // Set current box to next item in collection.
    //            curBox = _collection[curIndex];
    //        }
    //        return true;
    //    }

    //    public void Reset() { curIndex = -1; }

    //    void IDisposable.Dispose() { }

    //    public Box Current
    //    {
    //        get { return curBox; }
    //    }


    //    object IEnumerator.Current
    //    {
    //        get { return Current; }
    //    }

    //}
}