using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

namespace Osm
{
    public class OsmImportUtilites
    {
        public static int GetHash(string hashedString)
        {
            return hashedString.GetHashCode();
        }
    }

    public class UploadTableInDb
    {
        public static void UploadTableInSqlServerNewThread(DataTable dataTable, string connectionString, string destinationNameTable)
        {
            UploadTableInDb.UploadTableInSqlServerNewThread(new ImportTable(dataTable, connectionString, destinationNameTable));
        }

        public static void UploadTableInSqlServerNewThread(DataTable dataTable, string connectionString, string destinationNameTable, int timeout)
        {
            UploadTableInDb.UploadTableInSqlServerNewThread(new ImportTable(dataTable, connectionString, destinationNameTable, timeout));
        }

        private static void UploadTableInSqlServerNewThread(ImportTable importTable)
        {
            Thread threadUploadTableInSqlServer = new Thread(
                new ParameterizedThreadStart(UploadTableInDb.UploadTableInSqlServer));
            threadUploadTableInSqlServer.Start(importTable);
        }

        private static void UploadTableInSqlServer(object importTable)
        {
            ImportTable _importTable = (ImportTable)importTable;

            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(_importTable.ConnectionString))
            {
                foreach (DataColumn dataColumn in _importTable.DataTable.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                }
                sqlBulkCopy.BulkCopyTimeout = 100;
                sqlBulkCopy.DestinationTableName = _importTable.DestinationNameTable;

                sqlBulkCopy.WriteToServer(_importTable.DataTable);
            }
        }
    }

    public class ImportTable
    {
        public ImportTable(DataTable dataTable, string connectionString, string destinationNameTable, int timeout)
        {
            this._dataTable = dataTable;
            this._connectionString = connectionString;
            this._destinationNameTable = destinationNameTable;
            this._timeout = timeout;
        }

        public ImportTable(DataTable dataTable, string connectionString, string destinationNameTable)
            : this(dataTable, connectionString, destinationNameTable, 100) { }

        public ImportTable(DataTable dataTable, string connectionString, int timeout)
            : this(dataTable, connectionString, dataTable.TableName, timeout) { }

        public ImportTable(DataTable dataTable, string connectionString)
            : this(dataTable, connectionString, dataTable.TableName, 100) { }


        private DataTable _dataTable;
        private string _connectionString;
        private string _destinationNameTable;
        private int _timeout;

        public DataTable DataTable
        {
            get { return _dataTable; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public string DestinationNameTable
        {
            get { return _destinationNameTable; }
        }

        public int Timeout
        {
            get { return _timeout; }
        }
    }

    public class ConstructDataTable
    {
        public ConstructDataTable(string nameColumn, string typeColumn)
        {
            _columnsNameAndTypes.Add(nameColumn, Type.GetType(typeColumn));
        }

        public ConstructDataTable(string nameColumn, TypeDataTable typeDataTable)
        {
            _columnsNameAndTypes.Add(nameColumn, this.GetTypeDataTable(typeDataTable));
        }

        public void AddColumn(string nameColumn, string typeColumn)
        {
            _columnsNameAndTypes.Add(nameColumn, Type.GetType(typeColumn));
        }

        public void AddColumn(string nameColumn, TypeDataTable typeDataTable)
        {
            _columnsNameAndTypes.Add(nameColumn, this.GetTypeDataTable(typeDataTable));
        }

        public DataTable GetDataTable(string nameDataTable)
        {
            DataTable dataTableConstructed = new DataTable(nameDataTable);
            foreach (KeyValuePair<string, Type> columnNameAndType in _columnsNameAndTypes)
            {
                DataColumn columnNew = new DataColumn(columnNameAndType.Key, columnNameAndType.Value);
            }
            return dataTableConstructed;
        }

        public DataTable GetDataTable()
        {
            return this.GetDataTable("");
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
            }

            throw new TypeLoadException("Type " + typeDataTable + " not supported");
        }

        Dictionary<string, Type> _columnsNameAndTypes = new Dictionary<string, Type>();
    }

    public enum TypeDataTable
    {
        ByteArray,
        Int16,
        Int32,
        Int64,
        Double,
        String
    }
}
