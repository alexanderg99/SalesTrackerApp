// See https://aka.ms/new-console-template for more information


using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
using HabitTracker;
using Spectre.Console;
using System;
using System.Data;
using System.IO;



//string solDir = Directory.GetCurrentDirectory();
//string dbPath = Path.Combine(solDir, "mydb.db");

class Program
{

    public static async Task displayTable(SalesDataDisplay dd)
    {
        await dd.DisplaySalesTableAsync();
        await dd.ShowSalesDashboardAsync();
    }

    public static void IngestCsvToSqlite(string csvFilePath, string sqliteDbPath)
    {
        using (var connection = new SqliteConnection($"Data Source={sqliteDbPath}"))
        {
            connection.Open();

            using (var reader = new StreamReader(csvFilePath))
            using (var csvParser = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                csvParser.Read(); // Skip header row
                csvParser.ReadHeader();
                
                var indexNumbers = new Dictionary<string, int>();

                while (csvParser.Read())
                {
                    string orderNumber = csvParser.GetField<string>("S/O");
                    int indexNumber = indexNumbers.GetValueOrDefault(orderNumber, 0);
                    indexNumbers[orderNumber] = indexNumber + 1;

                    using (var insertCommand = connection.CreateCommand())
                    {
                        insertCommand.CommandText = @"
                                INSERT INTO SalesRecords (
                                    OrderNumber,
                                    IndexNumber,
                                    NegotiationDate,
                                    CustomerName,
                                    ProductName,
                                    Price,
                                    QuantitySold,
                                    LastDeliveryDate,
                                    QuantityDelivered,
                                    RemainingQuantity
                                ) VALUES (
                                    @OrderNumber,
                                    @IndexNumber,
                                    @NegotiationDate,
                                    @CustomerName,
                                    @ProductName,
                                    @Price,
                                    @QuantitySold,
                                    @LastDeliveryDate,
                                    @QuantityDelivered,
                                    @RemainingQuantity
                                );";
                        insertCommand.Parameters.AddWithValue("@OrderNumber", csvParser.GetField<string>("S/O"));
                        insertCommand.Parameters.AddWithValue("@IndexNumber", indexNumber);
                        //insertCommand.Parameters.AddWithValue("@NegotiationDate", csvParser.GetField<string>("Date"));
                        insertCommand.Parameters.AddWithValue("@NegotiationDate","01/01/1970");
                        insertCommand.Parameters.AddWithValue("@CustomerName", csvParser.GetField<string>("Customer"));
                        insertCommand.Parameters.AddWithValue("@ProductName", csvParser.GetField<string>("Item"));
                        insertCommand.Parameters.AddWithValue("@Price", csvParser.GetField<string>("Price: USD/tonne"));
                        insertCommand.Parameters.AddWithValue("@QuantitySold", csvParser.GetField<string>("Quantity"));
                        //insertCommand.Parameters.AddWithValue("@LastDeliveryDate", csvParser.GetField<string>("LastUpdated"));
                        insertCommand.Parameters.AddWithValue("@LastDeliveryDate", "01/01/1999");
                        insertCommand.Parameters.AddWithValue("@QuantityDelivered", csvParser.GetField<string>("Dealt Quantity"));
                        insertCommand.Parameters.AddWithValue("@RemainingQuantity", csvParser.GetField<string>("Outstanding/MT"));
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    static void Main(string[] args)
    {
        string dbPath = "/Users/alexandergunawan/RiderProjects/HabitTracker/HabitTracker/Sales.db";
        string csvPath = "/Users/alexandergunawan/RiderProjects/HabitTracker/HabitTracker/masterfilecopy.csv";
        Console.WriteLine($"db path: {dbPath}");
        bool endapp = false;
        DatabaseConnection db = new DatabaseConnection(dbPath);
        SalesDataDisplay dataDisplay = new SalesDataDisplay(dbPath);
        
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS SalesRecords (
            OrderNumber TEXT NOT NULL,
            IndexNumber INTEGER DEFAULT 0,
            NegotiationDate TEXT,
            CustomerName TEXT,
            ProductName TEXT,
            Price INTEGER DEFAULT 0,
            QuantitySold INTEGER,
            LastDeliveryDate TEXT,
            QuantityDelivered INTEGER, 
            RemainingQuantity INTEGER,
            PRIMARY KEY (OrderNumber, IndexNumber)
        );";
        command.ExecuteNonQuery();

        while (!endapp)
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("\ti - Insert Sales Record");
            Console.WriteLine("\td - Delete Sales Record");
            Console.WriteLine("\tu - Update Sales Record");
            Console.WriteLine("\tv - View Sales Record");
            Console.WriteLine("\ts - Stop Application");
            Console.WriteLine("\tk - Display Sales Data");
            Console.Write("Your option? ");
            string? userInput = Console.ReadLine();
            
            if (userInput == null || ! Regex.IsMatch(userInput, "[i|d|u|v|s|k]"))
            {
                Console.WriteLine("Error: Unrecognized input.");
            }

            else
            {
                try
                {
                    if (userInput == "i")
                    {
                        Console.WriteLine("Insert a habit");
                        var salesRecord = SalesRecordInput.GetSalesRecordFromConsole();
                        if (salesRecord != null)
                        {
                            db.Insert(salesRecord);
                        }
                        
                    }
                    
                    if (userInput == "u")
                    {
                        IngestCsvToSqlite(csvPath, dbPath);
                        
                    }
                    
                    else if (userInput == "d")
                    {
                        command.CommandText = "DROP TABLE IF EXISTS SalesRecords;";
                        command.ExecuteNonQuery();
                    }
                    
                    else if (userInput == "s")
                    {
                        endapp = true;
                    }
                    
                    else if (userInput == "k")
                    {
                        displayTable(dataDisplay);
                    }
                    
                    
                    
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }
            
            
        }

        
    }
}




