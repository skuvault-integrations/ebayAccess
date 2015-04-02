using System;
using System.Collections.Generic;
using System.Globalization;
using EbayAccess.Misc;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	[ Serializable ]
	public partial class Item : ISerializableMnual
	{
		public decimal BuyItNowPrice { get; set; }

		public string BuyItNowPriceCurrencyId { get; set; }

		public string ItemId { get; set; }

		public long Quantity { get; set; }

		public SellingStatus SellingStatus { get; set; }

		public string Title { get; set; }

		public string Sku { get; set; }

		public List< Variation > Variations { get; set; }
		public string Duration { get; set; }

		public string ToJson()
		{
			var id = string.IsNullOrWhiteSpace( this.ItemId ) ? PredefinedValues.NotAvailable : this.ItemId;

			var sku = PredefinedValues.NotAvailable;
			try
			{
				var variationSku = this.GetSku().Sku;
				sku = string.IsNullOrWhiteSpace( variationSku ) ? PredefinedValues.NotAvailable : variationSku;
			}
			catch
			{
			}

			var qty = this.Quantity.ToString( CultureInfo.InvariantCulture );
			var res = string.Format( "{{id:{0},sku:'{1}',qty:{2}}}", id, sku, qty );
			return res;
		}
	}
}