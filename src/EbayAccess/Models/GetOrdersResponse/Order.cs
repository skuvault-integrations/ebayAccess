using System;
using System.Collections.Generic;
using System.Linq;

namespace EbayAccess.Models.GetOrdersResponse
{
	public sealed partial class Order
	{
		public string OrderId { get; set; }

		public EbayOrderStatusEnum Status { get; set; }

		public CheckoutStatus CheckoutStatus { get; set; }

		public DateTime CreatedTime { get; set; }

		public string PaymentMethods { get; set; }

		public List< Transaction > TransactionArray { get; set; }

		public string BuyerUserId { get; set; }

		public ShippingAddress ShippingAddress { get; set; }

		public DateTime PaidTime { get; set; }

		public DateTime ShippedTime { get; set; }

		public decimal Total
		{
			get { return this.TransactionArray.Aggregate( 0m, ( ac, item ) => ac + ( decimal )item.TransactionPrice * ( decimal )item.QuantityPurchased ); }
		}

		public ShippingDetails ShippingDetails { get; set; }
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