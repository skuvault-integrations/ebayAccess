using System;

namespace EbayAccess.Models.ReviseFixedPriceItemResponse
{
	public class Item
	{
		public long ItemId { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}