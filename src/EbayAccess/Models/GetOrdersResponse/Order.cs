using System;
using System.Collections.Generic;

namespace EbayAccess.Models.GetOrdersResponse
{
	public sealed class Order
	{
		public string OrderId { get; set; }

		public EbayOrderStatusEnum Status { get; set; }

		public CheckoutStatus CheckoutStatus { get; set; }

		public DateTime CreatedTime { get; set; }

		public string PaymentMethods { get; set; }

		public List< Transaction > TransactionArray { get; set; }

		public string BuyerUserId { get; set; }

		public ShippingAddress ShippingAddress { get; set; }
	}

	public enum EbayOrderStatusEnum
	{
		Undefined,
		Active,
		Cancelled,
		Completed,
		Inactive,
		Shipped
	}
}