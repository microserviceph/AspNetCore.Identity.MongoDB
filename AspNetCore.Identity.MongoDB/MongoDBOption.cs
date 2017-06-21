namespace AspNetCore.Identity.MongoDB
{
    public class MongoDBOption
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }

        public Option User { get; set; } = new Option();
        public Option Role { get; set; } = new Option();
    }

    public class Option
    {
        public string CollectionName { get; set; } = "Users";

        public bool ManageIndicies { get; set; } = true;
    }
}
