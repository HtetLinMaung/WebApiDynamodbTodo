using Amazon.DynamoDBv2.DataModel;

namespace WebApiDynamodbTodo.Models
{
    [DynamoDBTable("TodoItems")]
    public class TodoItem
    {
        [DynamoDBHashKey]
        public string? Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}