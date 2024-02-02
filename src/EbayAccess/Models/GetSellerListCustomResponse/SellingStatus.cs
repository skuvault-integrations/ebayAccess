using System;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	[ Serializable ]
	public class SellingStatus
	{
		public decimal CurrentPrice { get; set; }

		public string CurrentPriceCurrencyId { get; set; }

		public int QuantitySold { get; set; }
		
		public ListingStatusCodeTypeEnum ListingStatus { get; set; }
	}
}