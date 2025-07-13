namespace Assetgaze.Features.Transactions.DTOs;

public class UpdateTransactionRequest
{    
    public DateTime TransactionDate { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? NativePrice { get; set; }
    public decimal Consideration { get; set; }
    public decimal? BrokerCharge { get; set; }
    public decimal? StampDuty { get; set; }
}