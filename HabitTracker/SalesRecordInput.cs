namespace HabitTracker;

using System;
using System.Globalization;

public class SalesRecordInput
{
    public class SalesRecord
    {
        public int OrderNumber { get; set; }
        public DateTime NegotiationDate { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public int QuantitySold { get; set; }
        public DateTime LastDeliveryDate { get; set; }
        public int QuantityDelivered { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public static SalesRecord GetSalesRecordFromConsole()
    {
        var record = new SalesRecord();

        try
        {
            Console.WriteLine("Enter Sales Record Details:");
            Console.WriteLine("---------------------------");

            while (true)
            {
                Console.Write("Enter Order Number: ");
                if (int.TryParse(Console.ReadLine(), out int orderNumber))
                {
                    record.OrderNumber = orderNumber;
                    break;
                }

                Console.WriteLine("Invalid Order Number");
            }

            while (true)
            {
                Console.Write("Enter Negotiation Date: ");
                if (DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime negDate))
                {
                    record.NegotiationDate = negDate;
                    break;
                }

                Console.WriteLine("Invalid Negotiation Date");
            }

            while (string.IsNullOrEmpty(record.CustomerName))
            {
                Console.WriteLine("Enter Customer Name: ");
                record.CustomerName = Console.ReadLine()?.Trim();
                break;
            }
            
            do
            {
                Console.Write("Product Name: ");
                record.ProductName = Console.ReadLine()?.Trim();
            } while (string.IsNullOrEmpty(record.ProductName));

            while (true)
            {
                Console.Write("Price");
                if (int.TryParse(Console.ReadLine(), out int price))
                {
                    record.Price = price;
                    break;
                }
                Console.WriteLine("Invalid Price");
            }
            
            while (true)
            {
                Console.Write("Quantity Sold: ");
                if (int.TryParse(Console.ReadLine(), out int quantitySold) && quantitySold > 0)
                {
                    record.QuantitySold = quantitySold;
                    break;
                }
                Console.WriteLine("Please enter a valid positive integer for Quantity Sold.");
            }

            // Last Delivery Date
            while (true)
            {
                Console.Write("Last Delivery Date (DD/MM/YYYY): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastDeliveryDate))
                {
                    record.LastDeliveryDate = lastDeliveryDate;
                    break;
                }
                Console.WriteLine("Please enter a valid date in DD/MM/YYYY format.");
            }
            
            while (true)
            {
                Console.Write("Quantity Delivered: ");
                if (int.TryParse(Console.ReadLine(), out int quantityDelivered) && 
                    quantityDelivered >= 0 && quantityDelivered <= record.QuantitySold)
                {
                    record.QuantityDelivered = quantityDelivered;
                    break;
                }
                Console.WriteLine($"Please enter a valid number between 0 and {record.QuantitySold}.");
            }
            
            record.RemainingQuantity = record.QuantitySold - record.QuantityDelivered;

            return record;
            


        }


        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }

        return record;
    }

}