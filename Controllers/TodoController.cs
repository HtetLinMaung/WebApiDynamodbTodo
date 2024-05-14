using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using WebApiDynamodbTodo.Data;
using WebApiDynamodbTodo.Models;

namespace WebApiDynamodbTodo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly DynamoDBContextWrapper _contextWrapper;

        public TodoController(DynamoDBContextWrapper contextWrapper)
        {
            _contextWrapper = contextWrapper;
        }

        [HttpGet]
        public async Task<ActionResult> GetTodos(
            [FromQuery] string? searchTerm,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? lastEvaluatedKey = null)
        {
            var scanRequest = new ScanRequest
            {
                TableName = "TodoItems",
                Limit = pageSize,
                ExclusiveStartKey = !string.IsNullOrEmpty(lastEvaluatedKey) ?
                                    new Dictionary<string, AttributeValue> { { "Id", new AttributeValue { S = lastEvaluatedKey } } } :
                                    null
            };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                scanRequest.FilterExpression = "contains(#name, :searchTerm)";
                scanRequest.ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#name", "Name" }
                };
                scanRequest.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":searchTerm", new AttributeValue { S = searchTerm } }
                };
            }

            var response = await _contextWrapper.Client.ScanAsync(scanRequest);

            var todos = new List<TodoItem>();
            foreach (var item in response.Items)
            {
                todos.Add(new TodoItem
                {
                    Id = item["Id"].S,
                    Name = item["Name"].S,
                    IsComplete = item["IsComplete"].BOOL
                });
            }

            var result = new
            {
                Items = todos,
                LastEvaluatedKey = response.LastEvaluatedKey.Count > 0 ? response.LastEvaluatedKey["Id"].S : null
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoById(string id)
        {
            var todo = await _contextWrapper.Context.LoadAsync<TodoItem>(id);
            if (todo == null)
            {
                return NotFound();
            }
            return Ok(todo);
        }

        [HttpPost]
        public async Task<ActionResult> CreateTodo([FromBody] TodoItem todo)
        {
            todo.Id = Guid.NewGuid().ToString();
            await _contextWrapper.Context.SaveAsync(todo);
            return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(string id, [FromBody] TodoItem todo)
        {
            var existingTodo = await _contextWrapper.Context.LoadAsync<TodoItem>(id);
            if (existingTodo == null)
            {
                return NotFound();
            }
            existingTodo.Name = todo.Name;
            existingTodo.IsComplete = todo.IsComplete;
            await _contextWrapper.Context.SaveAsync(existingTodo);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoById(string id)
        {
            var todo = await _contextWrapper.Context.LoadAsync<TodoItem>(id);
            if (todo == null)
            {
                return NotFound();
            }
            await _contextWrapper.Context.DeleteAsync(todo);
            return NoContent();
        }
    }
}