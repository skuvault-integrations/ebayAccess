using System;
using System.Collections.Generic;

namespace EbayAccess.Models.GetOrdersResponse
{
	public sealed partial class Order
	{
		public string OrderId { get; set; }

		public EbayOrderStatusEnum Status { get; set; }

		public CheckoutStatus CheckoutStatus { get; set; }

		public CancelStatusEnum CancelStatus { get; set; }

		public DateTime CreatedTime { get; set; }

		public string PaymentMethods { get; set; }

		public List< Transaction > TransactionArray { get; set; }

		public string BuyerUserId { get; set; }

		public ShippingAddress ShippingAddress { get; set; }

		public DateTime PaidTime { get; set; }

		public DateTime ShippedTime { get; set; }

		public decimal Total { get; set; }

		public ShippingDetails ShippingDetails { get; set; }

		public MonetaryDetails MonetaryDetails { get; set; }

		public decimal Subtotal { get; set; }
	}

	public enum CancelStatusEnum
	{
		Undefined,
		CancelComplete,
		CancelFailed,
		CancelPending
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