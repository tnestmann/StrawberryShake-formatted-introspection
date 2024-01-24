using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.SystemTextJson;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Example.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly IDocumentExecuter _documentExecuter;
        private readonly ISchema _schema;
        private readonly IOptions<GraphQLSettings> _graphQLOptions;

        public ApiController(IDocumentExecuter documentExecuter, ISchema schema, IOptions<GraphQLSettings> options)
        {
            _documentExecuter = documentExecuter;
            _schema = schema;
            _graphQLOptions = options;
        }

        [HttpPost("graphql")]
        public async Task<IActionResult> GraphQLAsync([FromBody] GraphQLRequest request)
        {
            var startTime = DateTime.UtcNow;

            var result = await _documentExecuter.ExecuteAsync(s =>
            {
                s.Schema = _schema;
                s.Query = request.Query;
                s.Variables = request.Variables;
                s.OperationName = request.OperationName;
                s.RequestServices = HttpContext.RequestServices;
                s.UserContext = new GraphQLUserContext
                {
                    User = HttpContext.User,
                };
                s.CancellationToken = HttpContext.RequestAborted;
            });

            if (_graphQLOptions.Value.EnableMetrics)
            {
                result.EnrichWithApolloTracing(startTime);
            }

            return new ExecutionResultActionResult(result);
        }

        [HttpPost("graphql-indented")]
        public async Task<IActionResult> GraphQLBadAsync([FromBody] GraphQLRequest request)
        {
            string result = await _schema.ExecuteAsync(
                    x =>
                    {
                        x.RequestServices = HttpContext.RequestServices;
                        x.Query = request.Query;
                        x.Variables = request.Variables;
                    }).ConfigureAwait(false);

            // var tempObj = JsonSerializer.Deserialize<JsonElement>(result);

            // var graphQLSerializer = new GraphQLSerializer();
            // string inlineJson = graphQLSerializer.Serialize(tempObj);

            return Ok(result);
        }
    }
}
