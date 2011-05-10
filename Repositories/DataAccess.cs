using System;
using System.Data;
using System.Data.Common;

namespace OsmImportToSqlServer.Repositories
{
    public abstract class DataAccess
    {
        private string _connectionString = "";
        protected string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }
        
        protected int ExecuteNonQuery(DbCommand cmd)
        {
            
                foreach (DbParameter param in cmd.Parameters)
                {
                    if (param.Direction == ParameterDirection.Output ||
                       param.Direction == ParameterDirection.ReturnValue)
                    {
                        switch (param.DbType)
                        {
                            case DbType.AnsiString:
                            case DbType.AnsiStringFixedLength:
                            case DbType.String:
                            case DbType.StringFixedLength:
                            case DbType.Xml:
                                param.Value = "";
                                break;
                            case DbType.Boolean:
                                param.Value = false;
                                break;
                            case DbType.Byte:
                                param.Value = byte.MinValue;
                                break;
                            case DbType.Date:
                            case DbType.DateTime:
                                param.Value = DateTime.MinValue;
                                break;
                            case DbType.Currency:
                            case DbType.Decimal:
                                param.Value = decimal.MinValue;
                                break;
                            case DbType.Guid:
                                param.Value = Guid.Empty;
                                break;
                            case DbType.Double:
                            case DbType.Int16:
                            case DbType.Int32:
                            case DbType.Int64:
                                param.Value = 0;
                                break;
                            default:
                                param.Value = null;
                                break;
                        }
                    }
                }
                return 1;
        }

        protected IDataReader ExecuteReader(DbCommand cmd)
        {
            return ExecuteReader(cmd, CommandBehavior.Default);
        }

        protected IDataReader ExecuteReader(DbCommand cmd, CommandBehavior behavior)
        {
            return cmd.ExecuteReader(behavior);
        }

        protected object ExecuteScalar(DbCommand cmd)
        {
            return cmd.ExecuteScalar();
        }
    }
}
