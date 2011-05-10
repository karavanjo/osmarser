using System;
using System.Collections.Generic;
using System.Data;
using OsmImportToSqlServer.Config;

namespace OsmImportToSqlServer.Repositories
{
    public abstract class RelationsRolesRepository : DataAccess
    {
        static RelationsRolesRepository _instance = null;

        public static RelationsRolesRepository Instance
        {
            get
            {
                return _instance ?? (_instance = (RelationsRolesRepository) Activator.CreateInstance(
                        OsmImportConfigurator.Instance.RepositoriesConfig.RelationRoles.TypeRepository));
            }
        }

        public RelationsRolesRepository()
        {
            _roles = new Dictionary<string, int>();
            _freeIndexRole = Int32.MinValue;
            this.DownloadAllRolesDataFromDb();
        }

        public void AddRole(string role, out int idRole)
        {
            if (_roles.TryGetValue(role, out idRole)) return;
            idRole = _freeIndexRole;
            _roles.Add(role, idRole);
            _freeIndexRole++;
        }

        private int _freeIndexRole;
        private Dictionary<string, int> _roles;

        // Work for DB
        protected abstract void DownloadAllRolesDataFromDb();

        protected void GetDataFromDb(IDataReader dataReader)
        {
            int i = 0;
            do
            {
                switch (i)
                {
                    case 0:
                        AllRolesFromDb(dataReader);
                        break;
                    case 1:
                        MaxIdFromId(dataReader);
                        break;
                }
                i++;
            } while (dataReader.NextResult());
        }

        protected virtual void AllRolesFromDb(IDataReader dataReader)
        {
            while (dataReader.Read())
            {
                _roles.Add(
                    (string)dataReader["memberRole"],
                    (int)dataReader["id"]
                    );
            }
        }

        protected virtual int MaxIdFromId(IDataReader dataReader)
        {
            while (dataReader.Read())
            {
                return (dataReader["maximum"] == DBNull.Value) ?
                    Int32.MinValue : (int)dataReader["maximum"];
            }
            return Int32.MinValue;
        }
    }
}
