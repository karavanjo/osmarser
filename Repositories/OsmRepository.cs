using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmImportToSqlServer.Repositories
{
    public static class OsmRepository
    {
        public static void InitializeRepositories(string connectionString)
        {
            OsmRepository._connectionString = connectionString;
            InitializeRolesRepository();
            InitializeTagsRepository();
        }

        private static void InitializeRolesRepository()
        {
            RolesRepository = RelationsRolesRepository.Instance;
        }

        private static void InitializeTagsRepository()
        {
            TagsRepository = TagsRepository.Instance;
        }

        private static string _connectionString;
        public static RelationsRolesRepository RolesRepository;
        public static TagsRepository TagsRepository;
    }
}
