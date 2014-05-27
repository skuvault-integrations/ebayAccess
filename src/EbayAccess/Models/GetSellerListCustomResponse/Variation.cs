using System;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	[ Serializable ]
	public class Variation
	{
		public string Sku { get; set; }
		public decimal StartPrice { set; get; }
		public string StartPriceCurrencyId { get; set; }
		public int Quantity { get; set; }
		public SellingStatus SellingStatus { get; set; }
	}
}