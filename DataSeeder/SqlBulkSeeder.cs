using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace DataSeeder
{
    public class SqlBulkSeeder
    {
        private readonly string _connectionString;

        public SqlBulkSeeder(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SeedAsync()
        {
            Console.WriteLine("🚀 Starting Bulk Seeding...");

            await BulkInsertAsync("Locations", GenerateLocations(1_000_000)); 
            await BulkInsertAsync("Restaurants", GenerateRestaurants(1_000_000));
            await BulkInsertAsync("MenuItems", GenerateMenuItems(10_000_000));
            await BulkInsertAsync("Customers", GenerateCustomers(1_000_000));
            await BulkInsertAsync("Orders", GenerateOrders(500_000));
            await BulkInsertAsync("OrderItems", GenerateOrderItems(1_500_000));

            Console.WriteLine("✅ Seeding Complete.");
        }

        private async Task BulkInsertAsync(string tableName, DataTable table)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = tableName,
                BulkCopyTimeout = 0
            };

            await bulkCopy.WriteToServerAsync(table);
            Console.WriteLine($"✔ Inserted {table.Rows.Count} into {tableName}");
        }

        private DataTable GenerateLocations(int count)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("Country", typeof(string));
            dt.Columns.Add("Address", typeof(string));

            for (int i = 1; i <= count; i++)
            {
                dt.Rows.Add(i, $"City-{i}", "UP", "India", $"Address-{i}");
            }
            return dt;
        }

        private DataTable GenerateRestaurants(int count)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Mobile", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("LocationId", typeof(int));
            dt.Columns.Add("IsDeleted", typeof(bool));
            dt.Columns.Add("OpenTime", typeof(TimeSpan));
            dt.Columns.Add("CloseTime", typeof(TimeSpan));

            var open = new TimeSpan(11, 0, 0);
            var close = new TimeSpan(21, 0, 0);

            for (int i = 1; i <= count; i++)
            {
                dt.Rows.Add(i, $"Restaurant-{i}", $"Desc-{i}", $"9999{i:D6}", $"r{i}@mail.com", i, false, open, close);
            }

            return dt;
        }

        private DataTable GenerateMenuItems(int count)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Price", typeof(decimal));
            dt.Columns.Add("RestaurantId", typeof(int));
            dt.Columns.Add("IsDeleted", typeof(bool));

            var rand = new Random();

            for (int i = 1; i <= count; i++)
            {
                dt.Rows.Add(i, $"Item-{i}", $"Desc-{i}", rand.Next(100, 1000), (i % 1_000_000) + 1, false);
            }

            return dt;
        }

        private DataTable GenerateCustomers(int count)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("IsDeleted", typeof(bool));

            for (int i = 1; i <= count; i++)
            {
                dt.Rows.Add(i, $"Customer-{i}", $"c{i}@mail.com", $"98877{i % 10000:D4}", false);
            }

            return dt;
        }

        private DataTable GenerateOrders(int count)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("CustomerId", typeof(int));
            dt.Columns.Add("RestaurantId", typeof(int));
            dt.Columns.Add("ScheduledAt", typeof(DateTime));
            dt.Columns.Add("IsConfirmed", typeof(bool));

            var rand = new Random();
            var slotTracker = new Dictionary<(int RestaurantId, DateTime HourSlot), int>();

            int maxRestaurants = 1_000_000;
            int maxCustomers = 1_000_000;

            var openTime = new TimeSpan(11, 0, 0);
            var closeTime = new TimeSpan(21, 0, 0);

            int orderId = 1;

            int confirmedTarget = (int)(count * 0.85);
            int cancelledTarget = count - confirmedTarget;
            int i = 1;
            int j = 1;
            while (dt.Rows.Count < confirmedTarget)
            {
                var restaurantId = (i % 1_000_000) +1;
                var customerId = (i % 1_000_000) + 1;

                // Random future day (1 to 30 days)
                var futureDate = DateTime.UtcNow.Date.AddDays(rand.Next(1, 31));

                // Random hour in open window (11 to 20)
                var hour = rand.Next(openTime.Hours, closeTime.Hours); // up to 20, because slot is 1 hour

                var slot = futureDate.AddHours(hour); // this is the scheduled time (start of hour)
                var key = (restaurantId, slot);

                if (!slotTracker.ContainsKey(key))
                    slotTracker[key] = 0;

                if (slotTracker[key] < 10)
                {
                    slotTracker[key]++;
                    dt.Rows.Add(orderId++, customerId, restaurantId, slot, true);
                }
                j++;
                i= (j%5) == 0 ? i + 1 : i; // Increment restaurant every 5 iterations
                // else skip and retry to avoid violating the 10-per-hour rule
            }
            // Step 2: Add cancelled orders scheduled OUTSIDE valid time
            while (dt.Rows.Count < count)
            {
                var restaurantId = (i % 1_000_000) + 1;
                var customerId = (i % 1_000_000) + 1;
                var futureDate = DateTime.UtcNow.Date.AddDays(rand.Next(1, 31));

                // Intentionally select hour before 11 or after 20
                int invalidHour = rand.Next(0, 2) == 0
                    ? rand.Next(0, 10)      // before open
                    : rand.Next(22, 24);   // after close

                var invalidSlot = futureDate.AddHours(invalidHour);

                dt.Rows.Add(orderId++, customerId, restaurantId, invalidSlot, false);
                j++;
                i = (j % 5) == 0 ? i + 1 : i;
            }

            return dt;

           
        }


        private DataTable GenerateOrderItems(int count)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("OrderId", typeof(int));
            dt.Columns.Add("MenuItemId", typeof(int));
            dt.Columns.Add("Quantity", typeof(int));

            var rand = new Random();

            for (int i = 1; i <= count; i++)
            {
                dt.Rows.Add(i, (i % 5_000_000) + 1, (i % 10_000_000) + 1, rand.Next(1, 5));
            }

            return dt;
        }
    }
}