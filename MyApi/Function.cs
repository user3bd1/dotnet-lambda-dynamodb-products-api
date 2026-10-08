using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using productsApi.Models;
using System.Text.Json;

[assembly: LambdaSerializer(
    typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer)
)]

namespace MyAPi;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDb;

    private const string tableName = "Products";

    public Function()
    {
        _dynamoDb = new AmazonDynamoDBClient();
    }

    public async Task<object?> FunctionHandler(JsonElement input, ILambdaContext context)
    {
        context.Logger.LogLine($"Input: {input.GetRawText()}");

        if (input.ValueKind == JsonValueKind.Number)
        {
            return await GetProduct(input.GetInt32(), context);
        }

        return await GetProducts(context);
    }

    public async Task<List<Products>> GetProducts(
        ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine("Getting products from DynamoDB...");

            var request = new ScanRequest
            {
                TableName = tableName
            };

            var response = await _dynamoDb.ScanAsync(request);

            context.Logger.LogLine(
                $"DynamoDB returned {response.Items.Count} items."
            );

            var products = new List<Products>();

            foreach (var item in response.Items)
            {
                context.Logger.LogLine(
                    $"DynamoDB item: {JsonSerializer.Serialize(item)}"
                );

                products.Add(new Products
                {
                    Id = item.ContainsKey("id") && item["id"].N != null ? int.Parse(item["id"].N) : 0,
                    Title = item.GetValueOrDefault("title")?.S ?? string.Empty,
                    Description = item.GetValueOrDefault("description")?.S ?? string.Empty,
                    Category = item.GetValueOrDefault("category")?.S ?? string.Empty,
                    Price = item.ContainsKey("price") && item["price"].N != null ? decimal.Parse(item["price"].N) : 0m,
                    DiscountPercentage = item.ContainsKey("discountPercentage") && item["discountPercentage"].N != null ? decimal.Parse(item["discountPercentage"].N) : 0m,
                    Rating = item.ContainsKey("rating") && item["rating"].N != null ? decimal.Parse(item["rating"].N) : 0m,
                    Stock = item.ContainsKey("stock") && item["stock"].N != null ? int.Parse(item["stock"].N) : 0,
                    Brand = item.GetValueOrDefault("brand")?.S ?? string.Empty,
                    Sku = item.GetValueOrDefault("sku")?.S ?? string.Empty,
                    Weight = item.ContainsKey("weight") && item["weight"].N != null ? decimal.Parse(item["weight"].N) : 0m,
                    WarrantyInformation = item.GetValueOrDefault("warrantyInformation")?.S ?? string.Empty,
                    ShippingInformation = item.GetValueOrDefault("shippingInformation")?.S ?? string.Empty,
                    AvailabilityStatus = item.GetValueOrDefault("availabilityStatus")?.S ?? string.Empty,
                    ReturnPolicy = item.GetValueOrDefault("returnPolicy")?.S ?? string.Empty,
                    MinimumOrderQuantity = item.ContainsKey("minimumOrderQuantity") && item["minimumOrderQuantity"].N != null ? int.Parse(item["minimumOrderQuantity"].N) : 0,
                    Thumbnail = item.GetValueOrDefault("thumbnail")?.S ?? string.Empty,
                    Tags = item.ContainsKey("tags") && item["tags"].L != null ? item["tags"].L.Select(x => x.S).ToList() : new List<string>(),
                    Images = item.ContainsKey("images") && item["images"].L != null ? item["images"].L.Select(x => x.S).ToList()  : new List<string>(),
                    Dimensions = item.ContainsKey("dimensions") && item["dimensions"].M != null ? new Dimensions
                    {
                        Width = item["dimensions"].M.ContainsKey("width") ? decimal.Parse(item["dimensions"].M["width"].N) : 0m,
                        Height = item["dimensions"].M.ContainsKey("height") ? decimal.Parse(item["dimensions"].M["height"].N) : 0m,
                        Depth = item["dimensions"].M.ContainsKey("depth") ? decimal.Parse(item["dimensions"].M["depth"].N) : 0m
                    } : null,

                    
                    Meta = item.ContainsKey("meta") && item["meta"].M != null ? new Meta
                    {
                        CreatedAt = item["meta"].M.ContainsKey("createdAt") ? DateTime.Parse(item["meta"].M["createdAt"].S) : DateTime.MinValue,
                        UpdatedAt = item["meta"].M.ContainsKey("updatedAt") ? DateTime.Parse(item["meta"].M["updatedAt"].S) : DateTime.MinValue,
                        Barcode = item["meta"].M.GetValueOrDefault("barcode")?.S ?? string.Empty,
                        QrCode = item["meta"].M.GetValueOrDefault("qrCode")?.S ?? string.Empty
                    } : null,

                    
                    Reviews = item.ContainsKey("reviews") && item["reviews"].L != null
                    ? item["reviews"].L.Select(r => new Review
                    {
                         Rating = r.M.ContainsKey("rating") ? int.Parse(r.M["rating"].N) : 0,
                         Comment = r.M.GetValueOrDefault("comment")?.S ?? string.Empty,
                         Date = r.M.ContainsKey("date") ? DateTime.Parse(r.M["date"].S) : DateTime.MinValue,
                         ReviewerName = r.M.GetValueOrDefault("reviewerName")?.S ?? string.Empty,
                         ReviewerEmail = r.M.GetValueOrDefault("reviewerEmail")?.S ?? string.Empty
                    }).ToList() : new List<Review>()
                });
            }

            return products;
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"ERROR: {ex}");
            throw;
        }
    }


    
    public async Task<Products> GetProduct(int Id, ILambdaContext context)
    {
        context.Logger.LogLine($"Getting a product with Id : {Id}");

        var request = new GetItemRequest{
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id" , new AttributeValue { N = Id.ToString()} }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item is null || response.Item.Count == 0) return null;

        return new Products
        {
            Id = response.Item.ContainsKey("id") && response.Item["id"].N != null ? int.Parse(response.Item["id"].N) : 0,
            Title = response.Item.GetValueOrDefault("title")?.S ?? string.Empty,
            Description = response.Item.GetValueOrDefault("description")?.S ?? string.Empty,
            Category = response.Item.GetValueOrDefault("category")?.S ?? string.Empty,
            Price = response.Item.ContainsKey("price") && response.Item["price"].N != null ? decimal.Parse(response.Item["price"].N) : 0m,
            DiscountPercentage = response.Item.ContainsKey("discountPercentage") && response.Item["discountPercentage"].N != null ? decimal.Parse(response.Item["discountPercentage"].N) : 0m,
            Rating = response.Item.ContainsKey("rating") && response.Item["rating"].N != null ? decimal.Parse(response.Item["rating"].N) : 0m,
            Stock = response.Item.ContainsKey("stock") && response.Item["stock"].N != null ? int.Parse(response.Item["stock"].N) : 0,
            Brand = response.Item.GetValueOrDefault("brand")?.S ?? string.Empty,
            Sku = response.Item.GetValueOrDefault("sku")?.S ?? string.Empty,
            Weight = response.Item.ContainsKey("weight") && response.Item["weight"].N != null ? decimal.Parse(response.Item["weight"].N) : 0m,
            WarrantyInformation = response.Item.GetValueOrDefault("warrantyInformation")?.S ?? string.Empty,
            ShippingInformation = response.Item.GetValueOrDefault("shippingInformation")?.S ?? string.Empty,
            AvailabilityStatus = response.Item.GetValueOrDefault("availabilityStatus")?.S ?? string.Empty,
            ReturnPolicy = response.Item.GetValueOrDefault("returnPolicy")?.S ?? string.Empty,
            MinimumOrderQuantity = response.Item.ContainsKey("minimumOrderQuantity") && response.Item["minimumOrderQuantity"].N != null ? int.Parse(response.Item["minimumOrderQuantity"].N) : 0,
            Thumbnail = response.Item.GetValueOrDefault("thumbnail")?.S ?? string.Empty,
            Tags = response.Item.ContainsKey("tags") && response.Item["tags"].L != null ? response.Item["tags"].L.Select(x => x.S).ToList() : new List<string>(),
            Images = response.Item.ContainsKey("images") && response.Item["images"].L != null ? response.Item["images"].L.Select(x => x.S).ToList() : new List<string>(),
            Dimensions = response.Item.ContainsKey("dimensions") && response.Item["dimensions"].M != null ? new Dimensions
            {
                Width = response.Item["dimensions"].M.ContainsKey("width") ? decimal.Parse(response.Item["dimensions"].M["width"].N) : 0m,
                Height = response.Item["dimensions"].M.ContainsKey("height") ? decimal.Parse(response.Item["dimensions"].M["height"].N) : 0m,
                Depth = response.Item["dimensions"].M.ContainsKey("depth") ? decimal.Parse(response.Item["dimensions"].M["depth"].N) : 0m
            } : null,


            Meta = response.Item.ContainsKey("meta") && response.Item["meta"].M != null ? new Meta
            {
                CreatedAt = response.Item["meta"].M.ContainsKey("createdAt") ? DateTime.Parse(response.Item["meta"].M["createdAt"].S) : DateTime.MinValue,
                UpdatedAt = response.Item["meta"].M.ContainsKey("updatedAt") ? DateTime.Parse(response.Item["meta"].M["updatedAt"].S) : DateTime.MinValue,
                Barcode = response.Item["meta"].M.GetValueOrDefault("barcode")?.S ?? string.Empty,
                QrCode = response.Item["meta"].M.GetValueOrDefault("qrCode")?.S ?? string.Empty
            } : null,


            Reviews = response.Item.ContainsKey("reviews") && response.Item["reviews"].L != null
                    ? response.Item["reviews"].L.Select(r => new Review
                    {
                        Rating = r.M.ContainsKey("rating") ? int.Parse(r.M["rating"].N) : 0,
                        Comment = r.M.GetValueOrDefault("comment")?.S ?? string.Empty,
                        Date = r.M.ContainsKey("date") ? DateTime.Parse(r.M["date"].S) : DateTime.MinValue,
                        ReviewerName = r.M.GetValueOrDefault("reviewerName")?.S ?? string.Empty,
                        ReviewerEmail = r.M.GetValueOrDefault("reviewerEmail")?.S ?? string.Empty
                    }).ToList() : new List<Review>()
        };
    }

    


}
