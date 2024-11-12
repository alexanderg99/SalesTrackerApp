namespace HabitTracker;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
public class DatabaseConnection
{
    public string? Dbpath { get; set; }
    private readonly string _connectionString;
    public DatabaseConnection(string dbPath)
    {

        Dbpath = dbPath;
        _connectionString = new SqliteConnectionStringBuilder { DataSource = Dbpath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
        
    }
    
    

    public void Delete(int orderNumber)
    {
        using var connection = new SqliteConnection(_connectionString);
        var command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM SalesRecords 
                                WHERE OrderNumber = @OrderNumber"; 
        command.Parameters.AddWithValue("@OrderNumber", orderNumber);
        command.ExecuteNonQuery();

    }
    
    
    public void Insert(SalesRecordInput.SalesRecord record)
    {
        using var connection = new SqliteConnection(_connectionString);
        Console.WriteLine($"Database file exists: {File.Exists(Dbpath)}");
        connection.Open();
        var insertSql = @"
            INSERT INTO SalesRecords (
                OrderNumber,
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
                @NegotiationDate,
                @CustomerName,
                @ProductName,
                @Price,
                @QuantitySold,
                @LastDeliveryDate,
                @QuantityDelivered,
                @RemainingQuantity
            );";
        

        try
        {
            using var command = new SqliteCommand(insertSql, connection);
            
            command.Parameters.AddWithValue("@OrderNumber", record.OrderNumber);
            command.Parameters.AddWithValue("@NegotiationDate", record.NegotiationDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@CustomerName", record.CustomerName);
            command.Parameters.AddWithValue("@ProductName", record.ProductName);
            command.Parameters.AddWithValue("@Price", record.Price);
            command.Parameters.AddWithValue("@QuantitySold", record.QuantitySold);
            command.Parameters.AddWithValue("@LastDeliveryDate", record.LastDeliveryDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@QuantityDelivered", record.QuantityDelivered);
            command.Parameters.AddWithValue("@RemainingQuantity", record.RemainingQuantity);

            command.ExecuteNonQuery();
           
        }
        catch (SqliteException ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
            if (ex.SqliteErrorCode == 19) // SQLite UNIQUE constraint failed
            {
                Console.WriteLine("A record with this Order Number already exists.");
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            
        }
        
        
        
        
        
    }


}