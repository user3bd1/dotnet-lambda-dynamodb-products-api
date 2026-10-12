using Amazon.Lambda;
using Amazon.Lambda.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using productsApi.Models;



namespace productsApi.Controllers
{
    [ApiController]
    [Route("products")]
    public class productsController:ControllerBase
    {
        private readonly IAmazonLambda _lambda;
        private const string functionName = "MyApi";

        public productsController(IAmazonLambda lambda)
        {
            _lambda = lambda;
        }


        [HttpGet()]
        public async Task<IActionResult> GetProducts([FromQuery] int limit= 100)
        {
            var requestData = new InvokeRequest
            {
                FunctionName = functionName,
                InvocationType = InvocationType.RequestResponse,
                Payload = JsonSerializer.Serialize(new { limit = limit })
            };

            var response = await _lambda.InvokeAsync(requestData);

            var json = System.Text.Encoding.UTF8.GetString(
                response.Payload.ToArray()
            );

            if (!string.IsNullOrEmpty(response.FunctionError))
            {
                return StatusCode(500, json);
            }

            var products = JsonSerializer.Deserialize<List<Products>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (products == null || !products.Any())
            {
                return NotFound();
            }

            return Ok(products);
        }
        
        
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProductById(int id)
        {
            var request = new InvokeRequest
            {
                FunctionName = functionName,
                InvocationType = InvocationType.RequestResponse,
                Payload= JsonSerializer.Serialize(id)

            };

            var response = await _lambda.InvokeAsync(request);
            if (!string.IsNullOrEmpty(response.FunctionError)) return StatusCode(500, $"Lambda Error: {response.FunctionError}");

            var json = System.Text.Encoding.UTF8.GetString(response.Payload.ToArray());
            var product = JsonSerializer.Deserialize<Products>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive=true });

            if (product is null) return NotFound();

            return Ok(product);
        }
        
    }
}
