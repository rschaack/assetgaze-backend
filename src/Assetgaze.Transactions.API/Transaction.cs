namespace Assetgaze.Transactions.API;


// In: src/Assetgaze.Transactions.API/Transaction.cs
using LinqToDB.Mapping; // Add this using

[Table("Transactions")] // Maps this class to the "Transactions" table
public class Transaction
{
    [PrimaryKey, Identity] // Defines the primary key
    public Guid Id { get; set; }

    [Column("Ticker"), NotNull] // Maps this property to the "Ticker" column
    public string Ticker { get; set; }

    [Column("Quantity")]
    public int Quantity { get; set; }

    [Column("Price")]
    public decimal Price { get; set; }

    [Column("TransactionType"), NotNull]
    public string TransactionType { get; set; }

    [Column("TransactionDate")]
    public DateTime TransactionDate { get; set; }
    
    [Column("Currency"), NotNull]
    public string Currency { get; set; }
}