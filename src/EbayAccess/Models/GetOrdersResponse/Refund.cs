using System;

namespace EbayAccess.Models.GetOrdersResponse
{
	public class Refund
	{
		public decimal RefundAmount { get; set; }
		public string RefundAmountCurrencyID { get; set; }
		public DateTime RefundTime { get; set; }
		public string RefundStatus { get; set; }
	}
}