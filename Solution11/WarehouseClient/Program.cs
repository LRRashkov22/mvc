using WarehouseClient.Models;
using WarehouseClient.Services;

Console.WriteLine("WAREHOUSE REST API CLIENT");
Console.WriteLine("API address: https://localhost:7001/");
Console.WriteLine();

using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://localhost:7001/")
};
var api = new WarehouseApiClient(httpClient);

while (true)
{
    Console.WriteLine("1. Register");
    Console.WriteLine("2. Login");
    Console.WriteLine("3. List products (requires login)");
    Console.WriteLine("4. Create product (requires login)");
    Console.WriteLine("0. Exit");
    Console.Write("Choose: ");
    var choice = Console.ReadLine();
    Console.WriteLine();

    try
    {
        switch (choice)
        {
            case "1":
            {
                var error = await api.RegisterAsync(ReadCredentials());
                Console.WriteLine(error ?? "Registration successful. Log in to use protected endpoints.");
                break;
            }
            case "2":
            {
                var error = await api.LoginAsync(ReadCredentials());
                Console.WriteLine(error ?? "Login successful. JWT access token stored in HttpClient.");
                break;
            }
            case "3":
            {
                var products = await api.GetProductsAsync();
                if (products.Count == 0) Console.WriteLine("No products found.");
                foreach (var product in products)
                    Console.WriteLine($"#{product.ProductId} | {product.Name} | {product.Price:C} | Category: {product.CategoryId}");
                break;
            }
            case "4":
            {
                Console.Write("Product name: ");
                var name = Console.ReadLine() ?? string.Empty;
                Console.Write("Price: ");
                if (!decimal.TryParse(Console.ReadLine(), out var price))
                {
                    Console.WriteLine("Invalid price.");
                    break;
                }
                Console.Write("Category ID: ");
                if (!int.TryParse(Console.ReadLine(), out var categoryId))
                {
                    Console.WriteLine("Invalid category ID.");
                    break;
                }
                var error = await api.CreateProductAsync(new ProductDto
                {
                    Name = name,
                    Price = price,
                    CategoryId = categoryId
                });
                Console.WriteLine(error ?? "Product created.");
                break;
            }
            case "0": return;
            default: Console.WriteLine("Unknown option."); break;
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Could not reach the API: {ex.Message}");
        Console.WriteLine("Check that WarehouseApi is running and its HTTPS certificate is trusted.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    Console.WriteLine();
}

static Credentials ReadCredentials()
{
    Console.Write("Username: ");
    var username = Console.ReadLine() ?? string.Empty;
    Console.Write("Password: ");
    var password = ReadPassword();
    return new Credentials { Username = username, Password = password };
}

static string ReadPassword()
{
    var chars = new List<char>();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) break;
        if (key.Key == ConsoleKey.Backspace && chars.Count > 0)
        {
            chars.RemoveAt(chars.Count - 1);
            continue;
        }
        if (!char.IsControl(key.KeyChar)) chars.Add(key.KeyChar);
    }
    Console.WriteLine();
    return new string(chars.ToArray());
}
