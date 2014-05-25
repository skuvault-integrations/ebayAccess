using System;

namespace EbayAccess.Models.GetSellerListResponse
{
	[ Serializable ]
	public class Variation
	{
		public string Sku { get; set; }
		public int Quantity { get; set; }
		public int QuantitySold { get; set; }
		public decimal StartPrice { get; set; }
		public string Currency { get; set; }
	}
}