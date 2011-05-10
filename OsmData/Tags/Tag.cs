namespace OsmImportToSqlServer.OsmData.Tags
{
    public struct TagId
    {
        public int KeyId { get; set; }
        public int ValueId { get; set; }
    }

    public class Tag
    {
        public Key Key { get; set; }
        public Value Value { get; set; }
    }

    //public class Tag
    //{
    //    public TagId TagId { get; set; }
    //    public string 
    //}
}
