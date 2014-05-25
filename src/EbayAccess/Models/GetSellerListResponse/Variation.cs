using System;

namespace EbayAccess.Models.GetSellerListResponse
{
	[Serializable]
	public class Variation
	{
		public string Sku { get; set; }
		public uint Quantity { get; set; }
		public uint QuantitySold { get; set; }
		public decimal StartPrice { get; set; }
		public string Currency { get; set; }
	}
}