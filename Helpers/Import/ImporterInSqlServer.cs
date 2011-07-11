using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace OsmImportToSqlServer.Helpers.Import
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
        private bool _shedule = false;

        public bool Shedule
        {
            get { return _shedule; }
            set { _shedule = value; }
        }

        public void UploadTableInSqlServer(DataTable dataTable, string connectionString,
            string destinationNameTable = "",
            int timeout = 100)
        {
            if (destinationNameTable == "") destinationNameTable = dataTable.TableName;
            ImportTable importTable = new ImportTable(dataTable, connectionString, destinationNameTable, timeout);
            ImporterInSqlServer.UploadTableInSqlServer(importTable);
        }

        public void UploadTableInSqlServerNewThread(DataTable dataTable, string connectionString,
            string destinationNameTable = "",
            int timeout = 100,
            bool shedule = false)
        {
            if (destinationNameTable == "") destinationNameTable = dataTable.TableName;
            ImportTable importTable = new ImportTable(dataTable, connectionString, destinationNameTable, timeout);
            if (shedule)
            {
                new Thread(this.ImportBySheduller).Start(importTable);
            }
            else
            {
                new Thread(this.ImportWithoutSheduller).Start(importTable);
            }
        }

        private void ImportBySheduller(object importTable)
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
            catch (Exception e)
            {
                int i;
                //throw new AbandonedMutexException();
            }
        }

        private void ImportWithoutSheduller(object importTable)
        {
            ImportTable _importTable = (ImportTable)importTable;
            ImporterInSqlServer.UploadTableInSqlServer(_importTable);
        }

        private static void UploadTableInSqlServer(ImportTable importTable)
        {
            ImportTable _importTable = (ImportTable)importTable;
            
            // ----- DEBUG
            Log.Log.Write("Start upload - table "
                + _importTable.DestinationNameTable
                + " (" + _importTable.DataTable.Rows.Count + ")");
            // ------

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

            // ----- DEBUG
            Log.Log.Write("End upload - table "
                + _importTable.DestinationNameTable
                + " (" + _importTable.DataTable.Rows.Count + ")");
            // ------
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

    
    

    public enum TypeDataTable
    {
        ByteArray,
        Int16,
        Int32,
        Int64,
        Double,
        String,
        DateTime,
        Byte
    }
}
