using System;

namespace EbayAccess.Models.GetSellerListResponse
{
	[ Serializable ]
	public class SellingStatus
	{
		public decimal CurrentPrice { get; set; }

		public string CurrentPriceCurrencyId { get; set; }
	}
}