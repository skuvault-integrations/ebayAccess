using System;

namespace EbayAccess.Models.GetOrdersResponse
{
	public class CheckoutStatus
	{
		public string EBayPaymentStatus { get; set; }

		public DateTime LastModifiedTime { get; set; }

		public string PaymentMethod { get; set; }

		public string Status { get; set; }

		public bool? IntegratedMerchantCreditCardEnabled { get; set; }
	}
}