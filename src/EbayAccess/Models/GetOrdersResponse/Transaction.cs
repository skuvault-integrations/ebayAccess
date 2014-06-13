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
		public EbaySku GetSku()
		{
			if( this.Variation != null && !string.IsNullOrWhiteSpace( this.Variation.Sku ) )
				return new EbaySku( this.Variation.Sku, true );

			if( this.Item != null && !string.IsNullOrWhiteSpace( this.Item.Sku ) )
				return new EbaySku( this.Item.Sku, false );

			return new EbaySku( string.Empty, false );
		}
	}

	public class EbaySku
	{
		public EbaySku( string sku, bool isVariationSku )
		{
			this.Sku = sku;
			this.IsVariationSku = isVariationSku;
		}

		public string Sku { get; set; }
		public bool IsVariationSku { get; set; }
	}
}