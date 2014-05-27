using System;
using System.Collections.Generic;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	[ Serializable ]
	public partial class Item
	{
		public decimal BuyItNowPrice { get; set; }

		public string BuyItNowPriceCurrencyId { get; set; }

		public string ItemId { get; set; }

		public long Quantity { get; set; }

		public SellingStatus SellingStatus { get; set; }

		public string Title { get; set; }

		public string Sku { get; set; }

		public List< Variation > Variations { get; set; }
	}
}