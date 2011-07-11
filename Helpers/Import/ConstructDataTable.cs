using System;
using System.Collections.Generic;
using System.Data;

namespace OsmImportToSqlServer.Helpers.Import
{
    public class ConstructDataTable
    {
        public ConstructDataTable(string nameTable = "")
        {
            this._nameDataTable = nameTable;
        }

        public void AddColumn(string nameColumn, string typeColumn)
        {
            _columnsNameAndTypes.Add(nameColumn, Type.GetType(typeColumn));
        }

        public void AddColumn(string nameColumn, TypeDataTable typeDataTable)
        {
            _columnsNameAndTypes.Add(nameColumn, this.GetTypeDataTable(typeDataTable));
        }

        public DataTable GetDataTable()
        {
            DataTable dataTableConstructed = new DataTable(this._nameDataTable);
            foreach (KeyValuePair<string, Type> columnNameAndType in _columnsNameAndTypes)
            {
                DataColumn columnNew = new DataColumn(columnNameAndType.Key, columnNameAndType.Value);
                dataTableConstructed.Columns.Add(columnNew);
            }
            return dataTableConstructed;
        }

        private Type GetTypeDataTable(TypeDataTable typeDataTable)
        {
            switch (typeDataTable)
            {
                case TypeDataTable.ByteArray:
                    return Type.GetType("System.Byte[]");
                    break;
                case TypeDataTable.Double:
                    return Type.GetType("System.Double");
                    break;
                case TypeDataTable.Int16:
                    return Type.GetType("System.Int16");
                    break;
                case TypeDataTable.Int32:
                    return Type.GetType("System.Int32");
                    break;
                case TypeDataTable.Int64:
                    return Type.GetType("System.Int64");
                    break;
                case TypeDataTable.String:
                    return Type.GetType("System.String");
                    break;
                case TypeDataTable.DateTime:
                    return Type.GetType("System.DateTime");
                case TypeDataTable.Byte:
                    return Type.GetType("System.Byte");
                    break;
            }

            throw new TypeLoadException("Type " + typeDataTable + " not supported");
        }

        public string NameDataTable
        {
            get { return _nameDataTable; }
            set { _nameDataTable = value; }
        }

        private string _nameDataTable;
        Dictionary<string, Type> _columnsNameAndTypes = new Dictionary<string, Type>();
    }
}
