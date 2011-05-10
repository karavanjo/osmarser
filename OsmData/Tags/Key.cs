using OsmImportToSqlServer.Config;

namespace OsmImportToSqlServer.OsmData.Tags
{
    public class Key
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TypeValueTag TypeValue { get; set; }
    }
}
