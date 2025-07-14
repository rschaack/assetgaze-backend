using Assetgaze.Domain;

namespace Assetgaze.Features.Transactions.DTOs;

public class UpdateTransactionRequest
{    
    public TransactionType TransactionType { get; set; }
    public string? BrokerDealReference { get; set; }
    public Guid BrokerId { get; set; }
    public Guid AccountId { get; set; }
    public TaxWrapper TaxWrapper { get; set; }
    public string ISIN { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal NativePrice { get; set; }
    public decimal LocalPrice { get; set; }
    public decimal Consideration { get; set; }
    public decimal? BrokerCharge { get; set; }
    public decimal? StampDuty { get; set; }
    public decimal? FxCharge { get; set; }
    public decimal? AccruedInterest { get; set; }
}