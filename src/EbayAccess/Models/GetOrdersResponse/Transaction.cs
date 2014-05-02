namespace EbayAccess.Models.GetOrdersResponse
{
	public class Transaction
	{
		public Buyer Buyer { get; set; }

		public Item Item { get; set; }

		public int QuantityPurchased { get; set; }

		public int TransactionId { get; set; }

		public double TransactionPrice { get; set; }

		public string CurrencyId { get; set; }

		public string OrderLineItemId { get; set; }

		public Variation Variation { get; set; }
	}
}