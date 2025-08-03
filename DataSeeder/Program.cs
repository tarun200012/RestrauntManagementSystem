using DataSeeder;

internal partial class Program
{
    public static async Task Main(string[] args)
    {
        string connectionString = "Server=TARUNGUPTA1\\MSSQLSERVER14;Database=RestaurantDb;Trusted_Connection=True;TrustServerCertificate=True;";

        Console.Write("Enter scale (e.g. 0.1, 0.5, 1.0): ");
        //string input = Console.ReadLine();
        //double scale = string.IsNullOrWhiteSpace(input) ? 1.0 : Math.Clamp(Convert.ToDouble(input), 0.01, 10);

        var seeder = new SqlBulkSeeder(connectionString);
        await seeder.SeedAsync();

        Console.WriteLine("✅ Seeding completed.");
    }
}
