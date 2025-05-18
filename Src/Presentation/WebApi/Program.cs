using Microsoft.Data.SqlClient;
using TaskMaster.ServiceDefaults;
using WebApi.Apis;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services DI
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();
builder.Services.AddAuthorizationBuilder();

var withApiVersioning = builder.Services.AddApiVersioning();
builder.AddDefaultOpenApi(withApiVersioning);



builder.Services.AddHealthChecks();


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var taskList = app.NewVersionedApi("TaskList");
taskList.TaskListApiV1().RequireAuthorization();


var taskItem = app.NewVersionedApi("TaskItem");
taskItem.TaskItemApiV1().RequireAuthorization();

var dashboard = app.NewVersionedApi("Dashboard");
dashboard.DashboardApiV1().RequireAuthorization();


//app.MapGet("/connectforecast", () =>
//{

//    string returnMessage = "";

//    IConfigurationRoot configuration = new ConfigurationBuilder()
//         .SetBasePath(Directory.GetCurrentDirectory())
//         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//         .Build();

//    // Read the connection string from the configuration
//    string connectionString = configuration.GetConnectionString("SqlServerConnection");


//    // The SQL query you want to execute
//    string sqlQuery = "SELECT [FullName] FROM [TaskMaster].[dbo].[Users]";

//    using (SqlConnection connection = new SqlConnection(connectionString))
//    {
//        try
//        {
//            // Open the connection
//            connection.Open();
//            returnMessage = "Successfully connected to the database.";

//            // Create a SQL command object
//            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
//            {
//                // Execute the query and get a data reader
//                using (SqlDataReader reader = command.ExecuteReader())
//                {
//                    // Check if there are any rows returned
//                    if (reader.HasRows)
//                    {
//                        while (reader.Read())
//                        {
//                            returnMessage += reader["FullName"].ToString();
//                        }

//                    }
//                    else
//                    {
//                        Console.WriteLine("\nNo rows were returned by the query.");
//                    }
//                }
//            }
//        }
//        catch (SqlException ex)
//        {
//            returnMessage = $"Error connecting to or querying the database: {ex.Message}";
//        }
//        finally
//        {
//            // Ensure the connection is closed, even if an error occurred
//            if (connection.State == System.Data.ConnectionState.Open)
//            {
//                connection.Close();
//                Console.WriteLine("Connection closed.");
//            }
//        }
//    }
//    return returnMessage;

//})
//.WithName("ConnectForecast");



app.MapHealthChecks("_health");

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();


public partial class Program { }