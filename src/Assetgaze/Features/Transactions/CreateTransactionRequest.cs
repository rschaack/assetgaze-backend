namespace Assetgaze.Features.Transactions;

public class CreateTransactionRequest
{
    public string Ticker { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Currency { get; set; }
}