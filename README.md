# Blacus.ServiceNowTableAPIWrapper

A lightweight, dependency-free .NET library for wrapping the ServiceNow Table API. It simplifies interacting with ServiceNow tables by providing a strongly-typed, intuitive interface that abstracts away the complexities of the underlying HTTP requests and data transformations.

## Features

*   **Fluent Interface**: A simple and readable API for querying ServiceNow tables.
*   **Strongly-Typed Models**: Map ServiceNow tables to your C# classes.
*   **Attribute-Based Mapping**: Use attributes like `[TableName]` to define table mappings.
*   **Custom JSON Converters**: Automatically handles ServiceNow-specific data types, like reference fields (`link`/`value` pairs).
*   **Automatic Pagination**: Automatically handles paginated results from the ServiceNow API.
*   **Zero Dependencies**: The library has no external dependencies.
*   **Caching**: Optional caching layer to reduce redundant API calls.

## Getting Started

1.  Add the `Blacus.ServiceNowTableAPIWrapper` library to your project.
2.  Define your C# models to represent the ServiceNow tables you want to work with.
3.  Use the `ServiceNowTableClient` to interact with the API.

## Defining a Model

Create a class for each ServiceNow table you want to query. The class must implement the `IServiceNowTableModel` interface, which requires a `SysId` property. Use the `[TableName]` attribute to specify the ServiceNow table name.

Here is an example of a model for the `sys_user` table:

```csharp
using Blacus.ServiceNowTableAPIWrapper;

[TableName("sys_user")]
public class SysUser : IServiceNowTableModel
{
    [JsonPropertyName("sys_id")]
    public string SysId { get; set; }

    [JsonPropertyName("user_name")]
    public string UserName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    // Example of a reference field
    [JsonPropertyName("department")]
    [JsonConverter(typeof(ReferenceLinkConverter<CmnDepartment>))]
    public ReferenceLink<CmnDepartment> Department { get; set; }
}

[TableName("cmn_department")]
public class CmnDepartment : IServiceNowTableModel
{
    [JsonPropertyName("sys_id")]
    public string SysId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
```

## Usage

Instantiate the `ServiceNowTableClient` with your ServiceNow instance details and credentials. Then, use the provided methods to query for records.

```csharp
using Blacus.ServiceNowTableAPIWrapper;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var credentials = new ServiceNowCredential(
            "https://your-instance.service-now.com",
            "your-username",
            "your-password"
        );

        var client = new ServiceNowTableClient(credentials);

        // Example: Get all active users
        var query = new QueryParameters<SysUser>()
            .Where("active", "true")
            .Limit(10);

        try
        {
            var response = await client.GetAsync(query);

            foreach (var user in response.Result)
            {
                Console.WriteLine($"User: {user.UserName}, Email: {user.Email}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
```

## Authentication

The library uses Basic Authentication. You provide your ServiceNow instance URL, username, and password when creating the `ServiceNowCredential` object.

## License

This project is licensed under the terms of the MIT license. See the [LICENSE](LICENSE) file for details.
