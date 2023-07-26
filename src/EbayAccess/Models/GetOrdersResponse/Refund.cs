using System;

namespace EbayAccess.Models.GetOrdersResponse
{
	public class Refund
	{
		public DateTime RefundTime { get; set; }
		public RefundStatus RefundStatus { get; set; }
	}
}