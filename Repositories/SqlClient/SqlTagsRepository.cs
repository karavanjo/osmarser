using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmImportToSqlServer.Repositories.SqlClient
{
    public class SqlTagsRepository : TagsRepository
    {
        protected override void DownloadAllRolesDataFromDb()
        {
            throw new NotImplementedException();
        }
    }
}
