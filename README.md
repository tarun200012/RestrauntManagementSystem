📖 Setup Instructions – RestaurantAPI Project

Follow these steps to run the project locally:

1️⃣ Unzip the Project named "RestrauntManagement"

Extract the zipped project to any folder on your machine.

Open the solution (RestrauntManagement.sln) 

2️⃣ Configure Database Connection

You need to update the SQL Server connection string in two places with your local SQL Server name and no need to change database name as we will run a script also to create database

🔹 A) appsettings.json

Go to RestaurantAPI/appsettings.json and update:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=RestaurantDb;Trusted_Connection=True;TrustServerCertificate=True;"
}


Replace:

YOUR_SERVER_NAME → your SQL Server instance (e.g., TARUNGUPTA1\\MSSQLSERVER14)


🔹 B) ApplicationDbContextFactory.cs

Go to RestaurantAPI/Data/ApplicationDbContextFactory.cs and update the same connection string:

private const string ConnectionString =
    "Server=YOUR_SERVER_NAME;Database=RestaurantDb;Trusted_Connection=True;TrustServerCertificate=True;";

3️⃣ Create Database from Script

Locate the SQL script file RestaurantAPI/DatabaseTestScript/DatabaseScript.sql provided in the project.

Open SQL Server Management Studio (SSMS).

Connect to your SQL Server instance.

Run the script – it will create the database and all tables and seed some data.

4️⃣ Run Backend

In Visual Studio, set RestaurantAPI project.

Press F5 to run.

API will start at: http://localhost:5112

5️⃣ Run Frontend

Open the frontend project folder named "resturant-app" in VS Code or terminal.

Install dependencies:

npm install


Start Angular dev server:

ng serve -o


Frontend will run at: http://localhost:4200

6️⃣ Login & Test

The backend already has JWT + RSA keys configured in appsettings.json.

After both frontend & backend are running, you can log in and test the app.

Login Credentials:

Role Admin :
email - admin@example.com
password - abc123

Role Customer :
email - john@example.com
password - abc123
