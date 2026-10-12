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

        int? limit = null;

        if (input.ValueKind == JsonValueKind.Object &&
        input.TryGetProperty("limit", out var limitElement) &&
        limitElement.ValueKind == JsonValueKind.Number)
        {
           limit = limitElement.GetInt32();

        }

        var products = await GetProducts(context, limit);

        return products;

    }

    public async Task<List<Products>> GetProducts(
        ILambdaContext context, int? limit = null)
    {
        try
        {
            context.Logger.LogLine("Getting products from DynamoDB...");
            var request= new ScanRequest();

            if (limit is null)
            {
                request = new ScanRequest
                {
                    TableName = tableName,
                   

                };
            }

            else
            {
                 request = new ScanRequest
                {
                    TableName = tableName,
                    Limit = limit

                };
            }

           

           

            var response = await _dynamoDb.ScanAsync(request);

            

            var products = new List<Products>();

            foreach (var item in response.Items)
            {
                

                products.Add(new Products
                {
                    Id =   int.Parse(item["id"].N) ,
                    Title = item["title"].S,
                    Description = item["description"].S,
                    Category = item["category"].S,
                    Price = decimal.Parse(item["price"].N),
                    DiscountPercentage = decimal.Parse(item["discountPercentage"].N),
                    Rating = decimal.Parse(item["rating"].N) ,
                    Stock =  int.Parse(item["stock"].N),
                    Brand = item.GetValueOrDefault("brand")?.S ?? string.Empty,
                    Sku = item["sku"].S,
                    Weight = decimal.Parse(item["weight"].N) ,
                    WarrantyInformation = item["warrantyInformation"].S ,
                    ShippingInformation = item["shippingInformation"].S ,
                    AvailabilityStatus = item["availabilityStatus"].S ,
                    ReturnPolicy = item["returnPolicy"].S,
                    MinimumOrderQuantity = int.Parse(item["minimumOrderQuantity"].N),
                    Thumbnail = item["thumbnail"].S,
                    Tags =  item["tags"].L.Select(x => x.S).ToList(),
                    Images = item["images"].L.Select(x => x.S).ToList() ,
                    Dimensions =  new Dimensions
                    {
                        Width = decimal.Parse(item["dimensions"].M["width"].N),
                        Height =  decimal.Parse(item["dimensions"].M["height"].N) ,
                        Depth =  decimal.Parse(item["dimensions"].M["depth"].N) 
                    } ,

                    
                    Meta =  new Meta
                    {
                        CreatedAt =  DateTime.Parse(item["meta"].M["createdAt"].S) ,
                        UpdatedAt =  DateTime.Parse(item["meta"].M["updatedAt"].S) ,
                        Barcode = item["meta"].M["barcode"].S,
                        QrCode = item["meta"].M["qrCode"].S
                    } ,

                    
                    Reviews = item["reviews"].L.Select(r => new Review
                    {
                         Rating =  int.Parse(r.M["rating"].N) ,
                         Comment = r.M["comment"].S ,
                         Date =  DateTime.Parse(r.M["date"].S),
                         ReviewerName = r.M["reviewerName"].S ,
                         ReviewerEmail = r.M["reviewerEmail"].S 
                    }).ToList()
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
            Id = int.Parse(response.Item["id"].N),
            Title = response.Item["title"].S,
            Description = response.Item["description"].S,
            Category = response.Item["category"].S,
            Price = decimal.Parse(response.Item["price"].N),
            DiscountPercentage = decimal.Parse(response.Item["discountPercentage"].N),
            Rating = decimal.Parse(response.Item["rating"].N),
            Stock = int.Parse(response.Item["stock"].N),
            Brand = response.Item.GetValueOrDefault("brand")?.S ?? string.Empty,
            Sku = response.Item["sku"].S,
            Weight = decimal.Parse(response.Item["weight"].N),
            WarrantyInformation = response.Item["warrantyInformation"].S,
            ShippingInformation = response.Item["shippingInformation"].S,
            AvailabilityStatus = response.Item["availabilityStatus"].S,
            ReturnPolicy = response.Item["returnPolicy"].S,
            MinimumOrderQuantity = int.Parse(response.Item["minimumOrderQuantity"].N),
            Thumbnail = response.Item["thumbnail"].S,
            Tags = response.Item["tags"].L.Select(x => x.S).ToList(),
            Images = response.Item["images"].L.Select(x => x.S).ToList(),
            Dimensions = new Dimensions
            {
                Width = decimal.Parse(response.Item["dimensions"].M["width"].N),
                Height = decimal.Parse(response.Item["dimensions"].M["height"].N),
                Depth = decimal.Parse(response.Item["dimensions"].M["depth"].N)
            },


            Meta = new Meta
            {
                CreatedAt = DateTime.Parse(response.Item["meta"].M["createdAt"].S),
                UpdatedAt = DateTime.Parse(response.Item["meta"].M["updatedAt"].S),
                Barcode = response.Item["meta"].M["barcode"].S,
                QrCode = response.Item["meta"].M["qrCode"].S
            },


            Reviews = response.Item["reviews"].L.Select(r => new Review
            {
                Rating = int.Parse(r.M["rating"].N),
                Comment = r.M["comment"].S,
                Date = DateTime.Parse(r.M["date"].S),
                ReviewerName = r.M["reviewerName"].S,
                ReviewerEmail = r.M["reviewerEmail"].S
            }).ToList()
        };
    }

    


}
