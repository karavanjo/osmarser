using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace OsmImportToSqlServer.Config
{
    /// <summary>
    /// The class that maps the configuration file in section 'database'
    /// </summary>
    public class DatabaseConfig
    {
        public string ConnectionStringName { get; set; }
        public string TableNameGeo { get; set; }
        public string TableNameValues { get; set; }

        public string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings[this.ConnectionStringName].ToString(); }
        }
    }
}
