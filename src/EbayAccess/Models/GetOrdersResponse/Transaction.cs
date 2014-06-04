namespace EbayAccess.Models.GetOrdersResponse
{
	public partial class Transaction
	{
		public Buyer Buyer { get; set; }

		public Item Item { get; set; }

		public int QuantityPurchased { get; set; }

		public int TransactionId { get; set; }

		public decimal TransactionPrice { get; set; }

		public string CurrencyId { get; set; }

		public string OrderLineItemId { get; set; }

		public Variation Variation { get; set; }
	}

	public partial class Transaction
	{
		public string GetSku()
		{
			if( this.Item != null && !string.IsNullOrWhiteSpace( this.Item.Sku ) )
				return this.Item.Sku;

			if( this.Variation != null && !string.IsNullOrWhiteSpace( this.Variation.Sku ) )
				return this.Variation.Sku;

			return string.Empty;
		}
	}
}