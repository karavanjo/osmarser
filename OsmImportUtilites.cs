using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

using Osm.Utilites;

namespace Osm
{
    public class OsmImportUtilites
    {
        public static int GetHash(string hashedString)
        {
            return hashedString.GetHashCode();
        }
    }

    public class ImporterInSqlServer
    {
        private Scheduler _scheduler = new Scheduler();

        public void UploadTableInSqlServerNewThread(DataTable dataTable, string connectionString, string destinationNameTable, int timeout)
        {
            this.Import(new ImportTable(dataTable, connectionString, destinationNameTable, timeout));
        }

        public void UploadTableInSqlServerNewThread(DataTable dataTable, string connectionString, string destinationNameTable)
        {
            this.Import(new ImportTable(dataTable, connectionString, destinationNameTable));
        }

        public void UploadTableInSqlServerNewThread(DataTable dataTable, string connectionString)
        {
            new Thread(this.Import).Start(new ImportTable(dataTable, connectionString));
        }

        private void Import(object importTable)
        {
            ImportTable _importTable = (ImportTable) importTable;
            try
            {
                _scheduler.Enter(_importTable);
                try
                {
                    ImporterInSqlServer.UploadTableInSqlServer(_importTable);
                }
                finally
                {
                    _scheduler.Done();
                }
            }
            catch (Exception)
            {
                throw new AbandonedMutexException();
            }
        }

        private static void UploadTableInSqlServer(ImportTable importTable)
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

    public class ImportTable : ISchedulerOrdering
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

        private DateTime _time;
        public DateTime Time { get { return _time; } }

        public bool ScheduleBefore(ISchedulerOrdering s)
        {
            if (s is ImportTable)
            {
                ImportTable importTable = (ImportTable)s;
                return (this.Time < importTable.Time);
            }
            return false;
        }
    }

    
    public class ConstructDataTable
    {
        public ConstructDataTable(string nameTable)
        {
            this._nameDataTable = nameTable;
        }

        public ConstructDataTable()
        {
            this._nameDataTable = "";
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
