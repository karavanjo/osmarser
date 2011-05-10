using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using OsmImportToSqlServer.Config;

namespace OsmImportToSqlServer.Repositories.SqlClient
{
    public class SqlRelationsRolesRepository : RelationsRolesRepository
    {
        protected override void DownloadAllRolesDataFromDb()
        {
            string connString =
                ConfigurationManager.ConnectionStrings[
                    OsmImportConfigurator.Instance.RepositoriesConfig.RelationRoles.ConnectionString].ToString();
            using (SqlConnection cn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("dbo.GetAllMemberRoles", cn) { CommandType = CommandType.StoredProcedure };
                cn.Open();
                IDataReader reader = ExecuteReader(cmd, CommandBehavior.Default);
                this.GetDataFromDb(reader);
            }
        }
    }
}
