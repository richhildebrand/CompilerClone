using MongoDB.Driver;

namespace Compilify.DataAccess.MongoDB
{
    public class MongoConnectionFactory : IMongoConnectionFactory
    {
        private readonly string connectionString;

        public MongoConnectionFactory(string connectionString)
        {
            //this.connectionString = connectionString;
           this.connectionString = "mongodb://appharbor_46b59e98-aff4-463f-90ac-89aea8d0ac8f:mnu5pi8h4co8en6ftdjp478qsi@ds037407-a.mongolab.com:37407/appharbor_46b59e98-aff4-463f-90ac-89aea8d0ac8f";
        }

        public MongoDatabase Create()
        {
            return MongoDatabase.Create(connectionString);
        }
    }
}