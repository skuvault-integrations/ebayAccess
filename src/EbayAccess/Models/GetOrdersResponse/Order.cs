using System;
using System.Collections.Generic;

namespace EbayAccess.Models.GetOrdersResponse
{
	public sealed class Order
	{
		public string OrderId { get; set; }

		public OrderStatus Status { get; set; }

		public CheckoutStatus CheckoutStatus { get; set; }

		public DateTime CreatedTime { get; set; }

		public string PaymentMethods { get; set; }

		public List< Transaction > TransactionArray { get; set; }

		public string BuyerUserId { get; set; }
	}

	public enum OrderStatus
	{
		Undefined,
		Active,
		Cancelled,
		Completed,
		Inactive,
		Shipped
	}
}