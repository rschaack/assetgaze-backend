using Assetgaze.Domain;
using LinqToDB.Mapping;

namespace Assetgaze.Features.Transactions;


// In: src/Assetgaze/Transaction.cs

// Add this using

[Table("Transactions")] // Maps this class to the "Transactions" table
public class Transaction
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Column("TransactionType"), NotNull]
    public TransactionType TransactionType { get; set; }
    
    [Column("BrokerDealReference")]
    public string? BrokerDealReference { get; set; }
    
    [Column("BrokerId"), NotNull]
    public Guid BrokerId { get; set; }
    
    [Column("AccountId"), NotNull]
    public Guid AccountId { get; set; }

    [Column("TaxWrapper"), NotNull]
    public TaxWrapper TaxWrapper { get; set; }
    
    [Column("ISIN"), NotNull] 
    public string ISIN { get; set; } = string.Empty; // This is how we represent the asset for now
    
    [Column("TransactionDate"), NotNull]
    public DateTime TransactionDate { get; set; }
    
    [Column("Quantity")]
    public decimal? Quantity { get; set; }

    [Column("NativePrice")]
    public decimal? NativePrice { get; set; }
    
    [Column("LocalPrice")]
    public decimal? LocalPrice { get; set; }
    
    [Column("Consideration"), NotNull]
    public decimal Consideration { get; set; }
    
    [Column("BrokerCharge")]
    public decimal? BrokerCharge { get; set; }
    
    [Column("StampDuty")]
    public decimal? StampDuty { get; set; }
    
    [Column("FxCharge")]
    public decimal? FxCharge { get; set; }
    
    [Column("AccruedInterest")]
    public decimal? AccruedInterest { get; set; }
}