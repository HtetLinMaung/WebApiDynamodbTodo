using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace WebApiDynamodbTodo.Data
{
    public class DynamoDBContextWrapper
    {
        public DynamoDBContext Context { get; }
        public IAmazonDynamoDB Client { get; }

        public DynamoDBContextWrapper(IConfiguration configuration)
        {
            var awsOptions = configuration.GetSection("AWS");
            var accessKeyId = awsOptions["AccessKeyId"];
            var secretAccessKey = awsOptions["SecretAccessKey"];
            var region = awsOptions["Region"];

            var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
            };
            Client = new AmazonDynamoDBClient(credentials, config);
            Context = new DynamoDBContext(Client);
        }
    }
}
