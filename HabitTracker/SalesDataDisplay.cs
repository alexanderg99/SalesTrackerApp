using System.Data;

namespace HabitTracker;

using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


public class SalesDataDisplay
{
    private readonly string _connectionString;

    public SalesDataDisplay(string connectionString)
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = connectionString, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
    }

    private async Task<List<SalesRecordInput.SalesRecord>> GetSalesRecordAsync()
    {
        var records = new List <SalesRecordInput.SalesRecord>();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM SalesRecords ORDER BY OrderNumber DESC";
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            records.Add(new SalesRecordInput.SalesRecord
            {
                OrderNumber = reader.GetInt32(0),
                NegotiationDate = reader.GetDateTime(2),
                CustomerName = reader.GetString(3),
                ProductName = reader.GetString(4),
                Price = reader.GetInt32(5),
                QuantitySold = reader.GetInt32(6),
                LastDeliveryDate = reader.GetDateTime(7),
                QuantityDelivered = reader.GetInt32(8),
                RemainingQuantity = reader.GetInt32(9)
            });
        }

        return records;
    }

    public async Task DisplaySalesTableAsync()
    {
        var records = await GetSalesRecordAsync();
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[yellow]Sales Records[/]")
            .AddColumn(new TableColumn("[blue]Order #[/]").Centered())
            .AddColumn(new TableColumn("[blue]Customer[/]"))
            .AddColumn(new TableColumn("[blue]Product[/]"))
            .AddColumn(new TableColumn("[blue]Price[/]").RightAligned())
            .AddColumn(new TableColumn("[blue]Quantity[/]").RightAligned())
            .AddColumn(new TableColumn("[blue]Delivered[/]").RightAligned())
            .AddColumn(new TableColumn("[blue]Remaining[/]").RightAligned());
        
        foreach (var record in records)
        {
            table.AddRow(
                record.OrderNumber.ToString(),
                record.CustomerName,
                record.ProductName,
                $"${record.Price:N2}",
                record.QuantitySold.ToString(),
                record.QuantityDelivered.ToString(),
                $"[{(record.RemainingQuantity > 0 ? "red" : "green")}]{record.RemainingQuantity}[/]"
            );
        }

        AnsiConsole.Write(table);
        
    }

    public async Task DisplaySalesBarChartAsync()
    {
        var records = await GetSalesRecordAsync();
        
        // Create a bar chart of sales by product
        var salesByProduct = records
            .GroupBy(r => r.ProductName)
            .Select(g => new { Product = g.Key, Total = g.Sum(r => r.Price * r.QuantitySold) })
            .OrderByDescending(x => x.Total)
            .Take(5);  // Top 5 products

        var chart = new BarChart()
            .Width(60)
            .Label("[yellow]Top 5 Products by Sales Value[/]")
            .CenterLabel();

        foreach (var item in salesByProduct)
        {
            chart.AddItem(item.Product, item.Total, Color.Blue);
        }

        AnsiConsole.Write(chart);
    }
    
    public async Task ShowSalesDashboardAsync()
    {
        // Clear the console
        AnsiConsole.Clear();

        // Create a layout
        var layout = new Layout("Root")
            .SplitRows(
                new Layout("Top")
                    .SplitColumns(
                        new Layout("Stats"),
                        new Layout("Chart")),
                new Layout("Table"));

        // Get the data
        var records = await GetSalesRecordAsync();
        
        // Add content to each panel
        await Task.WhenAll(
            AddSummaryStatsAsync(layout["Stats"], records),
            AddBarChartAsync(layout["Chart"], records),
            AddSalesTableAsync(layout["Table"], records)
        );

        // Render the layout
        AnsiConsole.Write(layout);
    }
    
    
    private async Task AddSummaryStatsAsync(Layout layout, List<SalesRecordInput.SalesRecord> records)
    {
        var panel = new Panel(
            Align.Center(
                new Rows(
                    new Text($"Total Orders: {records.Count}"),
                    new Text($"Total Revenue: ${records.Sum(r => r.Price * r.QuantitySold):N2}"),
                    new Text($"Pending Deliveries: {records.Count(r => r.RemainingQuantity > 0)}")
                )
            ))
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1),
            Header = new PanelHeader("[yellow]Summary Statistics[/]")
        };

        layout.Update(panel);
    }
    private async Task AddBarChartAsync(Layout layout, List<SalesRecordInput.SalesRecord> records)
    {
        var chart = new BarChart()
            .Width(40)
            .Label("[yellow]Sales by Product[/]")
            .CenterLabel();

        var salesByProduct = records
            .GroupBy(r => r.ProductName)
            .Select(g => new { Product = g.Key, Total = g.Sum(r => r.Price * r.QuantitySold) })
            .OrderByDescending(x => x.Total)
            .Take(5);

        foreach (var item in salesByProduct)
        {
            chart.AddItem(item.Product, item.Total, Color.Blue);
        }

        layout.Update(chart);
    }
    
    
    private async Task AddSalesTableAsync(Layout layout, List<SalesRecordInput.SalesRecord> records)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[yellow]Recent Sales[/]")
            .AddColumn(new TableColumn("[blue]Order #[/]").Centered())
            .AddColumn(new TableColumn("[blue]Customer[/]"))
            .AddColumn(new TableColumn("[blue]Product[/]"))
            .AddColumn(new TableColumn("[blue]Total[/]").RightAligned());

        foreach (var record in records.Take(5))  // Show only last 5 records
        {
            table.AddRow(
                record.OrderNumber.ToString(),
                record.CustomerName,
                record.ProductName,
                $"${record.Price * record.QuantitySold:N2}"
            );
        }

        layout.Update(table);
    }
    
    
    
    
    
   
}