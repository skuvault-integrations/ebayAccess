using System;
using System.Net;

namespace EbayAccess.Models.GetOrdersResponse
{
	public partial class Transaction
	{
		public Buyer Buyer { get; set; }
		public Item Item { get; set; }
		public int QuantityPurchased { get; set; }
		public string TransactionId { get; set; }
		public decimal TransactionPrice { get; set; }
		public ebayCurrency CurrencyId { get; set; }
		public string OrderLineItemId { get; set; }
		public Variation Variation { get; set; }
		public decimal TotalTaxAmount { get; set; }
		public ebayCurrency TotalTaxAmountCurrencyId { get; set; }
		public ShippingDetails ShippingDetails { get; set; }
		public ShippingServiceSelected ShippingServiceSelected { get; set; }
		public DateTime CreatedDate { get; set; }
	}

	public static class TransactionExtensions
	{
		public static EbaySku GetSku( this Transaction transaction )
		{
			if( transaction.Variation != null && !string.IsNullOrWhiteSpace( transaction.Variation.Sku ) )
				return new EbaySku( Decode( transaction.Variation.Sku ), true );

			if( transaction.Item != null && !string.IsNullOrWhiteSpace( transaction.Item.Sku ) )
				return new EbaySku( Decode( transaction.Item.Sku ), false );

			return new EbaySku( string.Empty, false );
		}

		private static string Decode( string sku )
		{
			if( string.IsNullOrEmpty( sku ) )
				return sku;

			return WebUtility.HtmlDecode( sku );
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